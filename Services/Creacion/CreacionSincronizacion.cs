using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Services.Characters;
using Services.Worlds;

namespace Services.Creacion;

/// <summary>
/// Al arrancar, tras las migraciones: la ficha de cada personaje en creación sigue a sus pasos (<see cref="IWorldRules.FichaDeCreacion"/>, aplicado por <see cref="CreacionFicha"/>),
/// también la de quien respondió antes de que existiera la regla (26-10-2026). Idempotente y barata: solo personajes en creación de campañas
/// en preparación, y solo escribe las fichas que difieren de lo que dictan sus pasos.
/// </summary>
public static class CreacionSincronizacion
{
    public static void Sincronizar(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CosmereContext>();
        var mundos = scope.ServiceProvider.GetRequiredService<IWorldRulesProvider>();
        var enCreacion = db.Characters
            .Where(c => !c.CreacionCompletada && !c.IsNpc && c.Campaign.IniciadaEn == null)
            .Select(c => new { Personaje = c, c.Campaign.World })
            .ToList();
        var cambiados = 0;
        foreach (var x in enCreacion)
        {
            var p = x.Personaje;
            var dictado = mundos.Get(x.World).FichaDeCreacion(CharacterJson.ParseCreacionDatos(p.CreacionDatos), p.Ascendencia);
            if (dictado is null) continue;
            if (!CreacionFicha.AplicarAsync(db, p, dictado).GetAwaiter().GetResult()) continue;
            p.UpdatedAt = DateTime.UtcNow;
            cambiados++;
        }
        if (cambiados > 0) db.SaveChanges();
    }
}
