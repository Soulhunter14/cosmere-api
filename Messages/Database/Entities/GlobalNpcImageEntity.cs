namespace Messages.Database.Entities;

/// <summary>
/// An image of an adversary as its book prints it (AddGlobalNpcImages): its stat block, its tactics box or its illustration, cut
/// from the GM's own PDF by `scripts/aventura/fichas_libro.py` (web repository) and uploaded to the global catalog, so every
/// campaign of the adversary's world sees it. One row per adversary and kind, deleted with the adversary. The server never reads
/// inside it.
/// </summary>
public class GlobalNpcImageEntity
{
    public long Id { get; set; }
    public long GlobalNpcId { get; set; }
    /// <summary>One of <see cref="GlobalNpcImageKinds.All"/>.</summary>
    public required string Kind { get; set; }
    /// <summary>image/webp, image/png or image/jpeg.</summary>
    public required string ContentType { get; set; }
    public byte[] Data { get; set; } = [];
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public GlobalNpcEntity GlobalNpc { get; set; } = null!;
}

/// <summary>
/// What an adversary image shows: «ficha» (the stat block), «tacticas» (the book's tactics box) or «retrato» (its illustration
/// in the book).
/// </summary>
public static class GlobalNpcImageKinds
{
    public const string Ficha = "ficha";
    public const string Tacticas = "tacticas";
    public const string Retrato = "retrato";
    public static readonly string[] All = [Ficha, Tacticas, Retrato];
}
