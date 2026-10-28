using System.Text.Json.Serialization;

namespace Messages.Creacion;

/// <summary>
/// Lo que una decisión del guion da a la ficha, sumado por la pantalla con la lógica de su registro: atributo → puntos, habilidad →
/// grados (campo de la ficha o nombre de una habilidad personalizada), pericias, metas (títulos) y objetos. Viaja con cada decisión en el
/// <c>PUT</c> de decisiones y en «Aplicar a la ficha»; en el registro de respuestas queda, por fila, lo pedido y lo hecho tras los topes,
/// para deshacerlo si la decisión cambia o se quita.
/// </summary>
public class EfectosFicha
{
    public Dictionary<string, int> Atributos { get; set; } = new();
    public Dictionary<string, int> Habilidades { get; set; } = new();
    public List<string> Pericias { get; set; } = [];
    public List<string> Metas { get; set; } = [];
    public List<string> Objetos { get; set; } = [];

    [JsonIgnore]
    public bool EstaVacio => Atributos.Count == 0 && Habilidades.Count == 0 && Pericias.Count == 0 && Metas.Count == 0 && Objetos.Count == 0;
}
