namespace Messages.Creacion.In;

/// <summary>Pasos liberados de la creación guiada: 0 = sin abrir; como mucho, los pasos del mundo (<c>Pasos</c> de la respuesta).</summary>
public class CreacionPasoRequest
{
    public int Paso { get; set; }
}
