using UnityEngine;

/// <summary>
/// Contenedor de datos para los servicios consumibles (Taberna) y bendiciones (Iglesia).
/// </summary>
public class ServiceData : ScriptableObject
{
    [Header("Identificación")]
    public string ID;
    public string nombreDisplay;
    public string tipo;

    [Header("Datos Visuales")]
    [TextArea(3, 5)]
    public string descripcion;
    public Sprite icono;

    [Header("Efectos Mecánicos")]
    public string objetivoEfecto;
    public float valorEfecto;

    [Header("Economía")]
    public int precioBase;
}
