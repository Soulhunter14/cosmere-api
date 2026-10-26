using Messages.Characters;
using Messages.Characters.In;
using Messages.Characters.Out;
using Messages.Database.Entities;
using Messages.Worlds;
using Services.Characters;

namespace Services.Worlds;

/// <summary>
/// Identidad efectiva de un personaje que valida cada mundo (§5.3). Enumera los campos de identidad de los dos mundos
/// (Stormlight: <c>CaminoRadiante</c>; Nacidos de la bruma: <c>CaminoMetal</c>, <c>CaminoInicial</c>, <c>Poderes</c>,
/// <c>Bendiciones</c>, <c>Recursos</c> y, desde T49a, <c>Clavos</c>). Sin BD: la existencia de <c>MetaId</c> la comprueba
/// <c>CharacterService.UpdateCharacterAsync</c>.
/// </summary>
public sealed record IdentidadPersonaje(
    string CaminoHeroico,
    string CaminoRadiante,
    string CaminoMetal,
    string CaminoInicial,
    string Ascendencia,
    IReadOnlyList<PoderPersonaje> Poderes,
    IReadOnlyList<string> Bendiciones,
    IReadOnlyDictionary<string, decimal> Recursos,
    IReadOnlyList<ClavoHemalurgico>? Clavos = null,
    string Legado = "",
    IReadOnlyList<string>? LegadoRespuestas = null);

/// <summary>
/// Reglas de un mundo (ambientación) de campaña. Lo compartido es Cosmere y vive en el núcleo (<c>CharacterService</c>,
/// <c>TalentosReglas</c>); lo específico de cada mundo se resuelve aquí, a través de la campaña. Implementaciones sin
/// estado ni BD (singleton).
/// </summary>
public interface IWorldRules
{
    /// <summary>Id del mundo (<see cref="WorldIds"/>).</summary>
    string Id { get; }

    /// <summary>
    /// Normaliza la era recibida al crear la campaña. Stormlight no tiene era: devuelve <c>null</c> siempre.
    /// Nacidos de la bruma exige <c>era1</c> o <c>era2</c> y lanza <see cref="ArgumentException"/> (400) si no.
    /// </summary>
    string? NormalizarEra(string? era);

    /// <summary>
    /// Reglas de talento propias del mundo (Roshar: potencias, órdenes y cantores; Scadrial: Resistencia koloss).
    /// </summary>
    IReadOnlyDictionary<string, List<ReglaTalento>> ReglasPropias { get; }

    /// <summary>
    /// Reglas efectivas = <c>TalentosReglas.Efectivas(ReglasPropias)</c>: núcleo Cosmere + propias, en el orden del literal
    /// <c>TalentosReglas.Reglas</c> (el orden decide el de las líneas del desglose).
    /// </summary>
    IReadOnlyDictionary<string, List<ReglaTalento>> ReglasTalentos { get; }

    /// <summary>Claves de <c>Recursos</c> que admite el mundo (Stormlight: ninguna).</summary>
    IReadOnlySet<string> RecursosPermitidos { get; }

    /// <summary>
    /// Clave de <c>Recursos</c> que guarda el dinero del personaje (Nacidos de la bruma: arquillas), la que mueve un aporte a la tesorería
    /// de la banda. <c>null</c>: el dinero del mundo no vive en <c>Recursos</c> (Stormlight: sus marcos son columnas propias).
    /// </summary>
    string? ClaveDinero { get; }

    /// <summary>El camino Investido del mundo (radiante o de nacido del metal) solo lo cambia el director.</summary>
    bool CaminoInvestidoLoCambiaElDirector { get; }

    /// <summary>
    /// Pasos de la creación guiada de personajes, en orden, como ids de campo de <c>CreacionGuiada</c> (<c>name</c>, <c>legado</c>,
    /// <c>ascendencia</c>, <c>legadoRespuestas:0</c>…). Vacía: el mundo no tiene creación guiada (Stormlight). Añadir un paso es una línea
    /// aquí y otra en <c>WorldConfig.creacion.pasos</c> del cliente.
    /// </summary>
    IReadOnlyList<string> PasosCreacion { get; }

    /// <summary>
    /// Lo que los pasos de la creación guiada dictan para la ficha mientras el personaje está en creación (26-10-2026): Nacidos de la bruma
    /// deriva de «¿Quieres ser alomante?» y «¿Qué tipo de brumoso?» el camino de nacido del metal, su poder, la habilidad Alomancia, el talento
    /// principal y la meta de nacido del metal, como el flujo «Al elegir camino» de la ficha. <c>null</c> = el mundo no deriva nada de sus
    /// pasos (Stormlight). Lo aplica <c>CreacionFicha</c> en el <c>PUT</c> de personajes y, al arrancar, a los que ya respondieron.
    /// </summary>
    FichaDeCreacion? FichaDeCreacion(IReadOnlyDictionary<string, string> datos, string ascendencia);

    /// <summary>Valida la identidad efectiva del personaje (§5.3); <see cref="ArgumentException"/> → 400.</summary>
    void ValidarIdentidad(IdentidadPersonaje id);

    /// <summary>
    /// Bloqueo no-GM propio del mundo en el <c>PUT</c> (§5.2), tras el del núcleo (<c>Name</c> y <c>CaminoHeroico</c>):
    /// Stormlight conserva <c>CaminoRadiante</c>; Nacidos de la bruma, <c>CaminoMetal</c>, <c>CaminoInicial</c>,
    /// <c>Bendiciones</c>, <c>Clavos</c> (T49a) y la reinyección de poderes.
    /// </summary>
    void RestringirCambiosNoGm(UpdateCharacterRequest request, CharacterEntity character);

    /// <summary>
    /// Acciones de mesa (<c>PATCH recursos</c>, <c>beber-vial</c>, <c>inicio-escena</c>; §5.2). <paramref name="estado"/> es
    /// el <c>MapToResponse</c> previo (total de Investidura, cargas máximas). Stormlight → <see cref="ArgumentException"/> (400).
    /// </summary>
    void AplicarAccionMesa(CharacterEntity c, AccionMesa accion, CharacterResponse estado);

    /// <summary>Tras concluir una meta (Nacidos de la bruma marca completos los poderes enlazados; Stormlight no-op).</summary>
    void AlConcluirMeta(CharacterEntity c, MetaEntity meta);

    /// <summary>Tras borrar una meta (Nacidos de la bruma desenlaza los poderes; Stormlight no-op).</summary>
    void AlBorrarMeta(CharacterEntity c, MetaEntity meta);

    /// <summary>
    /// Al morir el personaje (núcleo: <c>MuertePersonaje</c>): retira de la ficha el equipo propio del mundo y lo devuelve como claves →
    /// valores, que se guardan con el equipo común. Nacidos de la bruma: arquillas, cuentas de atium, viales y cargas; Stormlight: nada.
    /// </summary>
    Dictionary<string, decimal> RetirarEquipo(CharacterEntity c);

    /// <summary>Al volver a la vida: devuelve lo que <see cref="RetirarEquipo"/> guardó (Stormlight: no-op).</summary>
    void DevolverEquipo(CharacterEntity c, IReadOnlyDictionary<string, decimal> guardado);

    /// <summary>
    /// Talentos que el cliente concede automáticamente (<c>autoGranted</c>) y no guarda en <c>c.Talentos</c>, pero que las
    /// reglas del servidor deben contar.
    /// </summary>
    IEnumerable<string> TalentosImplicitos(CharacterEntity c);

    /// <summary>Si el personaje tiene Investidura (decide la línea Base de la Investidura máxima y la condición de regla).</summary>
    bool TieneInvestidura(CharacterEntity c, IReadOnlyList<string> talentos, IReadOnlyList<PoderPersonaje> poderes);

    /// <summary>
    /// Bonos de atributo del personaje y su origen (texto para la etiqueta de la línea de bono). Reutiliza el record
    /// <see cref="BonosForma"/> del WIP sin renombrarlo: representa bonos de atributo de cualquier origen (forma de cantor,
    /// Bendición, talento, clavo), no solo de forma.
    /// </summary>
    BonosForma BonosAtributos(CharacterEntity c, IReadOnlyList<string> talentos, out string? origen);

    /// <summary>Concepto de la línea de bono de atributo en los desgloses (Stormlight: <c>Forma: {origen}</c>).</summary>
    string EtiquetaBono(string? origen);

    /// <summary>Si el desvío del bono se suma al de la armadura (<c>true</c>) o se toma el mayor (<c>false</c>).</summary>
    bool DesvioBonoSeAcumula { get; }

    /// <summary>Derivados propios del mundo (<c>CharacterResponse.DerivadosSet</c>; Stormlight: vacío).</summary>
    Dictionary<string, StatDesglose> Derivar(CharacterEntity c, IReadOnlyList<string> talentos, IReadOnlyList<PoderPersonaje> poderes, BonosForma fb);

    /// <summary>
    /// Completa un desglose ya calculado por el motor de talentos (<c>TalentosReglas.Calcular</c>, llamado tras cada cálculo de
    /// <c>MapToResponse</c>) con líneas propias del mundo que no son reglas de talento (T49a). Nacidos de la bruma añade a la
    /// Defensa espiritual las líneas de los clavos hemalúrgicos, recalcula el <c>Total</c> y la línea situacional «Desorientado al
    /// inicio de escena»; Stormlight no hace nada.
    /// </summary>
    void CompletarDesglose(CharacterEntity c, StatAfectada stat, StatDesglose d);
}

/// <summary>Acciones de mesa (§5.2): el núcleo solo las encamina a <see cref="IWorldRules.AplicarAccionMesa"/>.</summary>
public abstract record AccionMesa
{
    public sealed record PatchRecursos(RecursosRequest Cuerpo, bool EsGm) : AccionMesa;

    public sealed record BeberVial(IReadOnlyList<string> Metales) : AccionMesa;

    public sealed record InicioEscena(bool Sorprendido) : AccionMesa;
}

/// <summary>
/// Lo que la creación guiada dicta para la ficha (<see cref="IWorldRules.FichaDeCreacion"/>): el flujo «Al elegir camino» de la ficha
/// (caminoMetalFlujo.ts del cliente) en la parte que necesita la creación. <c>CaminoMetal</c> vacío = sin camino: se deshace lo que la
/// creación hubiera puesto. <c>Habilidad</c> es la habilidad investida del camino (Alomancia, Voluntad, 1 grado; <c>GradoInicial</c> = un
/// grado gratuito más si el camino es el inicial, L.21 / PDF 27; L.135 / PDF 141). <c>TalentoPrincipal</c> se guarda solo cuando el camino no
/// es el inicial (si lo es, se concede sin ocupar hueco, L.17-18 / PDF 23-24); <c>TalentosDeCaminos</c> son todos los talentos principales
/// que la creación puede haber escrito, para quitarlos al cambiar o deshacer. <c>Meta</c> es la meta de nacido del metal que entrena los
/// poderes nacientes (L.132-133 / PDF 138-139); <c>null</c> cuando no hace falta (alomancia de atium, L.177 / PDF 183).
/// </summary>
public sealed record FichaDeCreacion(
    string CaminoMetal,
    List<PoderPersonaje> Poderes,
    HabilidadInvestidaCreacion Habilidad,
    string? TalentoPrincipal,
    IReadOnlyList<string> TalentosDeCaminos,
    MetaCreacion? Meta);

/// <summary>Habilidad investida que da el camino: nombre de la habilidad personalizada, código de su atributo (<c>VOL</c>), arte, grados y si el camino inicial suma uno gratuito.</summary>
public sealed record HabilidadInvestidaCreacion(string Nombre, string Codigo, string Arte, int Grados, bool GradoInicial);

/// <summary>Meta de nacido del metal que la creación crea (o reutiliza si sigue activa) y enlaza a los poderes nacientes.</summary>
public sealed record MetaCreacion(string Titulo, string Descripcion);

public interface IWorldRulesProvider
{
    /// <summary>Reglas del mundo; un id nulo, vacío o desconocido cae a Stormlight.</summary>
    IWorldRules Get(string? worldId);

    /// <summary>Fuente única de los mundos válidos: son las <see cref="IWorldRules"/> registradas.</summary>
    bool Existe(string? worldId);
}

public sealed class WorldRulesProvider(IEnumerable<IWorldRules> mundos) : IWorldRulesProvider
{
    private readonly Dictionary<string, IWorldRules> _porId = mundos.ToDictionary(s => s.Id);

    public IWorldRules Get(string? id) =>
        id is not null && _porId.TryGetValue(id, out var s) ? s : _porId[WorldIds.Stormlight];

    public bool Existe(string? id) => id is not null && _porId.ContainsKey(id);
}
