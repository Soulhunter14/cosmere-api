namespace Messages.Characters;

/// <summary>
/// Poder de artes metálicas de un personaje de «Nacidos de la bruma» (alomancia o feruquimia de un metal). Compartido por
/// request, response y servicio; se persiste como JSON (camelCase) en la columna <c>Characters.Poderes</c> (T11). Id derivado
/// <c>${arte}:${metal}</c> (p. ej. <c>alomancia:acero</c>).
/// </summary>
public sealed class PoderPersonaje
{
    /// <summary><c>alomancia</c> | <c>feruquimia</c>.</summary>
    public required string Arte { get; set; }

    /// <summary>Id ASCII de uno de los 17 metales (<c>hierro</c>, <c>acero</c>, <c>estano</c>…).</summary>
    public required string Metal { get; set; }

    /// <summary><c>camino</c> | <c>clavo</c> | <c>lerasium</c> | <c>medallon</c> (L.288-295 / PDF 294-301).</summary>
    public string Origen { get; set; } = "camino";

    /// <summary>
    /// Naciente (<c>false</c>) solo si <c>Origen == "camino"</c> y la meta no está concluida (L.162 / PDF 168);
    /// <c>alomancia:atium</c> siempre <c>true</c> (L.177 / PDF 183); <c>Origen != "camino"</c> siempre <c>true</c>
    /// (L.290 / PDF 296; L.295 / PDF 301).
    /// </summary>
    public bool Completo { get; set; } = false;

    /// <summary>Metas.Id de «Entrenar tu poder» / «Fabricar tu mente de metal» (L.132-133 / PDF 138-139).</summary>
    public long? MetaId { get; set; }

    /// <summary>Feruquimia: cargas actuales; una reserva por metal (L.131 / PDF 137).</summary>
    public int Cargas { get; set; } = 0;

    /// <summary>Componedor: −1 permanente por uso (L.155 / PDF 161).</summary>
    public int AjusteCargasMax { get; set; } = 0;

    /// <summary>Alomancia, solo metales raros (L.130 / PDF 136).</summary>
    public int Viales { get; set; } = 0;

    /// <summary>Estado Desprovisto [poder] (L.310 / PDF 316).</summary>
    public bool Desprovisto { get; set; } = false;
}
