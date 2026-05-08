using UnityEngine;

/// <summary>
/// Estructura de datos para crear gatos de forma ordenada desde el editor.
/// </summary>
public class CatData : ScriptableObject
{
    [Header("Identificación")]
    public string ID;
    public string nombreDisplay;

    [Header("Datos Visuales")]
    [TextArea(3, 5)]
    public string descripcion;
    public Rareza rareza;
    public Sprite icono;

    [Header("Preferencias")]
    public string pezFavoritoID;
}
