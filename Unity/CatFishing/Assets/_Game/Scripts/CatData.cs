using UnityEngine;

/// <summary>
/// Contenedor de datos para los NPCs Gatos registrados en la colección.
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
