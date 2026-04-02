namespace Messages.Catalog.Out;

public class WeaponCatalogResponse
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public int WeaponTypeId { get; set; }
    public int SkillId { get; set; }
    public int DamageDiceCount { get; set; }
    public int DamageDiceValue { get; set; }
    public int DamageTypeId { get; set; }
    public int RangeId { get; set; }
    public List<int> TraitIds { get; set; } = [];
    public List<int> ExpertTraitIds { get; set; } = [];
}

public class ArmorCatalogResponse
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public int ArmorTypeId { get; set; }
    public int Desvio { get; set; }
    public List<int> TraitIds { get; set; } = [];
    public List<int> ExpertTraitIds { get; set; } = [];
}

public class GearItemResponse
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public double Weight { get; set; }
    public double Price { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CatalogOptionResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
}
