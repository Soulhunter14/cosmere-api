using Infrastructure.Data;
using Messages.Creacion;
using Messages.Creacion.In;
using Messages.Creacion.Out;
using Messages.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Services.Creacion;

/// <summary>Ver <see cref="IRespuestasService"/> y <see cref="RespuestasPersonaje"/>.</summary>
public class RespuestasService(CosmereContext db) : IRespuestasService
{
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
        var esGm = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");
        if (!esGm) throw new UnauthorizedAccessException("Only the GM can perform this action.");
        var campaign = await db.Campaigns.AsNoTracking().FirstOrDefaultAsync(c => c.Id == campaignId)
            ?? throw new KeyNotFoundException("Campaign not found.");
        var existe = await db.Characters.AnyAsync(c => c.Id == characterId && c.CampaignId == campaignId);
        if (!existe) throw new KeyNotFoundException("Character not found.");

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
        var filas = await db.CharacterRespuestas
            .Where(r => r.CharacterId == characterId && r.TipoPregunta == RespuestasPersonaje.TipoDecision)
            .ToListAsync();
        var claves = pedidas.Select(x => x.clave).ToHashSet();
        foreach (var sobrante in filas.Where(f => !claves.Contains(f.Clave)).ToList())
        {
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
            if (fila.Respuesta != respuesta)
            {
                fila.Respuesta = respuesta;
                fila.RespondidaEn = ahora;
                fila.AplicadaEn = null;
            }
            fila.Pregunta = string.IsNullOrWhiteSpace(d.Pregunta) ? (fila.Pregunta == "" ? clave : fila.Pregunta) : d.Pregunta.Trim();
            fila.TipoRespuesta = string.IsNullOrWhiteSpace(d.TipoRespuesta) ? RespuestasPersonaje.RespuestaOpcion : d.TipoRespuesta.Trim();
            fila.Detalle = RespuestasPersonaje.SerializarDetalle(d.Detalle);
            fila.Origen = origen;
            fila.SesionId = (request.SesionId ?? "").Trim();
            fila.SesionTitulo = (request.SesionTitulo ?? "").Trim();
        }
        await db.SaveChangesAsync();

        return await Ordenadas(db.CharacterRespuestas.AsNoTracking().Where(r => r.CharacterId == characterId))
            .Select(r => RespuestasPersonaje.MapToResponse(r)).ToListAsync();
    }

    private static string ClaveDecision(string clave)
    {
        var c = (clave ?? "").Trim();
        if (c == "") throw new ArgumentException("Una decisión sin clave.");
        return c.StartsWith(RespuestasPersonaje.PrefijoDecision, StringComparison.Ordinal) ? c : RespuestasPersonaje.PrefijoDecision + c;
    }

    private static IOrderedQueryable<CharacterRespuestaEntity> Ordenadas(IQueryable<CharacterRespuestaEntity> filas) =>
        filas.OrderBy(r => r.RespondidaEn).ThenBy(r => r.Id);
}
