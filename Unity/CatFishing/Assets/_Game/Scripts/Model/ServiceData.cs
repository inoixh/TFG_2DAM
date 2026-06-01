using UnityEngine;

/// <summary>
/// Estructura de datos para los servicios adquiribles en la taberna o bufos temporales.
/// [Relaciones: Ninguna]
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
