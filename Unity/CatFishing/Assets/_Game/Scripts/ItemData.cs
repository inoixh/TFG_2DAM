using UnityEngine;

public enum Rareza
{
    Comun,
    Raro,
    Especial,
    Epico,
    Legendario,
}

/// <summary>
/// Métodos de extensión para el enumerador Rareza.
/// Devuelve el texto formateado con emojis y colores para TextMeshPro.
/// </summary>
public static class RarezaExtensions
{
    public static string NombreFormateado(this Rareza rareza)
    {
        switch (rareza)
        {
            case Rareza.Comun:
                return "<color=#55FF55>Común</color>";
            case Rareza.Raro:
                return "<color=#5555FF>Raro</color>";
            case Rareza.Especial:
                return "<color=#FFAA00>Especial</color>";
            case Rareza.Epico:
                return "<color=#AA55FF>Épico</color>";
            case Rareza.Legendario:
                return "<color=#FFFF55>Legendario</color>";
            default:
                return "Común";
        }
    }
}

/// <summary>
/// Contenedor de datos para los objetos del juego (peces, cañas, etc.).
/// </summary>
[CreateAssetMenu(fileName = "NuevoItem", menuName = "Pesca/Nuevo Item")]
public class ItemData : ScriptableObject
{
    [Header("ID Único (Para Base de Datos)")]
    public string ID;

    [Header("Datos Visuales")]
    public string nombreDisplay;

    [TextArea(3, 5)]
    public string descripcion;
    public Sprite icono;
    public GameObject modelo3D;

    [Header("Configuración Pesca")]
    public bool esCanaDePescar;
    public Rareza rareza;

    [Range(0.1f, 3f)]
    public float dificultadMovimiento = 1f;

    [Header("Economía")]
    public int precioVenta = 10;
}
