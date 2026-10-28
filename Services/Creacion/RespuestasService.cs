using Infrastructure.Data;
using Messages.Creacion;
using Messages.Creacion.In;
using Messages.Creacion.Out;
using Messages.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Services.Creacion;

/// <summary>
/// Ver <see cref="IRespuestasService"/> y <see cref="RespuestasPersonaje"/>. Las decisiones del guion llegan de la pantalla con lo que dan
/// (<see cref="EfectosFicha"/>); mientras el personaje está en creación y la pantalla lo pide (<c>aplicar</c>, la sesión de creación),
/// cada una se escribe en la ficha según llega y se deshace si cambia o se quita (<see cref="AplicadorFicha"/>). Fuera de ese modo,
/// «Aplicar a la ficha» a mano hace lo mismo con las pendientes. Lo aplicado se recuerda por fila, así nada se suma dos veces.
/// </summary>
public class RespuestasService(CosmereContext db) : IRespuestasService
{
    /// <summary>Descripción de las metas que crea una decisión: marca las que se pueden borrar al deshacerla.</summary>
    private const string MarcaMeta = "De la sesión de creación.";

    public async Task<List<RespuestaResponse>> GetAsync(long campaignId, long characterId, long userId)
    {
        var member = await db.CampaignMembers.AsNoTracking()
            .FirstOrDefaultAsync(m => m.CampaignId == campaignId && m.UserId == userId)
            ?? throw new KeyNotFoundException("Campaign not found.");
        var personaje = await db.Characters.AsNoTracking()
            .Where(c => c.Id == characterId && c.CampaignId == campaignId)
            .Select(c => new { c.OwnerId })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Character not found.");
        var esGm = member.Role == "gm";
        if (!esGm && personaje.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only view your own character.");

        var filas = db.CharacterRespuestas.AsNoTracking().Where(r => r.CharacterId == characterId);
        if (!esGm) filas = filas.Where(r => r.Origen == RespuestasPersonaje.OrigenCreacion);
        return await Ordenadas(filas).Select(r => RespuestasPersonaje.MapToResponse(r)).ToListAsync();
    }

    public async Task<List<RespuestaResponse>> PutDecisionesAsync(long campaignId, long characterId, DecisionesPersonajeRequest request, long userId)
    {
        var campaign = await ExigirDirectorAsync(campaignId, userId);
        var personaje = await db.Characters.FirstOrDefaultAsync(c => c.Id == characterId && c.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Character not found.");

        // Claves normalizadas (con el prefijo «decision:») y únicas: una respuesta por pregunta y personaje.
        var pedidas = request.Decisiones.Select(d => (clave: ClaveDecision(d.Clave), d)).ToList();
        var repetidas = pedidas.GroupBy(x => x.clave).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (repetidas.Count > 0)
            throw new ArgumentException($"Claves de decisión repetidas: {string.Join(", ", repetidas)}.");

        // Origen según la sesión de creación: en curso, las decisiones son de la creación, y la pantalla debe estar en esa sesión de mesa.
        var origen = RespuestasPersonaje.OrigenSesion;
        if (campaign.SesionCreacionEstado == SesionCreacionEstados.EnCurso)
        {
            var mesaId = (request.SesionId ?? "").Trim();
            if (campaign.SesionCreacionMesaId != "" && mesaId != campaign.SesionCreacionMesaId)
                throw new ArgumentException("La sesión de mesa de la pantalla no es la sesión de creación registrada en la campaña.");
            origen = RespuestasPersonaje.OrigenSesionCreacion;
        }

        var ahora = DateTime.UtcNow;
        var auto = request.Aplicar && !personaje.CreacionCompletada;
        var lineas = new List<string>();
        var ignorado = new List<string>();
        var metasNuevas = new HashSet<string>();
        var filas = await db.CharacterRespuestas
            .Where(r => r.CharacterId == characterId && r.TipoPregunta == RespuestasPersonaje.TipoDecision)
            .ToListAsync();
        var claves = pedidas.Select(x => x.clave).ToHashSet();
        foreach (var sobrante in filas.Where(f => !claves.Contains(f.Clave)).ToList())
        {
            await RevertirAsync(personaje, sobrante, lineas);
            db.CharacterRespuestas.Remove(sobrante);
            filas.Remove(sobrante);
        }
        foreach (var (clave, d) in pedidas)
        {
            var fila = filas.FirstOrDefault(f => f.Clave == clave);
            if (fila is null)
            {
                fila = new CharacterRespuestaEntity
                {
                    CharacterId = characterId, Clave = clave, TipoPregunta = RespuestasPersonaje.TipoDecision, CreatedAt = ahora, RespondidaEn = ahora,
                };
                db.CharacterRespuestas.Add(fila);
                filas.Add(fila);
            }
            var respuesta = (d.Respuesta ?? "").Trim();
            var cambia = fila.Respuesta != respuesta;
            if (cambia)
            {
                fila.Respuesta = respuesta;
                fila.RespondidaEn = ahora;
            }
            fila.Pregunta = string.IsNullOrWhiteSpace(d.Pregunta) ? (fila.Pregunta == "" ? clave : fila.Pregunta) : d.Pregunta.Trim();
            fila.TipoRespuesta = string.IsNullOrWhiteSpace(d.TipoRespuesta) ? RespuestasPersonaje.RespuestaOpcion : d.TipoRespuesta.Trim();
            fila.Detalle = RespuestasPersonaje.SerializarDetalle(d.Detalle);
            fila.Origen = origen;
            fila.SesionId = (request.SesionId ?? "").Trim();
            fila.SesionTitulo = (request.SesionTitulo ?? "").Trim();

            var pedido = d.Efectos ?? new EfectosFicha();
            var registro = AplicadorFicha.LeerAplicado(fila.Aplicado);
            if (auto)
            {
                // Ya escrita con lo mismo: nada que hacer; si pide otra cosa, se deshace lo anterior y se escribe lo nuevo
                if (registro is not null && AplicadorFicha.MismoPedido(registro.Pedido, pedido)) continue;
                if (registro is not null) await RevertirAsync(personaje, fila, lineas);
                await AplicarFilaAsync(personaje, fila, pedido, ahora, lineas, ignorado, metasNuevas);
            }
            else if (registro is not null && (cambia || !AplicadorFicha.MismoPedido(registro.Pedido, pedido)))
            {
                // Aplicada a mano y ahora distinta: se deshace y vuelve a quedar pendiente
                await RevertirAsync(personaje, fila, lineas);
            }
        }
        if (auto) AplicadorFicha.Recomendar(personaje, request.CaminoRecomendado ?? "", request.MetalRecomendado ?? "", lineas);
        if (lineas.Count > 0) personaje.UpdatedAt = ahora;
        await db.SaveChangesAsync();

        return await RespuestasDeAsync(characterId);
    }

    public async Task<AplicarRespuestasResponse> AplicarAsync(long campaignId, long characterId, AplicarRespuestasRequest request, long userId)
    {
        await ExigirDirectorAsync(campaignId, userId);
        var personaje = await db.Characters.FirstOrDefaultAsync(c => c.Id == characterId && c.CampaignId == campaignId && !c.IsNpc)
            ?? throw new KeyNotFoundException("Character not found.");
        var pedidas = request.Filas.Select(f => (clave: ClaveDecision(f.Clave), f.Efectos ?? new EfectosFicha())).ToList();
        if (pedidas.Count == 0) throw new ArgumentException("No hay decisiones que aplicar.");
        var claves = pedidas.Select(x => x.clave).ToList();
        var filas = await db.CharacterRespuestas
            .Where(r => r.CharacterId == characterId && r.TipoPregunta == RespuestasPersonaje.TipoDecision && claves.Contains(r.Clave))
            .ToListAsync();

        var ahora = DateTime.UtcNow;
        var lineas = new List<string>();
        var ignorado = new List<string>();
        var metasNuevas = new HashSet<string>();
        var aplicadas = 0;
        foreach (var (clave, efectos) in pedidas)
        {
            var fila = filas.FirstOrDefault(f => f.Clave == clave);
            if (fila is null) { ignorado.Add($"Decisión desconocida: {clave}"); continue; }
            if (fila.AplicadaEn is not null) continue;
            await AplicarFilaAsync(personaje, fila, efectos, ahora, lineas, ignorado, metasNuevas);
            aplicadas++;
        }
        if (aplicadas == 0) throw new InvalidOperationException("Esas decisiones ya están aplicadas o no existen.");
        AplicadorFicha.Recomendar(personaje, request.CaminoRecomendado ?? "", request.MetalRecomendado ?? "", lineas);
        personaje.UpdatedAt = ahora;
        await db.SaveChangesAsync();

        return new AplicarRespuestasResponse { Respuestas = await RespuestasDeAsync(characterId), Aplicado = lineas, Ignorado = ignorado };
    }

    // ── Aplicar y deshacer una fila ──────────────────────────────────────────

    private async Task AplicarFilaAsync(CharacterEntity personaje, CharacterRespuestaEntity fila, EfectosFicha pedido, DateTime ahora,
        List<string> lineas, List<string> ignorado, HashSet<string> metasNuevas)
    {
        var hecho = AplicadorFicha.AplicarFila(personaje, pedido, lineas, ignorado);
        foreach (var titulo in pedido.Metas.Select(m => m.Trim()).Where(m => m != "").Distinct())
        {
            if (metasNuevas.Contains(titulo)) continue;
            if (await db.Metas.AnyAsync(m => m.CharacterId == personaje.Id && m.Estado == "activa" && m.Titulo == titulo)) continue;
            db.Metas.Add(new MetaEntity { CharacterId = personaje.Id, Titulo = titulo, Descripcion = MarcaMeta });
            metasNuevas.Add(titulo);
            hecho.Metas.Add(titulo);
            lineas.Add($"Meta «{titulo}»");
        }
        fila.Aplicado = AplicadorFicha.EscribirAplicado(new AplicadorFicha.RegistroAplicado(pedido, hecho));
        fila.AplicadaEn = ahora;
    }

    /// <summary>Deshace lo que escribió una fila aplicada (si lo fue) y la deja pendiente.</summary>
    private async Task RevertirAsync(CharacterEntity personaje, CharacterRespuestaEntity fila, List<string> lineas)
    {
        var registro = AplicadorFicha.LeerAplicado(fila.Aplicado);
        if (registro is not null)
        {
            AplicadorFicha.RevertirFila(personaje, registro.Hecho, lineas);
            foreach (var titulo in registro.Hecho.Metas)
            {
                var meta = await db.Metas.FirstOrDefaultAsync(m => m.CharacterId == personaje.Id && m.Estado == "activa" && m.Titulo == titulo && m.Descripcion == MarcaMeta);
                if (meta is null) continue;
                db.Metas.Remove(meta);
                lineas.Add($"Meta «{titulo}» quitada");
            }
        }
        fila.Aplicado = "{}";
        fila.AplicadaEn = null;
    }

    // ── Ayudantes ───────────────────────────────────────────────────────────

    private async Task<CampaignEntity> ExigirDirectorAsync(long campaignId, long userId)
    {
        var esGm = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");
        if (!esGm) throw new UnauthorizedAccessException("Only the GM can perform this action.");
        return await db.Campaigns.AsNoTracking().FirstOrDefaultAsync(c => c.Id == campaignId)
            ?? throw new KeyNotFoundException("Campaign not found.");
    }

    private Task<List<RespuestaResponse>> RespuestasDeAsync(long characterId) =>
        Ordenadas(db.CharacterRespuestas.AsNoTracking().Where(r => r.CharacterId == characterId))
            .Select(r => RespuestasPersonaje.MapToResponse(r)).ToListAsync();

    private static string ClaveDecision(string clave)
    {
        var c = (clave ?? "").Trim();
        if (c == "") throw new ArgumentException("Una decisión sin clave.");
        return c.StartsWith(RespuestasPersonaje.PrefijoDecision, StringComparison.Ordinal) ? c : RespuestasPersonaje.PrefijoDecision + c;
    }

    private static IOrderedQueryable<CharacterRespuestaEntity> Ordenadas(IQueryable<CharacterRespuestaEntity> filas) =>
        filas.OrderBy(r => r.RespondidaEn).ThenBy(r => r.Id);
}
