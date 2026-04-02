using Messages.Database.Entities;

namespace Infrastructure.Data;

public static class SeedData
{
    public static void Seed(CosmereContext db)
    {
        SeedUsers(db);

        // Catalog is always cleared and re-seeded (system data, not user data)
        db.CatalogOptions.RemoveRange(db.CatalogOptions);
        db.WeaponCatalog.RemoveRange(db.WeaponCatalog);
        db.ArmorCatalog.RemoveRange(db.ArmorCatalog);
        db.GearItems.RemoveRange(db.GearItems);
        db.SaveChanges();

        SeedCatalog(db);
        db.SaveChanges();
    }

    /// <summary>Called on startup (after clear) and on demand via /catalog/reimport.</summary>
    public static void SeedCatalog(CosmereContext db)
    {
        SeedCatalogOptions(db);
        SeedWeapons(db);
        SeedArmors(db);
        SeedGear(db);
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    private static void SeedUsers(CosmereContext db)
    {
        if (db.Users.Any(u => u.Username == "soul")) return;
        db.Users.Add(new UserEntity
        {
            Username = "soul",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Soul"),
            DisplayName = "Soul"
        });
    }

    // ── Catalog Options ───────────────────────────────────────────────────────
    // ID ranges: weapon_type 1-3 · skill 10-27 · damage_type 30-32
    //            range 40-45 · weapon_trait 50-62
    //            armor_type 70-77 · armor_trait 80-85

    private static void SeedCatalogOptions(CosmereContext db)
    {
        db.CatalogOptions.AddRange(

            // weapon_type
            new CatalogOptionEntity { Id = 1,  Category = "weapon_type",  Name = "Armamento ligero",              Description = "Arma de una mano, ágil" },
            new CatalogOptionEntity { Id = 2,  Category = "weapon_type",  Name = "Armamento pesado",              Description = "Arma de dos manos, poderosa" },
            new CatalogOptionEntity { Id = 3,  Category = "weapon_type",  Name = "Armas especiales",              Description = "Arma con propiedades únicas" },

            // skill
            new CatalogOptionEntity { Id = 10, Category = "skill", Name = "Agilidad",         Description = "" },
            new CatalogOptionEntity { Id = 11, Category = "skill", Name = "Armamento Ligero",  Description = "" },
            new CatalogOptionEntity { Id = 12, Category = "skill", Name = "Armamento Pesado",  Description = "" },
            new CatalogOptionEntity { Id = 13, Category = "skill", Name = "Atletismo",         Description = "" },
            new CatalogOptionEntity { Id = 14, Category = "skill", Name = "Hurto",             Description = "" },
            new CatalogOptionEntity { Id = 15, Category = "skill", Name = "Sigilo",            Description = "" },
            new CatalogOptionEntity { Id = 16, Category = "skill", Name = "Deducción",         Description = "" },
            new CatalogOptionEntity { Id = 17, Category = "skill", Name = "Disciplina",        Description = "" },
            new CatalogOptionEntity { Id = 18, Category = "skill", Name = "Intimidación",      Description = "" },
            new CatalogOptionEntity { Id = 19, Category = "skill", Name = "Manufactura",       Description = "" },
            new CatalogOptionEntity { Id = 20, Category = "skill", Name = "Medicina",          Description = "" },
            new CatalogOptionEntity { Id = 21, Category = "skill", Name = "Saber",             Description = "" },
            new CatalogOptionEntity { Id = 22, Category = "skill", Name = "Engaño",            Description = "" },
            new CatalogOptionEntity { Id = 23, Category = "skill", Name = "Liderazgo",         Description = "" },
            new CatalogOptionEntity { Id = 24, Category = "skill", Name = "Percepción",        Description = "" },
            new CatalogOptionEntity { Id = 25, Category = "skill", Name = "Perspicacia",       Description = "" },
            new CatalogOptionEntity { Id = 26, Category = "skill", Name = "Persuasión",        Description = "" },
            new CatalogOptionEntity { Id = 27, Category = "skill", Name = "Supervivencia",     Description = "" },

            // damage_type
            new CatalogOptionEntity { Id = 30, Category = "damage_type", Name = "Laceración",  Description = "Daño por corte" },
            new CatalogOptionEntity { Id = 31, Category = "damage_type", Name = "Golpe",       Description = "Daño por impacto" },
            new CatalogOptionEntity { Id = 32, Category = "damage_type", Name = "Espiritual",  Description = "Daño espiritual" },

            // range
            new CatalogOptionEntity { Id = 40, Category = "range", Name = "Cuerpo a Cuerpo",    Description = "Alcance cuerpo a cuerpo" },
            new CatalogOptionEntity { Id = 41, Category = "range", Name = "A Distancia 15/30",  Description = "Alcance normal 15 m / máximo 30 m" },
            new CatalogOptionEntity { Id = 42, Category = "range", Name = "A Distancia 24/96",  Description = "Alcance normal 24 m / máximo 96 m" },
            new CatalogOptionEntity { Id = 43, Category = "range", Name = "A Distancia 9/36",   Description = "Alcance normal 9 m / máximo 36 m" },
            new CatalogOptionEntity { Id = 44, Category = "range", Name = "A Distancia 30/120", Description = "Alcance normal 30 m / máximo 120 m" },
            new CatalogOptionEntity { Id = 45, Category = "range", Name = "A Distancia 60/240", Description = "Alcance normal 60 m / máximo 240 m" },

            // weapon_trait
            new CatalogOptionEntity { Id = 50, Category = "weapon_trait", Name = "Discreta",           Description = "No llama la atención al portarla" },
            new CatalogOptionEntity { Id = 51, Category = "weapon_trait", Name = "A dos manos",        Description = "Requiere dos manos para usarse" },
            new CatalogOptionEntity { Id = 52, Category = "weapon_trait", Name = "Preparación rápida", Description = "Se puede sacar sin usar acción" },
            new CatalogOptionEntity { Id = 53, Category = "weapon_trait", Name = "Defensiva",          Description = "Puede usarse para bloquear ataques" },
            new CatalogOptionEntity { Id = 54, Category = "weapon_trait", Name = "Mano secundaria",    Description = "Diseñada para usarse en la mano secundaria" },
            new CatalogOptionEntity { Id = 55, Category = "weapon_trait", Name = "Arrojadiza",         Description = "Puede lanzarse como arma a distancia" },
            new CatalogOptionEntity { Id = 56, Category = "weapon_trait", Name = "Indirecta",          Description = "El ataque puede rodear obstáculos" },
            new CatalogOptionEntity { Id = 57, Category = "weapon_trait", Name = "Única",              Description = "Tiene propiedades únicas descritas aparte" },
            new CatalogOptionEntity { Id = 58, Category = "weapon_trait", Name = "Inercia",            Description = "Gana impulso con el movimiento" },
            new CatalogOptionEntity { Id = 59, Category = "weapon_trait", Name = "Cargada",            Description = "Requiere recarga entre usos" },
            new CatalogOptionEntity { Id = 60, Category = "weapon_trait", Name = "Mortífera",          Description = "Causa heridas especialmente graves" },
            new CatalogOptionEntity { Id = 61, Category = "weapon_trait", Name = "Frágil",             Description = "Puede romperse con un uso brusco" },
            new CatalogOptionEntity { Id = 62, Category = "weapon_trait", Name = "Perforante",         Description = "Ignora parte del desvío del objetivo" },
            new CatalogOptionEntity { Id = 63, Category = "weapon_trait", Name = "Voluminosa [5]",     Description = "Ocupa 5 unidades de capacidad de carga" },
            new CatalogOptionEntity { Id = 64, Category = "weapon_trait", Name = "Peligrosa",          Description = "Puede causar daño al propio portador" },

            // armor_type
            new CatalogOptionEntity { Id = 70, Category = "armor_type", Name = "Uniforme",                      Description = "Ropa y uniformes reforzados" },
            new CatalogOptionEntity { Id = 71, Category = "armor_type", Name = "Cuero",                         Description = "Armadura de cuero curtido o tachonado" },
            new CatalogOptionEntity { Id = 72, Category = "armor_type", Name = "Malla",                         Description = "Cota de mallas metálica" },
            new CatalogOptionEntity { Id = 73, Category = "armor_type", Name = "Coraza",                        Description = "Coraza de placas metálicas" },
            new CatalogOptionEntity { Id = 74, Category = "armor_type", Name = "Armadura de placas y malla",    Description = "Combinación de placas y cota de mallas" },
            new CatalogOptionEntity { Id = 75, Category = "armor_type", Name = "Armadura completa",             Description = "Protección metálica de cuerpo completo" },
            new CatalogOptionEntity { Id = 76, Category = "armor_type", Name = "Armadura esquilada",            Description = "Armadura con esquirlas de granito incorporadas" },
            new CatalogOptionEntity { Id = 77, Category = "armor_type", Name = "Armadura esquilada (Radiante)", Description = "Armadura esquilada forjada por un Radiante" },

            // armor_trait
            new CatalogOptionEntity { Id = 80, Category = "armor_trait", Name = "Presentable",    Description = "Adecuada para entornos sociales y formales" },
            new CatalogOptionEntity { Id = 81, Category = "armor_trait", Name = "Voluminosa [3]",  Description = "Ocupa 3 unidades de capacidad de carga" },
            new CatalogOptionEntity { Id = 82, Category = "armor_trait", Name = "Voluminosa [4]",  Description = "Ocupa 4 unidades de capacidad de carga" },
            new CatalogOptionEntity { Id = 83, Category = "armor_trait", Name = "Voluminosa [5]",  Description = "Ocupa 5 unidades de capacidad de carga" },
            new CatalogOptionEntity { Id = 84, Category = "armor_trait", Name = "Peligrosa",       Description = "Puede causar daño al propio portador" },
            new CatalogOptionEntity { Id = 85, Category = "armor_trait", Name = "Única",           Description = "Tiene propiedades únicas descritas aparte" }
        );
    }

    // ── Weapons ───────────────────────────────────────────────────────────────
    // ID ranges for options (global):
    //   weapon_type 1-3 · skill 10-27 · damage_type 30-32 · range 40-45
    //   weapon_trait 50-64 (50-62 base, 63-64 special)

    private static void SeedWeapons(CosmereContext db)
    {
        db.WeaponCatalog.AddRange(

            // ── Armamento ligero ──────────────────────────────────────────────
            new WeaponCatalogEntity { Id = 1,  Name = "Arco corto",              WeaponTypeId = 1, SkillId = 11, DamageDiceCount = 1, DamageDiceValue = 6,  DamageTypeId = 30, RangeId = 42, TraitIds = [51],     ExpertTraitIds = [52] },
            new WeaponCatalogEntity { Id = 2,  Name = "Bastón",                  WeaponTypeId = 1, SkillId = 11, DamageDiceCount = 1, DamageDiceValue = 6,  DamageTypeId = 31, RangeId = 40, TraitIds = [50, 51], ExpertTraitIds = [53] },
            new WeaponCatalogEntity { Id = 3,  Name = "Cuchillo",                WeaponTypeId = 1, SkillId = 11, DamageDiceCount = 1, DamageDiceValue = 4,  DamageTypeId = 30, RangeId = 40, TraitIds = [50],     ExpertTraitIds = [54, 55] },
            new WeaponCatalogEntity { Id = 4,  Name = "Espada lateral",          WeaponTypeId = 1, SkillId = 11, DamageDiceCount = 1, DamageDiceValue = 6,  DamageTypeId = 30, RangeId = 40, TraitIds = [52],     ExpertTraitIds = [54] },
            new WeaponCatalogEntity { Id = 5,  Name = "Espada ropera",           WeaponTypeId = 1, SkillId = 11, DamageDiceCount = 1, DamageDiceValue = 6,  DamageTypeId = 30, RangeId = 40, TraitIds = [52],     ExpertTraitIds = [53] },
            new WeaponCatalogEntity { Id = 6,  Name = "Honda",                   WeaponTypeId = 1, SkillId = 11, DamageDiceCount = 1, DamageDiceValue = 4,  DamageTypeId = 31, RangeId = 43, TraitIds = [50],     ExpertTraitIds = [56] },
            new WeaponCatalogEntity { Id = 7,  Name = "Jabalina",                WeaponTypeId = 1, SkillId = 11, DamageDiceCount = 1, DamageDiceValue = 6,  DamageTypeId = 30, RangeId = 40, TraitIds = [55],     ExpertTraitIds = [56] },
            new WeaponCatalogEntity { Id = 8,  Name = "Lanza corta",             WeaponTypeId = 1, SkillId = 11, DamageDiceCount = 1, DamageDiceValue = 8,  DamageTypeId = 30, RangeId = 40, TraitIds = [51],     ExpertTraitIds = [57] },
            new WeaponCatalogEntity { Id = 9,  Name = "Maza",                    WeaponTypeId = 1, SkillId = 11, DamageDiceCount = 1, DamageDiceValue = 6,  DamageTypeId = 31, RangeId = 40, TraitIds = [],       ExpertTraitIds = [58] },

            // ── Armamento pesado ──────────────────────────────────────────────
            new WeaponCatalogEntity { Id = 10, Name = "Alabarda",                WeaponTypeId = 2, SkillId = 12, DamageDiceCount = 1, DamageDiceValue = 10, DamageTypeId = 30, RangeId = 40, TraitIds = [51],     ExpertTraitIds = [57] },
            new WeaponCatalogEntity { Id = 11, Name = "Arco largo",              WeaponTypeId = 2, SkillId = 12, DamageDiceCount = 1, DamageDiceValue = 6,  DamageTypeId = 30, RangeId = 41, TraitIds = [51],     ExpertTraitIds = [56] },
            new WeaponCatalogEntity { Id = 12, Name = "Ballesta",                WeaponTypeId = 2, SkillId = 12, DamageDiceCount = 1, DamageDiceValue = 8,  DamageTypeId = 30, RangeId = 44, TraitIds = [59, 51], ExpertTraitIds = [60] },
            new WeaponCatalogEntity { Id = 13, Name = "Escudo",                  WeaponTypeId = 2, SkillId = 12, DamageDiceCount = 1, DamageDiceValue = 4,  DamageTypeId = 31, RangeId = 40, TraitIds = [53],     ExpertTraitIds = [54] },
            new WeaponCatalogEntity { Id = 14, Name = "Espada larga",            WeaponTypeId = 2, SkillId = 12, DamageDiceCount = 1, DamageDiceValue = 8,  DamageTypeId = 30, RangeId = 40, TraitIds = [52, 51], ExpertTraitIds = [57] },
            new WeaponCatalogEntity { Id = 15, Name = "Hacha",                   WeaponTypeId = 2, SkillId = 12, DamageDiceCount = 1, DamageDiceValue = 6,  DamageTypeId = 30, RangeId = 40, TraitIds = [55],     ExpertTraitIds = [54] },
            new WeaponCatalogEntity { Id = 16, Name = "Lanza larga",             WeaponTypeId = 2, SkillId = 12, DamageDiceCount = 1, DamageDiceValue = 8,  DamageTypeId = 30, RangeId = 40, TraitIds = [51],     ExpertTraitIds = [53] },
            new WeaponCatalogEntity { Id = 17, Name = "Mandoble",                WeaponTypeId = 2, SkillId = 12, DamageDiceCount = 1, DamageDiceValue = 10, DamageTypeId = 30, RangeId = 40, TraitIds = [51],     ExpertTraitIds = [60] },
            new WeaponCatalogEntity { Id = 18, Name = "Martillo",                WeaponTypeId = 2, SkillId = 12, DamageDiceCount = 1, DamageDiceValue = 10, DamageTypeId = 31, RangeId = 40, TraitIds = [51],     ExpertTraitIds = [58] },

            // ── Armas especiales ──────────────────────────────────────────────
            new WeaponCatalogEntity { Id = 19, Name = "Arma improvisada",        WeaponTypeId = 3, SkillId = 10, DamageDiceCount = 0, DamageDiceValue = 0,  DamageTypeId = 31, RangeId = 40, TraitIds = [61],             ExpertTraitIds = [57] },
            new WeaponCatalogEntity { Id = 20, Name = "Ataque sin armas",        WeaponTypeId = 3, SkillId = 13, DamageDiceCount = 0, DamageDiceValue = 0,  DamageTypeId = 31, RangeId = 40, TraitIds = [57],             ExpertTraitIds = [58, 54] },
            new WeaponCatalogEntity { Id = 21, Name = "Gran arco",               WeaponTypeId = 3, SkillId = 12, DamageDiceCount = 2, DamageDiceValue = 6,  DamageTypeId = 30, RangeId = 45, TraitIds = [63, 51],         ExpertTraitIds = [62] },
            new WeaponCatalogEntity { Id = 22, Name = "Hoja esquilada",          WeaponTypeId = 3, SkillId = 12, DamageDiceCount = 2, DamageDiceValue = 8,  DamageTypeId = 32, RangeId = 40, TraitIds = [64, 60, 57],     ExpertTraitIds = [57] },
            new WeaponCatalogEntity { Id = 23, Name = "Hoja esquilada (Radiante)",WeaponTypeId = 3, SkillId = 10, DamageDiceCount = 2, DamageDiceValue = 0,  DamageTypeId = 32, RangeId = 40, TraitIds = [60, 57],         ExpertTraitIds = [] },
            new WeaponCatalogEntity { Id = 24, Name = "Martillo de guerra",      WeaponTypeId = 3, SkillId = 12, DamageDiceCount = 2, DamageDiceValue = 10, DamageTypeId = 31, RangeId = 40, TraitIds = [63, 51],         ExpertTraitIds = [57] },
            new WeaponCatalogEntity { Id = 25, Name = "Semiesquirla",            WeaponTypeId = 3, SkillId = 12, DamageDiceCount = 4, DamageDiceValue = 0,  DamageTypeId = 31, RangeId = 40, TraitIds = [53, 51, 57],     ExpertTraitIds = [58] }
        );
    }

    // ── Armors ────────────────────────────────────────────────────────────────
    // armor_type 70-77 · armor_trait 80-85

    private static void SeedArmors(CosmereContext db)
    {
        db.ArmorCatalog.AddRange(
            new ArmorCatalogEntity { Id = 1, Name = "Uniforme",                      ArmorTypeId = 70, Desvio = 0, TraitIds = [80],     ExpertTraitIds = [] },
            new ArmorCatalogEntity { Id = 2, Name = "Cuero",                         ArmorTypeId = 71, Desvio = 1, TraitIds = [],       ExpertTraitIds = [80] },
            new ArmorCatalogEntity { Id = 3, Name = "Malla",                         ArmorTypeId = 72, Desvio = 2, TraitIds = [81],     ExpertTraitIds = [85] },
            new ArmorCatalogEntity { Id = 4, Name = "Coraza",                        ArmorTypeId = 73, Desvio = 2, TraitIds = [81],     ExpertTraitIds = [80] },
            new ArmorCatalogEntity { Id = 5, Name = "Armadura de placas y malla",    ArmorTypeId = 74, Desvio = 3, TraitIds = [82],     ExpertTraitIds = [85] },
            new ArmorCatalogEntity { Id = 6, Name = "Armadura completa",             ArmorTypeId = 75, Desvio = 4, TraitIds = [83],     ExpertTraitIds = [] },
            new ArmorCatalogEntity { Id = 7, Name = "Armadura esquilada",            ArmorTypeId = 76, Desvio = 5, TraitIds = [84, 85], ExpertTraitIds = [85] },
            new ArmorCatalogEntity { Id = 8, Name = "Armadura esquilada (Radiante)", ArmorTypeId = 77, Desvio = 5, TraitIds = [85],     ExpertTraitIds = [] }
        );
    }

    // ── Gear ──────────────────────────────────────────────────────────────────
    // 68 items from the Stonewalkers catalog. Weight in kg, price in marks (mc).
    // Items marked with * in the original have no specific game rules.

    private static void SeedGear(CosmereContext db)
    {
        db.GearItems.AddRange(
            new GearItemEntity { Id = 1,  Name = "Aceite (1 frasco)",                  Weight = 0.5,   Price = 1.0,   Description = "Combustible para linternas y trampas de fuego" },
            new GearItemEntity { Id = 2,  Name = "Alcohol (1 consinucilio)",            Weight = 0.1,   Price = 0.5,   Description = "Valor variable (0,5 – 50 mc). Sin reglas específicas." },
            new GearItemEntity { Id = 3,  Name = "Alcohol (botella)",                   Weight = 1.0,   Price = 1.0,   Description = "Peso y valor variables (1–12 kg / 1–300 mc). Sin reglas específicas." },
            new GearItemEntity { Id = 4,  Name = "Anestésico (5 dosis)",                Weight = 0.75,  Price = 75.0,  Description = "" },
            new GearItemEntity { Id = 5,  Name = "Antiséptico (débil, 5 dosis)",        Weight = 0.5,   Price = 25.0,  Description = "" },
            new GearItemEntity { Id = 6,  Name = "Antiséptico (potente, 5 dosis)",      Weight = 0.5,   Price = 50.0,  Description = "" },
            new GearItemEntity { Id = 7,  Name = "Arcón",                               Weight = 12.5,  Price = 30.0,  Description = "" },
            new GearItemEntity { Id = 8,  Name = "Balanza",                             Weight = 1.5,   Price = 20.0,  Description = "" },
            new GearItemEntity { Id = 9,  Name = "Barril",                              Weight = 35.0,  Price = 15.0,  Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 10, Name = "Bolsa",                               Weight = 0.5,   Price = 1.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 11, Name = "Botella (crem)",                      Weight = 1.5,   Price = 0.5,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 12, Name = "Botella (cristal)",                   Weight = 1.0,   Price = 1.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 13, Name = "Cadena (fina, 0,3 metros)",           Weight = 0.25,  Price = 20.0,  Description = "" },
            new GearItemEntity { Id = 14, Name = "Cadena (gruesa, 3 metros)",           Weight = 5.0,   Price = 20.0,  Description = "" },
            new GearItemEntity { Id = 15, Name = "Cálamo",                              Weight = 0.05,  Price = 0.1,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 16, Name = "Catalejo",                            Weight = 0.5,   Price = 500.0, Description = "" },
            new GearItemEntity { Id = 17, Name = "Cera (1 bloque)",                     Weight = 0.1,   Price = 2.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 18, Name = "Cerradura y llave",                   Weight = 0.5,   Price = 50.0,  Description = "" },
            new GearItemEntity { Id = 19, Name = "Comida (buena, 1 día)",               Weight = 0.25,  Price = 25.0,  Description = "" },
            new GearItemEntity { Id = 20, Name = "Comida (callejera, 1 día)",           Weight = 0.75,  Price = 3.0,   Description = "" },
            new GearItemEntity { Id = 21, Name = "Comida (ración, 1 día)",              Weight = 0.25,  Price = 0.2,   Description = "" },
            new GearItemEntity { Id = 22, Name = "Cubo",                                Weight = 1.0,   Price = 1.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 23, Name = "Cuerda (15 metros)",                  Weight = 2.5,   Price = 30.0,  Description = "" },
            new GearItemEntity { Id = 24, Name = "Diapasón",                            Weight = 0.25,  Price = 50.0,  Description = "" },
            new GearItemEntity { Id = 25, Name = "Escalera de mano (3 metros)",         Weight = 10.0,  Price = 5.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 26, Name = "Espejo (de mano)",                    Weight = 1.0,   Price = 25.0,  Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 27, Name = "Estuche (cuero)",                     Weight = 0.5,   Price = 4.0,   Description = "" },
            new GearItemEntity { Id = 28, Name = "Frasco o tarro",                      Weight = 0.5,   Price = 1.0,   Description = "" },
            new GearItemEntity { Id = 29, Name = "Ganzúa",                              Weight = 0.25,  Price = 5.0,   Description = "" },
            new GearItemEntity { Id = 30, Name = "Garfo de ensalada",                   Weight = 2.0,   Price = 10.0,  Description = "" },
            new GearItemEntity { Id = 31, Name = "Gemas sin recubrimiento (infusa)",    Weight = 0.005, Price = 2.0,   Description = "" },
            new GearItemEntity { Id = 32, Name = "Grilletes",                           Weight = 3.0,   Price = 10.0,  Description = "" },
            new GearItemEntity { Id = 33, Name = "Instrumento musical",                 Weight = 0.25,  Price = 1.0,   Description = "Peso y valor variables (250 g – 10 kg / 1–50 mc)." },
            new GearItemEntity { Id = 34, Name = "Jabón",                               Weight = 0.05,  Price = 1.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 35, Name = "Jarra o piche",                       Weight = 2.0,   Price = 2.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 36, Name = "Libro (de referencia)",               Weight = 0.5,   Price = 10.0,  Description = "Peso y valor variables (500 g – 2,5 kg / 10–500 mc)." },
            new GearItemEntity { Id = 37, Name = "Linterna (aceite)",                   Weight = 1.0,   Price = 20.0,  Description = "" },
            new GearItemEntity { Id = 38, Name = "Linterna (esfera)",                   Weight = 1.0,   Price = 20.0,  Description = "" },
            new GearItemEntity { Id = 39, Name = "Lupa",                                Weight = 0.1,   Price = 2.0,   Description = "" },
            new GearItemEntity { Id = 40, Name = "Manta",                               Weight = 1.0,   Price = 2.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 41, Name = "Martillo (de mano)",                  Weight = 1.5,   Price = 4.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 42, Name = "Mochila",                             Weight = 2.5,   Price = 8.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 43, Name = "Odre",                                Weight = 0.5,   Price = 1.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 44, Name = "Olla (hierro)",                       Weight = 5.0,   Price = 8.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 45, Name = "Pala",                                Weight = 2.5,   Price = 8.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 46, Name = "Palanca",                             Weight = 1.5,   Price = 10.0,  Description = "" },
            new GearItemEntity { Id = 47, Name = "Papel o pergamino (1 hoja)",          Weight = 0.05,  Price = 0.5,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 48, Name = "Pedernal y acero",                    Weight = 0.75,  Price = 4.0,   Description = "" },
            new GearItemEntity { Id = 49, Name = "Perfume (1 vial)",                    Weight = 0.25,  Price = 20.0,  Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 50, Name = "Pico (minero)",                       Weight = 5.0,   Price = 10.0,  Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 51, Name = "Piedra de afilar",                    Weight = 0.5,   Price = 0.2,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 52, Name = "Red (de caza)",                       Weight = 2.5,   Price = 4.0,   Description = "" },
            new GearItemEntity { Id = 53, Name = "Red (de pesca)",                      Weight = 7.5,   Price = 10.0,  Description = "" },
            new GearItemEntity { Id = 54, Name = "Ropa (buena)",                        Weight = 3.0,   Price = 50.0,  Description = "Valor variable (50–200 mc)." },
            new GearItemEntity { Id = 55, Name = "Ropa (común)",                        Weight = 1.5,   Price = 2.0,   Description = "" },
            new GearItemEntity { Id = 56, Name = "Ropa (ráida)",                        Weight = 0.75,  Price = 0.5,   Description = "" },
            new GearItemEntity { Id = 57, Name = "Saco",                                Weight = 0.25,  Price = 0.2,   Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 58, Name = "Sistema de poleas",                   Weight = 6.0,   Price = 100.0, Description = "" },
            new GearItemEntity { Id = 59, Name = "Suministros quirúrgicos",             Weight = 1.5,   Price = 20.0,  Description = "" },
            new GearItemEntity { Id = 60, Name = "Tienda de campaña (dos personas)",    Weight = 10.0,  Price = 10.0,  Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 61, Name = "Tinta (vial de 30 ml)",               Weight = 0.1,   Price = 40.0,  Description = "Incluido para contribuir al juego; sin reglas específicas." },
            new GearItemEntity { Id = 62, Name = "Tratamiento (médico, 1 dosis)",       Weight = 0.1,   Price = 10.0,  Description = "" },
            new GearItemEntity { Id = 63, Name = "Trompetilla",                         Weight = 0.5,   Price = 50.0,  Description = "" },
            new GearItemEntity { Id = 64, Name = "Vela",                                Weight = 0.1,   Price = 0.2,   Description = "" },
            new GearItemEntity { Id = 65, Name = "Veneno (débil, 1 dosis)",             Weight = 0.1,   Price = 20.0,  Description = "" },
            new GearItemEntity { Id = 66, Name = "Veneno (efectivo, 1 dosis)",          Weight = 0.1,   Price = 50.0,  Description = "" },
            new GearItemEntity { Id = 67, Name = "Veneno (potente, 1 dosis)",           Weight = 0.1,   Price = 120.0, Description = "" },
            new GearItemEntity { Id = 68, Name = "Vial (cristal)",                      Weight = 0.1,   Price = 4.0,   Description = "Incluido para contribuir al juego; sin reglas específicas." }
        );
    }
}
