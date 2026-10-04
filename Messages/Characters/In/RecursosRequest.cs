namespace Messages.Characters.In;

/// <summary>
/// Cuerpo de <c>PATCH …/characters/{id}/recursos</c> (estado de mesa de «Nacidos de la bruma»; lo usa T13). Solo se escribe
/// lo que no es nulo.
/// </summary>
public class RecursosRequest
{
    /// <summary>Solo las claves presentes se escriben (<c>investiduraActual</c>, <c>cuentasAtium</c>, <c>arquillas</c>).</summary>
    public Dictionary<string, decimal>? Recursos { get; set; }

    public List<PoderRecursosRequest>? Poderes { get; set; }
}

/// <summary>Estado de mesa de un poder ya existente, identificado por <c>(Arte, Metal)</c>.</summary>
public class PoderRecursosRequest
{
    public required string Arte { get; set; }
    public required string Metal { get; set; }
    public int? Cargas { get; set; }
    public int? Viales { get; set; }
    public bool? Desprovisto { get; set; }
    public bool? Completo { get; set; }

    /// <summary>Componedor: solo <c>&lt;= 0</c>, recortado a <c>&gt;= -(cargasMax sin ajuste)</c> (L.155 / PDF 161).</summary>
    public int? AjusteCargasMax { get; set; }
}

/// <summary>Cuerpo de <c>POST …/acciones/beber-vial</c>: ids de metal del vial (L.129-130 / PDF 135-136).</summary>
public class BeberVialRequest
{
    public List<string> Metales { get; set; } = [];
}

/// <summary>Cuerpo de <c>POST …/acciones/inicio-escena</c> (L.129 / PDF 135).</summary>
public class InicioEscenaRequest
{
    public bool Sorprendido { get; set; }
}
