using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAllNpcs : Migration
    {
        static readonly string[] Cols =
        [
            "Name","Source","Tipo","Ascendencia","Level",
            "Fuerza","Velocidad","Intelecto","Voluntad","Discernimiento","Presencia",
            "MaxHealth","MaxConcentration","MaxInvestiture",
            "Agilidad","ArmasLigeras","ArmasPesadas","Atletismo","Hurto","Sigilo",
            "Deduccion","Disciplina","Intimidacion","Manufactura","Medicina","Conocimiento",
            "Engano","Liderazgo","Percepcion","Perspicacia","Persuasion","Supervivencia",
            "Talentos","Apariencia","Notas","ImageUrl","CreatedAt","UpdatedAt"
        ];

        static readonly System.DateTime Ts = new(2026, 4, 8, 0, 0, 0, System.DateTimeKind.Utc);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix Anguila Aérea: was storing totals instead of base (total - attribute)
            migrationBuilder.Sql(@"
                UPDATE ""GlobalNpcs"" SET
                    ""Agilidad"" = 1, ""Sigilo"" = 1,
                    ""Percepcion"" = 2, ""Supervivencia"" = 3
                WHERE ""Name"" = 'Anguila Aérea'
            ");

            // Anguila Aérea Mayor
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Anguila Aérea Mayor","Caminapiedras","Rival Rango 1 – Animal Mediano","Animal",1,
                3,3,0,1,3,0,  24,3,0,
                2,0,0,0,0,1,  0,0,0,0,0,0,  0,0,2,0,0,3,
                "Sentidos aumentados: ventaja en pruebas de vista que no sean ataques.\nResbaladiza: no provoca Acometidas reactivas mientras nada.",
                "Como la anguila aérea pero de mayor tamaño, 2,10–2,40 metros.",
                "Movimiento: 3m, volar 9m, nadar 12m.\n1 Mordisco +5. Rasguño 3(1d6). Impacto 9(1d8+5); si Mediano o menor puede gastar 1 conc para Retener.\n2 Bomba en picado: vuela 18m hacia objetivo en tierra sin activar reactivas, Mordisco con ventaja. No puede volar hasta fin de escena.\n3 Constreñir: 15(3d6+5) a enemigo Retenido.",
                null, Ts, Ts
            });

            // Arquero
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Arquero","Caminapiedras","Secuaz Rango 1 – Humanoide Mediano","Humano",1,
                2,1,2,1,2,1,  12,3,0,
                2,2,2,0,0,0,  0,1,0,0,0,0,  0,0,2,0,0,2,
                "Secuaz: derrotado al sufrir una lesión; sus ataques no pueden provocar impactos críticos.",
                "",
                "Desvío: 1 (cuero).\nAcometida: Cuchillo +3. Rasguño 2(1d4). Impacto 5(1d4+3).\nAcometida: Arco largo +4, alcance 45/180m. Rasguño 3(1d6). Impacto 7(1d6+4).\nApuntar: primer turno sin Sorpresa, usa Obtener ventaja como acción gratuita.\nDisparo paralizante (1 conc): si enemigo se mueve a <45m recibe ataque; si impacta queda Inmovilizado hasta fin del siguiente turno.",
                null, Ts, Ts
            });

            // Axies
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Axies","Caminapiedras","Rival Rango 3 – Humanoide Mediano","Siah Aimiano",3,
                3,4,6,4,6,3,  60,6,0,
                2,0,0,0,4,4,  2,-1,0,0,0,4,  2,0,4,4,0,0,
                "Inmortal: no puede morir; si salud llega a 0 recupera 20(5d8) al inicio de su siguiente turno.\nMaldición de la amabilidad: debe subir la apuesta en pruebas difíciles; desventaja en tiradas del dado de trama.",
                "Siah aimiano que puede cambiar de forma sutilmente. No envejece ni muere por causas físicas.",
                "Inmunidades: Afligido, Agotado, Aturdido, Inconsciente.\nAcometida: Sin armas +3. Rasguño 2(1d4). Impacto 5(1d4+3).\nDato curioso (1 conc): Saber CD15; si supera, aliado ≤6m obtiene 2 conc. Oportunidad → ventaja; Complicación → desventaja en su siguiente prueba.\n2 Cambiar de forma (1 conc): oculta identidad; pruebas para ver a través del disfraz con desventaja.",
                null, Ts, Ts
            });

            // Bandido
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Bandido","Caminapiedras","Secuaz Rango 1 – Humanoide Mediano","Humano",1,
                1,1,1,0,2,1,  11,2,0,
                2,1,0,2,0,0,  0,-1,1,0,0,0,  0,0,1,0,0,3,
                "Secuaz: derrotado al sufrir una lesión; sus ataques no pueden provocar impactos críticos.\nInercia: si se mueve ≥3m en línea recta hacia un objetivo y lo ataca con la Maza en ese turno, obtiene ventaja en el ataque.",
                "",
                "Desvío: 1 (cuero).\nAcometida: Maza +2. Rasguño 3(1d6). Impacto 5(1d6+2).\nAcometida: Arco corto +2, alcance 24/96m. Rasguño 3(1d6). Impacto 5(1d6+2).\nDerribar (1 conc): Atletismo vs. Atletismo o Agilidad; si supera, Tumbado.",
                null, Ts, Ts
            });

            // Bersérker de la Emoción
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Bersérker de la Emoción","Caminapiedras","Rival Rango 1 – Humanoide Mediano","Humano",1,
                2,2,1,3,2,2,  22,5,0,
                0,1,2,2,0,0,  0,0,3,0,0,0,  0,1,2,0,0,0,
                "Emoción infecciosa: cuando un enemigo saca en prueba de ataque contra el bersérker, obtiene +3 a esa tirada; además puede rasguñar a un personaje cercano.",
                "",
                "Desvío: 2 (coraza).\n1 Acometida: Espada larga +4. Rasguño 4(1d8). Impacto 8(1d8+4); si Tumbado +4(1d8).\n1 Acometida: Daga +3, alcance 6/18m. Rasguño 2(1d4). Impacto 5(1d4+3).\n1 Golpe con escudo (1 conc): Atletismo vs. Defensa física; Tumbado si supera.\n0 Furia bersérker (2 conc): siguiente Acometida afecta a todos a ≤1,5m; +3(1d6) con impacto.\nr Arremeter (1 conc): tras ser atacado a ≤1,5m, inflige 4(1d8) al atacante.",
                null, Ts, Ts
            });

            // Cantor en forma de guerra
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Cantor en forma de guerra","Caminapiedras","Rival Rango 1 – Humanoide Mediano","Cantor",1,
                3,1,1,2,2,2,  24,4,0,
                0,2,1,2,0,0,  0,3,2,0,0,0,  0,2,2,0,0,0,
                "Caparazón externo: puede usar Prevenirse con su caparazón.\nExperiencia marcial: puede usar la acción Acometida dos veces en su turno.",
                "Cantor en forma de guerra con gruesa armadura de caparazón.",
                "Desvío: 2 (caparazón).\n1 Acometida: Hacha +4; puede saltar ≤3m antes o después. Rasguño 3(1d6). Impacto 7(1d6+4).\n1 Acometida: Arco corto +3, alcance 24/96m. Rasguño 3(1d6). Impacto 6(1d6+3).\nr Coordinación en pareja de guerra (1 conc): tras que otro cantor en forma de guerra a ≤1,5m use Moverse, este se desplaza hasta su movimiento terminando a ≤1,5m del primero.",
                null, Ts, Ts
            });

            // Cantor en forma diestra
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Cantor en forma diestra","Caminapiedras","Rival Rango 1 – Humanoide Mediano","Cantor",1,
                1,3,3,2,3,2,  20,4,0,
                2,2,0,0,0,1,  2,0,0,1,0,1,  0,0,2,0,0,3,
                "Velocidad letal: al inicio de escena sin Sorpresa puede jugar un turno rápido antes que cualquier otro personaje.\nSentidos agudos: puede gastar 1 conc para ventaja en Percepción.",
                "Cantor ágil sin caparazón, con mechones de pelo más largos.",
                "Movimiento: 9m.\nAcometida: Maza +5. Rasguño 3(1d6). Impacto 8(1d6+5).\nAcometida: Arco corto +5, alcance 24/96m. Rasguño 3(1d6). Impacto 8(1d6+5).\n2 Objetivo marcado (2 conc): elige enemigo a ≤36m; durante 1h los impactos infligen +3(1d6) y ventaja en Supervivencia para rastrearle.",
                null, Ts, Ts
            });

            // Caparácaro
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Caparácaro","Caminapiedras","Secuaz Rango 1 – Animal Pequeño","Animal",1,
                2,1,0,1,2,0,  9,3,0,
                0,0,0,0,0,2,  0,0,0,0,0,0,  0,0,2,0,0,0,
                "Secuaz: derrotado al sufrir una lesión; sus ataques no pueden provocar impactos críticos.\nCamuflaje agreste: sin ser detectado obtiene ventaja en ataques; inmóvil es casi indistinguible de una roca (Supervivencia CD14).",
                "Crustáceo del tamaño de un plato con caparazón rocoso. Al retraer extremidades se camufla con las rocas.",
                "2 Acometida: Pinzas +2. Rasguño 2(1d4). Impacto 4(1d4+2).\n2 Frenesí voraz (2 conc): se enfurece contra enemigo a ≤1,5m; caparácaros cercanos pueden unirse con r. El objetivo sufre 2(1d4) por laceración por cada caparácaro enfurecido.",
                null, Ts, Ts
            });

            // Chull
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Chull","Caminapiedras","Rival Rango 1 – Animal Grande o Enorme","Animal",1,
                4,0,0,1,3,0,  30,3,0,
                0,0,0,2,0,0,  0,0,0,0,0,0,  0,0,2,0,0,0,
                "Bestia de carga: capacidad de carga 750kg.\nAvance lento: solo puede usar una acción de Moverse por turno.",
                "Crustáceo enorme con gran caparazón poroso parecido a una piedra, antenas en forma de látigo y dos grandes garras.",
                "Desvío: 2 (caparazón). Movimiento: 3m.\nAcometida: Pinza +4, cercanía 3m. Rasguño 3(1d6). Impacto 9(1d6+6).\n2 Retraerse: retrae cabeza y patas; desvío aumenta a 6 y puede Prevenirse como a cubierto. Puede gastar 1 conc para mantener una ronda adicional.",
                null, Ts, Ts
            });

            // Espinablanca joven
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Espinablanca joven","Caminapiedras","Rival Rango 1 – Animal Mediano","Animal",1,
                2,3,0,2,3,0,  23,4,0,
                3,0,0,3,0,3,  0,0,1,0,0,0,  0,0,2,0,0,3,
                "Sentidos mejorados: ventaja en pruebas de olfato que no sean ataques.\nCaparazón espinoso: si enemigo a ≤1,5m le rasguña con arma, sufre 3(1d6) por laceración.",
                "Depredador del tamaño de una persona con hilera de púas en el caparazón dorsal. Se mueve sobre dos patas con dos pares de brazos con largas garras afiladas.",
                "Desvío: 1 (caparazón). Movimiento: 12m. Sentidos: 6m (olfato).\n1 Acometida: Garras +5. Rasguño 3(1d6). Impacto 8(1d6+5); puede gastar 1 conc para Afligido [1d4 vital].\n1 Colmillos empalantes +5. Rasguño 4(1d8). Impacto 9(1d8+5); si se movió ≥4,5m antes, +4(1d8) y Atletismo CD12 o Tumbado.\n2 Liberar feromonas (1 conc): no-espinablancas a ≤3m, Disciplina CD13 o Aturdidos hasta fin del siguiente turno.",
                null, Ts, Ts
            });

            // Experto
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Experto","Caminapiedras","Rival Rango 1 – Humanoide Mediano","Humano",1,
                1,1,1,2,2,1,  18,4,0,
                1,1,0,1,0,0,  1,0,0,1,0,0,  1,0,2,2,2,0,
                "Competencia: ventaja en pruebas de habilidad relacionadas con su profesión.\nRostro severo: tras gastar conc para resistirse a la influencia de alguien, ese personaje pierde 1 conc.",
                "",
                "Acometida: Arma improvisada +2. Con Complicación se rompe. Rasguño 2(1d4). Impacto 3(1d4+2).\nContraargumento: durante conversación, Persuasión vs. Defensa espiritual; si supera, objetivo pierde 3 conc.",
                null, Ts, Ts
            });

            // Fanático
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Fanático","Caminapiedras","Secuaz Rango 1 – Humanoide Mediano","Humano",1,
                3,1,1,2,1,2,  12,4,0,
                2,0,1,0,0,0,  0,0,2,0,0,0,  0,0,2,0,0,1,
                "Secuaz: derrotado al sufrir una lesión; sus ataques no pueden provocar impactos críticos.\nExaltación: pruebas de Intimidación y Persuasión contra el fanático con desventaja.\nEnajenación: mientras tiene ≤ mitad de salud, no puede Prevenirse pero movimiento +3m y tiradas de daño con ventaja.",
                "Fanáticos seguidores de Ylt. Visten túnicas verdes y portan espadas.",
                "1 Acometida: Espada larga +4. Rasguño 4(1d8). Impacto 8(1d8+4).\n1 Prevenirse: alza su escudo; ataques en su contra con desventaja hasta inicio de su siguiente turno.\n1 Inspirar fanatismo (1 conc): aliado a ≤6m puede usar inmediatamente Acometida como r.",
                null, Ts, Ts
            });

            // Fanático en forma sombría
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Fanático en forma sombría","Caminapiedras","Rival Rango 1 – Humanoide Mediano","Cantor",1,
                1,4,1,3,2,2,  22,5,4,
                1,2,0,0,0,3,  0,0,3,0,0,0,  0,0,2,0,0,1,
                "Incorporal: ataques contra su Defensa física con desventaja; ignora terreno difícil y puede atravesar huecos ≥15cm.\n0 Manifestarse: forma más sólida hasta inicio de su siguiente turno; pierde Incorporal pero puede atacar en zonas iluminadas.",
                "Atrapado entre los Reinos Físico y Cognitivo. Antes fue un cantor que vinculó un sombraspren a su gema corazón.",
                "Movimiento: 9m. Sentidos: 3m (vista); 12m en oscuridad.\n1 Acometida: Garras sombrías +6 (solo en oscuridad o tras Manifestarse). Rasguño 4(1d8) esp. Impacto 10(1d8+6) esp.\n1 Arrastre por las sombras (1 inv): se transporta a zona oscura ≤9m; puede llevar a personaje a ≤1,5m (Agilidad vs. Defensa física si reticente).\n1 Canto de duelo (1 inv): Intimidación vs. Defensa espiritual de objetivo ≤9m; si supera, Agotado [-1].",
                null, Ts, Ts
            });

            // Guardia
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Guardia","Caminapiedras","Rival Rango 1 – Humanoide Mediano","Humano",1,
                3,1,1,2,3,1,  24,4,0,
                0,0,2,2,0,0,  0,1,1,0,0,0,  0,0,2,1,0,0,
                "¡A las armas!: tras detectar a un enemigo, este guardia y todos los guardias aliados que puedan sentirle obtienen Concentrado hasta fin de escena.",
                "",
                "Desvío: 2 (cota de malla).\nAcometida: Lanza larga +5. Rasguño 4(1d8). Impacto 9(1d8+5); puede gastar 1 conc para Tumbado.\n2 Debilitar (2 conc): +5 contra objetivo Tumbado. Rasguño 9(2d8). Impacto 14(2d8+5).\nCoordinación (1 conc): tras impactar, un aliado puede Destrabarse inmediatamente.",
                null, Ts, Ts
            });

            // Kaiana
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Kaiana","Caminapiedras","Rival Rango 1 – Humanoide Mediano","Humana Reshi",1,
                1,3,1,2,3,3,  34,4,5,
                2,2,0,0,0,0,  0,1,0,0,2,0,  0,1,2,2,0,0,
                "Aliada apasionada (1 conc): al usar Obtener ventaja con éxito, un aliado ≤18m también obtiene ventaja en su siguiente prueba contra el objetivo.\nHuida rápida: una vez por escena usa Destello desorientador como acción gratuita y obtiene 2 acciones extras (solo Interactuar, Destrabarse, Moverse).\nPotencias: Iluminación +4 (1 grado), Progresión +5 (2 grados).",
                "Vigilante de la Verdad reshi vinculada a la brumaspren Horizontes-Siempre-A-La-Deriva. Aprendiz de Ylt.",
                "Movimiento: 9m.\n1 Acometida: Cuchillo +5, alcance 6/18m. Rasguño 1(1d4). Impacto 7(1d4+5); puede gastar 1 conc para Ralentizado.\n1 Ilusión distractiva (1 inv): duplicado de sí misma o aliado ≤9m; ataques con desventaja y no pueden rasguñar. Termina si un ataque falla o al fin de escena.\n2 Destello desorientador (1 inv): Iluminación vs. Defensa cognitiva de personajes a ≤1,5m; Desorientados hasta fin de su siguiente turno.\n2 Regeneración (1 inv): restablece 8(1d6+5) a sí misma o a quien toca.\nr Crecimiento espontáneo (1 inv): antes de impacto cuerpo a cuerpo sobre ella o aliado ≤6m, Progresión vs. Defensa física del atacante; si supera, planta crece alrededor (Inmovilizado, sin Acometidas reactivas).",
                null, Ts, Ts
            });

            // Khornak
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Khornak","Caminapiedras","Rival Rango 1 – Animal Mediano","Animal",1,
                3,2,0,2,2,0,  26,4,0,
                0,0,0,2,0,2,  0,0,0,0,0,0,  0,0,1,0,0,2,
                "Emboscada aterradora: al inicio de escena todos los enemigos que puedan sentirle hacen Disciplina CD14; los que fallen no pueden jugar turno rápido sin gastar 2 conc.\nDepredador implacable: cuando ataca a un enemigo que no ha jugado turno en esta ronda, +1d8 de daño.",
                "Acecha en aguas poco profundas. Enormes mandíbulas con múltiples hileras de dientes, extremidades palmeadas con caparazón y ocho ojos brillantes.",
                "Desvío: 2 (caparazón). Movimiento: 7,5m, nadar 9m.\nAcometida: Mandíbula aplastante +5. Rasguño 4(1d8). Impacto 9(1d8+5) y Retiene; 1 conc para Afligido [1d4 vital]. Escapar: Agilidad/Atletismo CD16 o el Profundo sufre ≥20 daño en un turno.\nColetazo +5; hasta 2 conc para afectar ese nº adicional de objetivos. Rasguño 3(1d6). Impacto 8(1d6+5) y Atletismo CD15 o Tumbado.\nArrastrar: se mueve ≤4,5m arrastrando al enemigo Retenido.",
                null, Ts, Ts
            });

            // Ladrón
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Ladrón","Caminapiedras","Rival Rango 1 – Humanoide Mediano","Humano",1,
                1,3,2,1,2,1,  20,3,0,
                2,2,0,0,2,2,  0,0,0,0,0,0,  2,0,2,0,2,0,
                "Manos rápidas: cuando un personaje provoca Acometida reactiva del ladrón, puede usar Hurtar como r en su lugar.\nDesaparecer: al final de su turno con cobertura puede hacer prueba de Sigilo con ventaja; si tiene éxito el enemigo le pierde la pista.",
                "",
                "Desvío: 1 (cuero).\n1 Acometida: Daga +5, alcance 6/18m. Rasguño 2(1d4). Impacto 7(1d4+5); si Desorientado, +2(1d4).\n1 Desorientar (1 conc): enemigos a ≤3m, Agilidad CD13 o Desorientados hasta fin del siguiente turno.\n2 Hurtar (2 conc): Hurto vs. Percepción; si supera, roba ≤50 marcos o un objeto no empuñado.\nr Resbaladizo: tras que enemigo termine Moverse a ≤1,5m, puede Destrabarse como r.",
                null, Ts, Ts
            });

            // Lancero
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Lancero","Caminapiedras","Secuaz Rango 1 – Humanoide Mediano","Humano",1,
                2,2,1,1,2,1,  14,3,0,
                0,1,2,2,0,0,  0,0,1,0,0,0,  0,0,2,0,0,0,
                "Secuaz: derrotado al sufrir una lesión; sus ataques no pueden provocar impactos críticos.\nAdiestramiento marcial: al inicio de escena con escudo y sin Sorpresa, obtiene beneficios de Prevenirse hasta su primer turno.\nTácticas militares: una vez por ronda puede gastar 1 conc adicional para usar reacción Ayudar o Acometida reactiva sin gastar su r.",
                "",
                "Desvío: 2 (malla).\nAcometida: Lanza corta +3. Rasguño 4(1d8). Impacto 7(1d8+3); si Tumbado, +4(1d8).\nAcometida: Arco corto +3, alcance 24/96m. Rasguño 3(1d6). Impacto 6(1d6+3).\nGolpe con escudo: Atletismo vs. Defensa física; Tumbado si supera.",
                null, Ts, Ts
            });

            // Maestro lancero
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Maestro lancero","Caminapiedras","Rival Rango 2 – Humanoide Mediano","Humano",2,
                3,2,1,3,2,1,  38,5,0,
                3,2,3,1,0,0,  0,2,2,0,0,0,  0,0,3,0,0,0,
                "Floritura defensiva: puede blandir su lanza a la defensiva, usando Prevenirse como si tuviera escudo.\nSin piedad: ventaja adicional en ataques contra objetivos Tumbados.",
                "",
                "Desvío: 1 (cuero).\nAcometida: Lanza larga +6, cercanía 3m. Rasguño 4(1d8). Impacto 10(1d8+6); puede gastar 1 conc para Tumbado.\nPatada +4. Rasguño 2(1d4). Impacto 6(1d4+4); si Mediano o menor, puede gastar 1 conc para empujarlo 1,5m.\n2 Atravesar (1 conc): +6 contra ≤2 objetivos a ≤1,5m el uno del otro. Rasguño 4(1d8). Impacto 10(1d8+6).",
                null, Ts, Ts
            });

            // Lilinum
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Lilinum","Caminapiedras","Rival Rango 2 – Humanoide Mediano","Fusionada (Cantor)",2,
                2,4,6,3,4,1,  41,5,6,
                3,3,0,3,0,0,  0,3,3,0,0,0,  0,0,2,0,0,3,
                "Agresión instintiva: no necesita gastar conc para Acometidas reactivas.\nMortaja de polvo: ataques a distancia desde ≥7,5m basados en vista u olfato con desventaja.\nDescomponer arma (r): tras sufrir impacto o rasguño de arma no Investida, convierte esa arma en polvo.\nPotencia: División +9 (3 grados).",
                "Devastadora Fusionada con caparazón de rayas blancas y rojas. Empuña una hoja esquirlada.",
                "Desvío: 2 (caparazón).\n1 Acometida: Garras +7. Rasguño 2(1d4). Impacto 9(1d4+7) + 3(1d6) esp.\n1 Acometida: Hoja esquirlada +7. Rasguño 8(2d8) esp. Impacto 15(2d8+7) esp.\n2 Toque devastador +9. Rasguño 10(3d6) esp. Impacto 19(3d6+9) esp.\n2 Potencia de División (2 inv): destruye objeto Grande ≤9m a su antojo; Agilidad/Atletismo CD15 o 13(3d8) daño y efectos varios.\n0 Revitalizar (1 inv): recupera 5(1d6+2); puede usarse inconsciente.",
                null, Ts, Ts
            });

            // Agente de los Ojos de Pala
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Agente de los Ojos de Pala","Caminapiedras","Rival Rango 1 – Humanoide Mediano","Humano",1,
                2,2,1,2,3,2,  25,4,0,
                0,2,0,2,0,2,  0,1,0,0,0,0,  0,0,2,2,0,0,
                "0 Defensa perspicaz: Perspicacia vs. Defensa espiritual de objetivo ≤18m; si supera, el objetivo no puede obtener ventajas en ataques contra el agente durante 1 minuto.\nr Aprovechar la brecha: tras que un enemigo a ≤1,5m le rasguñe, Acometida reactiva como si hubiera salido voluntariamente de su cercanía.\nr Huida rápida: tras que aliado a ≤1,5m impacte a un objetivo, se mueve hasta la mitad de su movimiento sin activar reactivas.",
                "Agente de la Heraldo Pailiah. Al servicio de los Ojos de Pala.",
                "1 Acometida: Espada lateral +4. Rasguño 3(1d6). Impacto 7(1d6+4); si Tumbado pierde 1 conc.\n1 Acometida: Arco corto +4, alcance 24/96m. Rasguño 3(1d6). Impacto 7(1d6+4); puede gastar 1 conc para Tumbado.",
                null, Ts, Ts
            });

            // Vigía de los Ojos de Pala
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Vigía de los Ojos de Pala","Caminapiedras","Rival Rango 2 – Humanoide Mediano","Humano",2,
                2,3,2,3,4,2,  42,5,0,
                0,2,2,2,0,3,  0,2,1,0,0,0,  0,0,3,3,0,0,
                "0 Defensa perspicaz: Perspicacia vs. Defensa espiritual de objetivo ≤18m; si supera, el objetivo no puede obtener ventajas en ataques contra el vigía durante 1 minuto.\nr Huida oportunista: tras que un enemigo a ≤1,5m le rasguñe, Acometida contra el atacante y luego se mueve hasta la mitad de su movimiento sin activar reactivas.",
                "Agentes diestros de Pailiah con venda de ojos ceremonial verde. Su perspicacia trasciende la necesidad de la vista.",
                "Movimiento: 9m. Sentidos: 6m (oído).\nAcometida: Espada larga +5. Rasguño 4(1d8). Impacto 11(1d8+7) y Ralentizado hasta fin del siguiente turno del vigía.\n2 Abanico de hojas (2 conc): +5 contra uno o más objetivos a ≤1,5m. Rasguño 4(1d8). Impacto 9(1d8+5) y Afligido [1d8 vital].",
                null, Ts, Ts
            });

            // Plebeyo
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Plebeyo","Caminapiedras","Secuaz Rango 1 – Humanoide Mediano","Humano",1,
                0,0,0,0,0,0,  10,2,0,
                0,0,0,1,0,0,  0,0,0,2,0,0,  0,0,2,2,0,0,
                "Secuaz: derrotado al sufrir una lesión; sus ataques no pueden provocar impactos críticos.\nCompetencia: ventaja en pruebas de habilidad relacionadas con su profesión.",
                "Ciudadanos típicos de Roshar: agricultores, panaderos, tejedores, escribas.",
                "Acometida: Arma improvisada +1. Con Complicación se rompe. Rasguño 2(1d4). Impacto 3(1d4+1).\nr Distraer (1 conc): cuando un aliado hace prueba contra un enemigo en su cercanía, el plebeyo le confiere ventaja.",
                null, Ts, Ts
            });

            // Portador de esquirlada duelista
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Portador de esquirlada duelista","Caminapiedras","Rival Rango 2 – Humanoide Mediano","Humano",2,
                3,4,2,3,2,4,  40,5,0,
                2,3,3,0,0,0,  0,2,3,0,0,0,  0,2,0,2,2,0,
                "Liderazgo inspirador: al usar Obtener ventaja con éxito, un aliado que puede influir obtiene ventaja en su siguiente prueba contra el mismo objetivo.",
                "Portador de hoja esquirlada entrenado en duelo. Puede invocarla en diez latidos de corazón.",
                "1 Acometida: Hoja esquirlada +6. Rasguño 9(2d8) esp. Impacto 22(2d8+6) esp.\n1 Cambio de posición (1 conc): adopta posición de fuego (acción gratuita extra ofensiva) o posición de viento (acción gratuita extra de Destrabarse o ataque).\n1 Finta (1 conc): Armamento pesado vs. Defensa cognitiva ≤1,5m; si supera, pierde r y 1d4 conc; si falla pierde 1 conc.\n1 Perspicacia brutal: Perspicacia vs. Disciplina del objetivo; si tiene éxito, el objetivo pierde 1 acción al inicio de su siguiente turno.\nr Reposicionarse (1 conc): antes de ser desplazado contra su voluntad, ignora el efecto y usa Destrabarse como acción gratuita.",
                null, Ts, Ts
            });

            // Portador del Polvo del Segundo Ideal
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Portador del Polvo del Segundo Ideal","Caminapiedras","Rival Rango 2 – Humanoide Mediano","Humano",2,
                2,3,3,2,3,1,  40,4,5,
                3,3,0,0,0,3,  0,1,1,0,0,0,  0,2,2,0,0,0,
                "Potencias: Abrasión +5 (2 grados), División +6 (3 grados).",
                "Caballero Radiante de la orden de los Portadores del Polvo, vinculado a cenizaspren.",
                "Desvío: 2 (malla). Movimiento: 12m (9m sin inv).\nAcometida: Espada +6. Rasguño 3(1d6). Impacto 9(1d6+6); puede gastar 1 conc para Destrabarse como acción gratuita.\nAcometida: Arco corto +6, alcance 24/96m. Rasguño 3(1d6). Impacto 9(1d6+6).\nPutridez ineludible (1 inv): +6 vs. def. espiritual. Rasguño 13(3d8) esp. Impacto 19(3d8+6) esp. y Agilidad CD14 o armadura pierde 1 desvío.\n3 Explosión ardiente (2 inv): estallido, 4(1d8) por energía a todos ≤3m; +1 conc para chispas a ≤9m (Agilidad CD14 o pierden 1 acción).\nFuga erosionada (1 inv): elimina Inmovilizado, Retenido o desventaja física de sí mismo o aliado cercano.\n0 Revitalizar (1 inv): recupera 5(1d6+2).\n0 Patinar (2 conc): se mueve en línea recta hasta su movimiento.",
                null, Ts, Ts
            });

            // Profundo
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Profundo","Caminapiedras","Rival Rango 2 – Humanoide Mediano","Fusionado (Cantor)",2,
                3,5,1,6,4,2,  45,8,6,
                2,3,0,0,0,2,  0,5,0,0,0,0,  0,0,3,2,0,0,
                "Tonos puros: ignora Desorientado mientras está sobre piedra sólida o sumergido.\nNavegar por la piedra: puede moverse a través de superficies sólidas como si fuera suelo (no madera); sumergido gasta 1 conc por acción que no sea Moverse o Revitalizar.\nForma fundida (reacción gratuita): antes de sufrir daño de golpe/laceración de fuente no de madera, aumenta desvío a 6 contra ese daño.\nPotencia: Cohesión +9 (3 grados).",
                "Fusionado de la marca makay-im. Piel lisa sin pelo, escaso caparazón, extremidades largas, ojos blancos con brillo rojo.",
                "Movimiento: 12m. Sentidos: 6m (vista y oído bajo el suelo).\n1 Acometida: Uñas de caparazón +8. Rasguño 7(2d6). Impacto 15(2d6+8).\n1 Acometida: Derribo +8. Rasguño 3(1d6). Impacto 11(1d6+8) y Tumbado.\n2 Aferrar: objetivo ≤1,5m, Agilidad CD17 o Retenido; si además Tumbado, empieza a asfixiarse (Afligido [2d10 vital]). Escapar: Agilidad CD17 o ≥20 daño en un turno.\n1 Potencia de Cohesión (1 inv): remodela zona u objeto Grande ≤9m de piedra o tierra (crea/elimina cobertura, terreno difícil, Retenido; Agilidad/Atletismo CD15 para evitar estado).\n0 Revitalizar (1 inv): recupera 5(1d6+2).\nr Atrapar: cuando enemigo provoca reactiva mientras está sumergido, puede usar Aferrar como r.",
                null, Ts, Ts
            });

            // Regio en forma funesta
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Regio en forma funesta","Caminapiedras","Rival Rango 2 – Humanoide Mediano","Regio (Cantor)",2,
                5,3,1,3,3,1,  42,5,5,
                2,3,3,3,0,0,  0,3,3,0,0,0,  0,0,2,0,0,0,
                "Experiencia marcial: puede usar Acometida dos veces en su turno.\nVigilancia marcial: le cuesta 1 conc menos resistirse a la influencia de un enemigo.\nCaparazón con púas: puede Prevenirse con su caparazón; mientras se beneficia de Prevenirse, la primera vez por turno que recibe impacto/rasguño cuerpo a cuerpo o le agarran/empujan con éxito, el enemigo sufre 3(1d6) por laceración.",
                "Cantor en forma funesta, más de dos metros, caparazón casi tan resistente como armadura esquirlada.",
                "Desvío: 4 (caparazón).\nAcometida: Martillo +8; ventaja si se movió ≥3m en línea recta hacia el objetivo. Rasguño 5(1d10). Impacto 13(1d10+8).\nAcometida: Arco largo +8, alcance 45/180m. Rasguño 3(1d6). Impacto 11(1d6+8).\nMejora investida (1 inv): Mejorado [FUE+1] y [VEL+1] hasta fin de su siguiente turno; puede gastar 1 inv para mantener una ronda adicional.\nr Firmeza (1 conc): si alguien intenta desplazarle o Tumbarle, ignora el efecto.\nr Placaje: en lugar de Acometida reactiva, puede Agarrar al enemigo; si supera, el enemigo sufre +3(1d6) de las púas.",
                null, Ts, Ts
            });

            // Regio en forma tormenta
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Regio en forma tormenta","Caminapiedras","Rival Rango 2 – Humanoide Mediano","Regio (Cantor)",2,
                2,4,2,4,1,3,  40,6,5,
                3,2,0,3,0,0,  0,5,6,0,0,0,  0,0,3,0,0,0,
                "Debilidad ante el agua: mientras empapado no puede usar Lanzar relámpago ni Carga eléctrica.\nCaparazón punzante: mientras Retenido por Agarrar, el personaje que le limita queda Afligido [1d8 por laceración].",
                "Cantor en forma tormenta con armadura sutil y caparazón con púas. El aire a su alrededor se electrifica con relámpagos rojos.",
                "Desvío: 1 (caparazón).\nAcometida: Lanza corta +6. Rasguño 4(1d8). Impacto 10(1d8+6).\n2 Lanzar relámpago (1 inv): +7, alcance 18m; desventaja a menos que gaste 1 conc para ignorarla. Rasguño 8(2d8) energía. Impacto 15(2d8+7) energía.\nSalto de tormenta (1 inv): salta ≤18m; si aterriza a ≤1,5m de un enemigo puede usar Obtener ventaja como acción libre.\nCarga eléctrica (acción libre tras Moverse): inmune a energía; cuando le impactan a ≤1,5m el atacante sufre 4(1d8) energía.",
                null, Ts, Ts
            });

            // Rompedor del Cielo del Segundo Ideal
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Rompedor del Cielo del Segundo Ideal","Caminapiedras","Rival Rango 2 – Humanoide Mediano","Humano",2,
                2,3,2,3,4,1,  40,5,6,
                3,3,3,0,0,0,  3,2,2,0,0,2,  0,0,2,3,0,0,
                "Búsqueda de la verdad: ventaja en Deducción y Perspicacia para determinar la verdad.\nr Amenaza: tras sufrir un ataque, Intimidación vs. Defensa cognitiva del atacante; si supera, pierde 1 conc.\nPotencia: Gravitación +6 (2 grados).",
                "Caballero Radiante de la orden de los Rompedores del Cielo, vinculado a altospren. Defienden estrictamente la ley.",
                "Acometida: Espada +6. Rasguño 3(1d6). Impacto 9(1d6+6).\nDisparo con Enlace (1 inv): +6, alcance 9m; lanza objeto Mediano o menor. Rasguño 9(2d8). Impacto 15(2d8+6).\nGravitación ofensiva (1 inv): Gravitación vs. Defensa física; si supera, desplaza ≤7,5m en cualquier dirección y Retenido; si golpea superficie, 9(2d8) por golpe.\nSoporte gravitacional (1 inv): vuelo 7,5m hasta inicio de su siguiente turno; puede extender gastando inv; o infunde vuelo a aliado.\n0 Revitalizar (1 inv): recupera 5(1d6+2).",
                null, Ts, Ts
            });

            // Sabueso-hacha
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Sabueso-hacha","Caminapiedras","Secuaz Rango 1 – Animal Pequeño","Animal",1,
                2,2,0,0,3,0,  12,2,0,
                2,0,0,2,0,1,  0,0,0,0,0,0,  0,0,2,0,0,4,
                "Sentidos mejorados: ventaja en pruebas de olfato que no sean ataques.\nSecuaz: derrotado al sufrir una lesión; sus ataques no pueden provocar impactos críticos.",
                "Depredador doméstico de seis patas, cola similar a la de un pez y antenas plumosas. Vocaliza con un barritar en eco.",
                "Movimiento: 12m. Sentidos: 12m (olfato).\nAcometida: Mordisco +4. Rasguño 2(1d4). Impacto 6(1d4+4); si Mediano o menor, puede gastar 1 conc para Tumbado y moverse 3m arrastrándolo.\nInstintos de manada (acción libre): mientras a ≤1,5m de un aliado, puede usar Obtener ventaja como acción libre.\nr A la caza: tras que enemigo a ≤9m caiga Tumbado, se mueve ≤4,5m hacia él.",
                null, Ts, Ts
            });

            // Taszo
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Taszo","Caminapiedras","Rival Rango 1 – Humanoide Mediano","Humano Shin",1,
                0,3,1,2,2,1,  17,4,0,
                0,0,0,0,0,0,  1,2,0,0,0,1,  0,0,2,2,2,0,
                "Determinado: tras gastar conc para resistirse a la influencia de un personaje, ese personaje pierde 1 conc.\nBrazo roto: solo puede usar una mano.",
                "Humano shin serio e inquisitivo. Chamán de piedra experto en combate. Único superviviente de su convoy.",
                "Movimiento: 9m. Idiomas: alezi, shin.\n1 Acometida: Espada lateral +3. Rasguño 3(1d6). Impacto 8(1d6+3).\nr Amistad en apuros (1 conc): cuando un aliado a ≤1,5m falla una prueba, Taszo añade una Oportunidad al resultado.",
                null, Ts, Ts
            });

            // Veth
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Veth","Caminapiedras","Jefe Rango 1 – Humanoide Mediano","Humano Alezi",1,
                2,2,1,1,3,2,  40,3,0,
                0,1,0,1,0,2,  0,0,0,0,0,0,  0,0,2,2,0,0,
                "Jefe: puede jugar un turno rápido y uno lento por ronda; puede gastar 1 conc para acción adicional tras el turno de un enemigo; puede gastar 1 conc en su turno para eliminar un estado.\nAcometidas dirigidas: mientras tiene ventaja al atacar, ignora el desvío del objetivo.",
                "Humano alezi. Agente de los Ojos de Pala con reflejos ultrarrápidos y capacidad de leer a sus oponentes.",
                "Idiomas: alezi, iriali.\n1 Acometida: Espada lateral +3. Rasguño 3(1d6). Impacto 6(1d6+3).\n1 Aprovechar ventaja: se mueve hasta su movimiento; si mueve ≥3m y termina a ≤1,5m de enemigo no atacado esta ronda, el movimiento no activa reactivas y hace Percepción vs. Defensa espiritual; si supera, ventaja en su próximo ataque contra dicho objetivo.\n0 Defensa perspicaz: Perspicacia vs. Defensa espiritual ≤18m; si supera, no puede obtener ventajas en ataques contra Veth durante 1 minuto.\nr Redirección intuitiva (1 conc): antes de que un ataque le falle, lo redirige contra un objetivo a ≤1,5m (no puede ser el atacante).",
                null, Ts, Ts
            });

            // Ylt
            migrationBuilder.InsertData("GlobalNpcs", Cols, new object[]
            {
                "Ylt","Caminapiedras","Jefe Rango 1 – Humanoide Mediano","Humano Iriali",1,
                2,3,3,3,3,4,  80,5,6,
                0,2,0,0,0,0,  3,3,0,0,0,0,  0,2,2,2,0,0,
                "Jefe: puede jugar un turno rápido y uno lento por ronda; puede gastar 1 conc para acción adicional tras el turno de un enemigo; puede gastar 1 conc en su turno para eliminar un estado.\nTierra fluida: tras usar Cohesión puede usar Piedra de salto sobre sí mismo de forma gratuita (sin ventaja en ataque).\nHoja de Honor de Taln: vinculado a esta hoja; si la pierde puede invocarla como acción y reaparece en su siguiente turno rápido. Sin ella no puede usar Cohesión ni Tensión.\nPotencias: Cohesión +6 (3 grados), Iluminación +6 (2 grados), Progresión +5 (2 grados), Tensión +5 (3 grados).",
                "Humano iriali. Arrogante Vigilante de la Verdad vinculado al brumaspren Tyche. Líder de los Ojos de Pala.",
                "Idiomas: alezi, iriali.\n1 Acometida: Hoja de Honor +5. Rasguño 11(2d10) esp. Impacto 16(2d10+5) esp.\n2 Lanza de piedra (1 inv): Cohesión +6, alcance 18m. Rasguño 9(2d8). Impacto 15(2d8+6); puede gastar Oportunidad para Tumbado.\n1 Floritura ilusoria (1 inv): Iluminación vs. Defensa espiritual de enemigo al que ha impactado cuerpo a cuerpo desde su último turno; si supera, Desorientado hasta fin de su siguiente turno.\n1 Visión repulsiva (1 inv): recupera 1d4 conc.\n1 Piedra de salto (1 inv): pilar de piedra impulsa a él o aliado ≤4,5m; aterriza ≤4,5m sin activar reactivas; si el siguiente ataque es cuerpo a cuerpo, obtiene ventaja.\n2 Regeneración (1 inv): restablece 1d6+5 de salud a sí mismo o a quien toca.\n0 Revitalizar (1 inv): recupera 4(1d6+1).\nr Parada de tensión (1 inv): antes de impacto o rasguño contra Defensa física, Tensión en su ropa; Defensa física +2 hasta inicio de su siguiente turno, incluso contra el ataque desencadenante.",
                null, Ts, Ts
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""GlobalNpcs"" SET
                    ""Agilidad"" = 4, ""Sigilo"" = 4,
                    ""Percepcion"" = 5, ""Supervivencia"" = 4
                WHERE ""Name"" = 'Anguila Aérea'
            ");

            foreach (var name in new[]
            {
                "Anguila Aérea Mayor","Arquero","Axies","Bandido","Bersérker de la Emoción",
                "Cantor en forma de guerra","Cantor en forma diestra","Caparácaro","Chull",
                "Espinablanca joven","Experto","Fanático","Fanático en forma sombría","Guardia",
                "Kaiana","Khornak","Ladrón","Lancero","Maestro lancero","Lilinum",
                "Agente de los Ojos de Pala","Vigía de los Ojos de Pala","Plebeyo",
                "Portador de esquirlada duelista","Portador del Polvo del Segundo Ideal",
                "Profundo","Regio en forma funesta","Regio en forma tormenta",
                "Rompedor del Cielo del Segundo Ideal","Sabueso-hacha","Taszo","Veth","Ylt"
            })
                migrationBuilder.DeleteData("GlobalNpcs", "Name", name);
        }
    }
}
