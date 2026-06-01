using UnityEngine;

/// <summary>
/// Define los niveles de escasez y valor de los objetos dentro del juego.
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
/// Proporciona métodos de extensión para formatear visualmente los nombres de las rarezas.
/// </summary>
public static class RarezaExtensions
{
    /// <summary>
    /// Devuelve el nombre de la rareza envuelto en etiquetas de color HTML para la interfaz.
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
/// Define las propiedades, precio y modelo 3D de un objeto o pez en el juego.
/// [Relaciones: Rareza]
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
