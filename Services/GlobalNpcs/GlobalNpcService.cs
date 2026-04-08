using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.GlobalNpcs.In;
using Messages.GlobalNpcs.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.GlobalNpcs;

public class GlobalNpcService(CosmereContext db) : IGlobalNpcService
{
    public async Task<List<GlobalNpcResponse>> GetAllAsync() =>
        await db.GlobalNpcs.OrderBy(n => n.Name).Select(n => Map(n)).ToListAsync();

    public async Task<GlobalNpcResponse> GetByIdAsync(long id) =>
        Map(await db.GlobalNpcs.FindAsync(id) ?? throw new KeyNotFoundException("Global NPC not found."));

    public async Task<GlobalNpcResponse> CreateAsync(GlobalNpcRequest request)
    {
        var entity = FromRequest(request);
        db.GlobalNpcs.Add(entity);
        await db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<GlobalNpcResponse> UpdateAsync(long id, GlobalNpcRequest request)
    {
        var entity = await db.GlobalNpcs.FindAsync(id) ?? throw new KeyNotFoundException("Global NPC not found.");
        Apply(entity, request);
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await db.GlobalNpcs.FindAsync(id) ?? throw new KeyNotFoundException("Global NPC not found.");
        db.GlobalNpcs.Remove(entity);
        await db.SaveChangesAsync();
    }

    private static GlobalNpcEntity FromRequest(GlobalNpcRequest r) => new()
    {
        Name = r.Name, Source = r.Source, Tipo = r.Tipo, Ascendencia = r.Ascendencia, Level = r.Level,
        Fuerza = r.Fuerza, Velocidad = r.Velocidad, Intelecto = r.Intelecto,
        Voluntad = r.Voluntad, Discernimiento = r.Discernimiento, Presencia = r.Presencia,
        MaxHealth = r.MaxHealth, MaxConcentration = r.MaxConcentration, MaxInvestiture = r.MaxInvestiture,
        Agilidad = r.Agilidad, ArmasLigeras = r.ArmasLigeras, ArmasPesadas = r.ArmasPesadas,
        Atletismo = r.Atletismo, Hurto = r.Hurto, Sigilo = r.Sigilo,
        Deduccion = r.Deduccion, Disciplina = r.Disciplina, Intimidacion = r.Intimidacion,
        Manufactura = r.Manufactura, Medicina = r.Medicina, Conocimiento = r.Conocimiento,
        Engano = r.Engano, Liderazgo = r.Liderazgo, Percepcion = r.Percepcion,
        Perspicacia = r.Perspicacia, Persuasion = r.Persuasion, Supervivencia = r.Supervivencia,
        Talentos = r.Talentos, Apariencia = r.Apariencia, Notas = r.Notas,
    };

    private static void Apply(GlobalNpcEntity e, GlobalNpcRequest r)
    {
        e.Name = r.Name; e.Source = r.Source; e.Tipo = r.Tipo; e.Ascendencia = r.Ascendencia; e.Level = r.Level;
        e.Fuerza = r.Fuerza; e.Velocidad = r.Velocidad; e.Intelecto = r.Intelecto;
        e.Voluntad = r.Voluntad; e.Discernimiento = r.Discernimiento; e.Presencia = r.Presencia;
        e.MaxHealth = r.MaxHealth; e.MaxConcentration = r.MaxConcentration; e.MaxInvestiture = r.MaxInvestiture;
        e.Agilidad = r.Agilidad; e.ArmasLigeras = r.ArmasLigeras; e.ArmasPesadas = r.ArmasPesadas;
        e.Atletismo = r.Atletismo; e.Hurto = r.Hurto; e.Sigilo = r.Sigilo;
        e.Deduccion = r.Deduccion; e.Disciplina = r.Disciplina; e.Intimidacion = r.Intimidacion;
        e.Manufactura = r.Manufactura; e.Medicina = r.Medicina; e.Conocimiento = r.Conocimiento;
        e.Engano = r.Engano; e.Liderazgo = r.Liderazgo; e.Percepcion = r.Percepcion;
        e.Perspicacia = r.Perspicacia; e.Persuasion = r.Persuasion; e.Supervivencia = r.Supervivencia;
        e.Talentos = r.Talentos; e.Apariencia = r.Apariencia; e.Notas = r.Notas;
    }

    private static GlobalNpcResponse Map(GlobalNpcEntity e) => new()
    {
        Id = e.Id, Name = e.Name, Source = e.Source, Tipo = e.Tipo, Ascendencia = e.Ascendencia, Level = e.Level,
        Fuerza = e.Fuerza, Velocidad = e.Velocidad, Intelecto = e.Intelecto,
        Voluntad = e.Voluntad, Discernimiento = e.Discernimiento, Presencia = e.Presencia,
        MaxHealth = e.MaxHealth, MaxConcentration = e.MaxConcentration, MaxInvestiture = e.MaxInvestiture,
        Agilidad = e.Agilidad, ArmasLigeras = e.ArmasLigeras, ArmasPesadas = e.ArmasPesadas,
        Atletismo = e.Atletismo, Hurto = e.Hurto, Sigilo = e.Sigilo,
        Deduccion = e.Deduccion, Disciplina = e.Disciplina, Intimidacion = e.Intimidacion,
        Manufactura = e.Manufactura, Medicina = e.Medicina, Conocimiento = e.Conocimiento,
        Engano = e.Engano, Liderazgo = e.Liderazgo, Percepcion = e.Percepcion,
        Perspicacia = e.Perspicacia, Persuasion = e.Persuasion, Supervivencia = e.Supervivencia,
        Talentos = e.Talentos, Apariencia = e.Apariencia, Notas = e.Notas,
        ImageUrl = e.ImageUrl, CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt,
    };
}
