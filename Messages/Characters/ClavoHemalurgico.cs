namespace Messages.Characters;

/// <summary>
/// Clavo hemalúrgico de un personaje de «Nacidos de la bruma» (hemalurgia en la ficha, T49a). Los clavos son una recompensa del
/// DJ (L.288 / PDF 294) y se guardan en su propia lista JSON (camelCase) en <c>Characters.Clavos</c>, independiente de
/// <see cref="PoderPersonaje"/>: los clavos de cinc, cobre, estaño y hierro no dan poder pero sí suben un atributo y restan
/// Defensa espiritual (L.290-291 / PDF 296-297), y el metal del clavo no es el poder (un clavo de acero da una alomancia física
/// a elegir). El poder de un clavo de poder viaja aparte, como <see cref="PoderPersonaje"/> con <c>Origen == "clavo"</c>.
/// </summary>
public sealed class ClavoHemalurgico
{
    /// <summary>
    /// Id ASCII del metal del clavo: uno de los 12 de la tabla «Efectos conocidos de los clavos hemalúrgicos»
    /// (L.291 / PDF 297): <c>cinc</c>, <c>cobre</c>, <c>estano</c>, <c>hierro</c>, <c>acero</c>, <c>bronce</c>, <c>cadmio</c>,
    /// <c>electro</c>, <c>peltre</c>, <c>laton</c>, <c>oro</c> o <c>bendaleo</c>.
    /// </summary>
    public required string MetalClavo { get; set; }

    /// <summary>
    /// Poder que otorga un clavo de poder, como <c>"{arte}:{metal}"</c> (p. ej. <c>alomancia:hierro</c>), a elegir entre los
    /// cuatro de su metal (L.291 / PDF 297); <c>null</c> en los clavos de atributo.
    /// </summary>
    public string? PoderElegido { get; set; }

    /// <summary>
    /// <c>true</c> mientras el clavo está implantado: solo entonces tiene efecto. Un clavo extraído conserva sus datos pero sus
    /// efectos terminan (L.290 / PDF 296).
    /// </summary>
    public bool Implantado { get; set; } = true;

    /// <summary>«Clavo secreto» (L.289 / PDF 295): solo informativo en v1, no cambia ningún cálculo.</summary>
    public bool Secreto { get; set; }
}
