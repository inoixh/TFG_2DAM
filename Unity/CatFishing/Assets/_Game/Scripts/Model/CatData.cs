using UnityEngine;

/// <summary>
/// Contiene los datos persistentes e información visual de un gato en el catálogo.
/// [Relaciones: Rareza]
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
