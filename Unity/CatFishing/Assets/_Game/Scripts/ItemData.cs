using UnityEngine;

/// <summary>
/// Niveles de rareza disponibles para todos los elementos del juego.
/// </summary>
public enum Rareza
{
    Comun,
    Raro,
    Especial,
    Epico,
    Legendario,
}

/// <summary>
/// Contiene herramientas adicionales para trabajar con las rarezas.
/// </summary>
public static class RarezaExtensions
{
    /// <summary>
    /// Traduce la rareza a un texto decorado con color y formato visual para la interfaz.
    /// </summary>
    public static string NombreFormateado(this Rareza rareza)
    {
        string resultado;

        switch (rareza)
        {
            case Rareza.Comun:
                resultado = "<color=#55FF55>Común</color>";
                break;
            case Rareza.Raro:
                resultado = "<color=#5555FF>Raro</color>";
                break;
            case Rareza.Especial:
                resultado = "<color=#FFAA00>Especial</color>";
                break;
            case Rareza.Epico:
                resultado = "<color=#AA55FF>Épico</color>";
                break;
            case Rareza.Legendario:
                resultado = "<color=#FFFF55>Legendario</color>";
                break;
            default:
                resultado = "Común";
                break;
        }

        return resultado;
    }
}

/// <summary>
/// Molde de información para los peces, basura y cañas de pescar.
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
