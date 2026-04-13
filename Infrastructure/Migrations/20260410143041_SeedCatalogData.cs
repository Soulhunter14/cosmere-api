using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatalogData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Clear existing data (dependents first) ────────────────────────
            migrationBuilder.Sql(@"DELETE FROM ""WeaponCatalog"";");
            migrationBuilder.Sql(@"DELETE FROM ""ArmorCatalog"";");
            migrationBuilder.Sql(@"DELETE FROM ""GearItems"";");
            migrationBuilder.Sql(@"DELETE FROM ""CatalogOptions"";");

            // ── CatalogOptions ────────────────────────────────────────────────
            migrationBuilder.Sql(@"
INSERT INTO ""CatalogOptions"" (""Id"", ""Category"", ""Name"", ""Description"") VALUES
(1,  'weapon_type',  'Armamento ligero',              'Arma de una mano, ágil'),
(2,  'weapon_type',  'Armamento pesado',              'Arma de dos manos, poderosa'),
(3,  'weapon_type',  'Armas especiales',              'Arma con propiedades únicas'),
(10, 'skill', 'Agilidad',          ''),
(11, 'skill', 'Armamento Ligero',  ''),
(12, 'skill', 'Armamento Pesado',  ''),
(13, 'skill', 'Atletismo',         ''),
(14, 'skill', 'Hurto',             ''),
(15, 'skill', 'Sigilo',            ''),
(16, 'skill', 'Deducción',         ''),
(17, 'skill', 'Disciplina',        ''),
(18, 'skill', 'Intimidación',      ''),
(19, 'skill', 'Manufactura',       ''),
(20, 'skill', 'Medicina',          ''),
(21, 'skill', 'Saber',             ''),
(22, 'skill', 'Engaño',            ''),
(23, 'skill', 'Liderazgo',         ''),
(24, 'skill', 'Percepción',        ''),
(25, 'skill', 'Perspicacia',       ''),
(26, 'skill', 'Persuasión',        ''),
(27, 'skill', 'Supervivencia',     ''),
(30, 'damage_type', 'Laceración',  'Daño por corte'),
(31, 'damage_type', 'Golpe',       'Daño por impacto'),
(32, 'damage_type', 'Espiritual',  'Daño espiritual'),
(40, 'range', 'Cuerpo a Cuerpo',    'Alcance cuerpo a cuerpo'),
(41, 'range', 'A Distancia 15/30',  'Alcance normal 15 m / máximo 30 m'),
(42, 'range', 'A Distancia 24/96',  'Alcance normal 24 m / máximo 96 m'),
(43, 'range', 'A Distancia 9/36',   'Alcance normal 9 m / máximo 36 m'),
(44, 'range', 'A Distancia 30/120', 'Alcance normal 30 m / máximo 120 m'),
(45, 'range', 'A Distancia 60/240', 'Alcance normal 60 m / máximo 240 m'),
(46, 'range', 'A Distancia 45/180', 'Alcance normal 45 m / máximo 180 m'),
(50, 'weapon_trait', 'Discreta',           'No llama la atención al portarla'),
(51, 'weapon_trait', 'A dos manos',        'Requiere dos manos para usarse'),
(52, 'weapon_trait', 'Preparación rápida', 'Se puede sacar sin usar acción'),
(53, 'weapon_trait', 'Defensiva',          'Puede usarse para bloquear ataques'),
(54, 'weapon_trait', 'Mano secundaria',    'Diseñada para usarse en la mano secundaria'),
(55, 'weapon_trait', 'Arrojadiza',         'Puede lanzarse como arma a distancia'),
(56, 'weapon_trait', 'Indirecta',          'El ataque puede rodear obstáculos'),
(57, 'weapon_trait', 'Única',              'Tiene propiedades únicas descritas aparte'),
(58, 'weapon_trait', 'Inercia',            'Gana impulso con el movimiento'),
(59, 'weapon_trait', 'Cargada',            'Requiere recarga entre usos'),
(60, 'weapon_trait', 'Mortífera',          'Causa heridas especialmente graves'),
(61, 'weapon_trait', 'Frágil',             'Puede romperse con un uso brusco'),
(62, 'weapon_trait', 'Perforante',         'Ignora parte del desvío del objetivo'),
(63, 'weapon_trait', 'Voluminosa [5]',     'Ocupa 5 unidades de capacidad de carga'),
(64, 'weapon_trait', 'Peligrosa',          'Puede causar daño al propio portador'),
(70, 'armor_type', 'Uniforme',                      'Ropa y uniformes reforzados'),
(71, 'armor_type', 'Cuero',                         'Armadura de cuero curtido o tachonado'),
(72, 'armor_type', 'Malla',                         'Cota de mallas metálica'),
(73, 'armor_type', 'Coraza',                        'Coraza de placas metálicas'),
(74, 'armor_type', 'Armadura de placas y malla',    'Combinación de placas y cota de mallas'),
(75, 'armor_type', 'Armadura completa',              'Protección metálica de cuerpo completo'),
(76, 'armor_type', 'Armadura esquirlada',            'Armadura Investida de metal desconocido'),
(77, 'armor_type', 'Armadura esquirlada (Radiante)', 'Armadura esquirlada forjada por un Radiante'),
(80, 'armor_trait', 'Presentable',    'Adecuada para entornos sociales y formales'),
(81, 'armor_trait', 'Voluminosa [3]', 'Ocupa 3 unidades de capacidad de carga'),
(82, 'armor_trait', 'Voluminosa [4]', 'Ocupa 4 unidades de capacidad de carga'),
(83, 'armor_trait', 'Voluminosa [5]', 'Ocupa 5 unidades de capacidad de carga'),
(84, 'armor_trait', 'Peligrosa',      'Puede causar daño al propio portador'),
(85, 'armor_trait', 'Única',          'Tiene propiedades únicas descritas aparte');
");

            // ── WeaponCatalog ─────────────────────────────────────────────────
            migrationBuilder.Sql(@"
INSERT INTO ""WeaponCatalog"" (""Id"", ""Name"", ""WeaponTypeId"", ""SkillId"", ""DamageDiceCount"", ""DamageDiceValue"", ""DamageTypeId"", ""RangeId"", ""TraitIds"", ""ExpertTraitIds"") VALUES
(1,  'Arco corto',                 1, 11, 1, 6,  30, 42, ARRAY[51],        ARRAY[52]),
(2,  'Bastón',                     1, 11, 1, 6,  31, 40, ARRAY[50,51],     ARRAY[53]),
(3,  'Cuchillo',                   1, 11, 1, 4,  30, 40, ARRAY[50],        ARRAY[54,55]),
(4,  'Espada lateral',             1, 11, 1, 6,  30, 40, ARRAY[52],        ARRAY[54]),
(5,  'Espada ropera',              1, 11, 1, 6,  30, 40, ARRAY[52],        ARRAY[53]),
(6,  'Honda',                      1, 11, 1, 4,  31, 43, ARRAY[50],        ARRAY[56]),
(7,  'Jabalina',                   1, 11, 1, 6,  30, 40, ARRAY[55],        ARRAY[56]),
(8,  'Lanza corta',                1, 11, 1, 8,  30, 40, ARRAY[51],        ARRAY[57]),
(9,  'Maza',                       1, 11, 1, 6,  31, 40, ARRAY[]::int[],   ARRAY[58]),
(10, 'Alabarda',                   2, 12, 1, 10, 30, 40, ARRAY[51],        ARRAY[57]),
(11, 'Arco largo',                 2, 12, 1, 6,  30, 46, ARRAY[51],        ARRAY[56]),
(12, 'Ballesta',                   2, 12, 1, 8,  30, 44, ARRAY[59,51],     ARRAY[60]),
(13, 'Escudo',                     2, 12, 1, 4,  31, 40, ARRAY[53],        ARRAY[54]),
(14, 'Espada larga',               2, 12, 1, 8,  30, 40, ARRAY[52,51],     ARRAY[57]),
(15, 'Hacha',                      2, 12, 1, 6,  30, 40, ARRAY[55],        ARRAY[54]),
(16, 'Lanza larga',                2, 12, 1, 8,  30, 40, ARRAY[51],        ARRAY[53]),
(17, 'Mandoble',                   2, 12, 1, 10, 30, 40, ARRAY[51],        ARRAY[60]),
(18, 'Martillo',                   2, 12, 1, 10, 31, 40, ARRAY[51],        ARRAY[58]),
(19, 'Arma improvisada',           3, 10, 0, 0,  31, 40, ARRAY[61],        ARRAY[57]),
(20, 'Ataque sin armas',           3, 13, 0, 0,  31, 40, ARRAY[57],        ARRAY[58,54]),
(21, 'Gran arco',                  3, 12, 2, 6,  30, 45, ARRAY[63,51],     ARRAY[62]),
(22, 'Hoja esquirlada',            3, 12, 2, 8,  32, 40, ARRAY[64,60,57],  ARRAY[57]),
(23, 'Hoja esquirlada (Radiante)', 3, 10, 2, 0,  32, 40, ARRAY[60,57],     ARRAY[]::int[]),
(24, 'Martillo de guerra',         3, 12, 2, 10, 31, 40, ARRAY[63,51],     ARRAY[57]),
(25, 'Semiesquirla',               3, 12, 2, 4,  31, 40, ARRAY[53,51,57],  ARRAY[58]);
");

            // ── ArmorCatalog ──────────────────────────────────────────────────
            migrationBuilder.Sql(@"
INSERT INTO ""ArmorCatalog"" (""Id"", ""Name"", ""ArmorTypeId"", ""Desvio"", ""TraitIds"", ""ExpertTraitIds"") VALUES
(1, 'Uniforme',                       70, 0, ARRAY[80],     ARRAY[]::int[]),
(2, 'Cuero',                          71, 1, ARRAY[]::int[], ARRAY[80]),
(3, 'Malla',                          72, 2, ARRAY[81],     ARRAY[85]),
(4, 'Coraza',                         73, 2, ARRAY[81],     ARRAY[80]),
(5, 'Armadura de placas y malla',     74, 3, ARRAY[82],     ARRAY[85]),
(6, 'Armadura completa',              75, 4, ARRAY[83],     ARRAY[]::int[]),
(7, 'Armadura esquirlada',            76, 5, ARRAY[84,85],  ARRAY[85]),
(8, 'Armadura esquirlada (Radiante)', 77, 5, ARRAY[85],     ARRAY[]::int[]);
");

            // ── GearItems ─────────────────────────────────────────────────────
            migrationBuilder.Sql(@"
INSERT INTO ""GearItems"" (""Id"", ""Name"", ""Weight"", ""Price"", ""Description"") VALUES
(1,  'Aceite (1 frasco)',                0.5,   1.0,   'Combustible para linternas y trampas de fuego'),
(2,  'Alcohol (1 consumición)',          0.1,   0.5,   'Valor variable (0,5 – 50 mc). Sin reglas específicas.'),
(3,  'Alcohol (botella)',                1.0,   1.0,   'Peso y valor variables (1–12 kg / 1–300 mc). Sin reglas específicas.'),
(4,  'Anestésico (5 dosis)',             0.75,  75.0,  ''),
(5,  'Antiséptico (débil, 5 dosis)',     0.5,   25.0,  ''),
(6,  'Antiséptico (potente, 5 dosis)',   0.5,   50.0,  ''),
(7,  'Arcón',                            12.5,  30.0,  'Sin reglas específicas.'),
(8,  'Balanza',                          1.5,   20.0,  ''),
(9,  'Barril',                           35.0,  15.0,  'Sin reglas específicas.'),
(10, 'Bolsa',                            0.5,   1.0,   'Sin reglas específicas.'),
(11, 'Botella (crem)',                   1.5,   0.5,   'Sin reglas específicas.'),
(12, 'Botella (cristal)',                1.0,   1.0,   'Sin reglas específicas.'),
(13, 'Cadena (fina, 0,3 metros)',        0.25,  20.0,  ''),
(14, 'Cadena (gruesa, 3 metros)',        5.0,   20.0,  ''),
(15, 'Cálamo',                           0.05,  0.1,   'Sin reglas específicas.'),
(16, 'Catalejo',                         0.5,   500.0, ''),
(17, 'Cera (1 bloque)',                  0.1,   2.0,   'Sin reglas específicas.'),
(18, 'Cerradura y llave',               0.5,   50.0,  ''),
(19, 'Comida (buena, 1 día)',            0.25,  25.0,  ''),
(20, 'Comida (callejera, 1 día)',        0.75,  3.0,   ''),
(21, 'Comida (ración, 1 día)',           0.25,  0.2,   ''),
(22, 'Cubo',                             1.0,   1.0,   'Sin reglas específicas.'),
(23, 'Cuerda (15 metros)',              2.5,   30.0,  ''),
(24, 'Diapasón',                         0.25,  50.0,  ''),
(25, 'Escalera de mano (3 metros)',     10.0,  5.0,   'Sin reglas específicas.'),
(26, 'Espejo (de mano)',                1.0,   25.0,  'Sin reglas específicas.'),
(27, 'Estuche (cuero)',                 0.5,   4.0,   ''),
(28, 'Frasco o tarro',                  0.5,   1.0,   ''),
(29, 'Ganzúa',                           0.25,  5.0,   ''),
(30, 'Garfio de escalada',              2.0,   10.0,  ''),
(31, 'Gema sin recubrimiento (infusa)', 0.005, 2.0,   ''),
(32, 'Grilletes',                        3.0,   10.0,  ''),
(33, 'Instrumento musical',             0.25,  1.0,   'Peso y valor variables (250 g – 10 kg / 1–50 mc).'),
(34, 'Jabón',                            0.05,  1.0,   'Sin reglas específicas.'),
(35, 'Jarra o pichel',                  2.0,   2.0,   'Sin reglas específicas.'),
(36, 'Libro (de referencia)',           0.5,   10.0,  'Peso y valor variables (500 g – 2,5 kg / 10–500 mc).'),
(37, 'Linterna (aceite)',               1.0,   20.0,  ''),
(38, 'Linterna (esfera)',               1.0,   20.0,  ''),
(39, 'Lupa',                             0.1,   5.0,   ''),
(40, 'Manta',                            1.0,   2.0,   'Sin reglas específicas.'),
(41, 'Martillo (de mano)',              1.5,   4.0,   'Sin reglas específicas.'),
(42, 'Mochila',                          2.5,   8.0,   'Sin reglas específicas.'),
(43, 'Odre',                             0.5,   1.0,   'Sin reglas específicas.'),
(44, 'Olla (hierro)',                    5.0,   8.0,   'Sin reglas específicas.'),
(45, 'Pala',                             2.5,   8.0,   'Sin reglas específicas.'),
(46, 'Palanca',                          1.5,   10.0,  ''),
(47, 'Papel o pergamino (1 hoja)',      0.05,  0.5,   'Sin reglas específicas.'),
(48, 'Pedernal y acero',                0.75,  4.0,   ''),
(49, 'Perfume (1 vial)',                0.25,  20.0,  'Sin reglas específicas.'),
(50, 'Pico (minería)',                  5.0,   10.0,  'Sin reglas específicas.'),
(51, 'Piedra de afilar',                0.5,   0.2,   'Sin reglas específicas.'),
(52, 'Red (de caza)',                   2.5,   4.0,   ''),
(53, 'Red (de pesca)',                  7.5,   10.0,  ''),
(54, 'Ropa (buena)',                    3.0,   50.0,  'Valor variable (50–200 mc).'),
(55, 'Ropa (común)',                    1.5,   2.0,   ''),
(56, 'Ropa (raída)',                    0.75,  0.5,   ''),
(57, 'Saco',                             0.25,  0.2,   'Sin reglas específicas.'),
(58, 'Sistema de poleas',              6.0,   100.0, ''),
(59, 'Suministros quirúrgicos',         1.5,   20.0,  ''),
(60, 'Tienda de campaña (dos personas)', 10.0, 10.0,  'Sin reglas específicas.'),
(61, 'Tinta (vial de 30 ml)',           0.1,   40.0,  'Sin reglas específicas.'),
(62, 'Tratamiento (médico, 1 dosis)',  0.1,   10.0,  ''),
(63, 'Trompetilla',                      0.5,   50.0,  ''),
(64, 'Vela',                             0.1,   0.2,   ''),
(65, 'Veneno (débil, 1 dosis)',         0.1,   20.0,  ''),
(66, 'Veneno (efectivo, 1 dosis)',      0.1,   50.0,  ''),
(67, 'Veneno (potente, 1 dosis)',       0.1,   120.0, ''),
(68, 'Vial (cristal)',                  0.1,   4.0,   'Sin reglas específicas.');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""WeaponCatalog"";");
            migrationBuilder.Sql(@"DELETE FROM ""ArmorCatalog"";");
            migrationBuilder.Sql(@"DELETE FROM ""GearItems"";");
            migrationBuilder.Sql(@"DELETE FROM ""CatalogOptions"";");
        }
    }
}
