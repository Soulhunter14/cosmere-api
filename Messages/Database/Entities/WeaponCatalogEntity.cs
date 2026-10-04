using Messages.Worlds;

namespace Messages.Database.Entities;

public class WeaponCatalogEntity
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
    public bool IsCustom { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Weight { get; set; }

    // World the item belongs to (M3). Era: 1 or 2, null = both eras. Price is in the world's currency (mc | ar), null = none.
    public string World { get; set; } = WorldIds.Stormlight;
    public short? Era { get; set; }
    public bool IsRewardOnly { get; set; }
    public double? Price { get; set; }
}

public class ArmorCatalogEntity
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public int ArmorTypeId { get; set; }
    public int Desvio { get; set; }
    public List<int> TraitIds { get; set; } = [];
    public List<int> ExpertTraitIds { get; set; } = [];
    public bool IsCustom { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Weight { get; set; }

    public string World { get; set; } = WorldIds.Stormlight;
    public short? Era { get; set; }
    public bool IsRewardOnly { get; set; }
    public double? Price { get; set; }
}

public class GearItemEntity
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public double Weight { get; set; }
    public double Price { get; set; }
    public string Description { get; set; } = string.Empty;

    public string World { get; set; } = WorldIds.Stormlight;
    public short? Era { get; set; }
    public bool IsRewardOnly { get; set; }
    public string? Category { get; set; } // 'vial' for rare-metal vials; null otherwise
}

public class CatalogOptionEntity
{
    public int Id { get; set; }
    public required string Category { get; set; } // weapon_type, skill, damage_type, range, weapon_trait, armor_type, armor_trait
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;

    // stormlight | mistborn | cosmere (cosmere = shared by every world, WorldIds.Cosmere)
    public string World { get; set; } = WorldIds.Stormlight;
}
