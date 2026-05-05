using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa visualmente una bendición o bufo activo en la interfaz de la Iglesia.
/// </summary>
public class ChurchSlot : MonoBehaviour
{
    [Header("Componentes Visuales")]
    public Image imagenBufo;
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoDescripcion;

    /// <summary>
    /// Asigna la información del servicio a los elementos visuales de la casilla.
    /// </summary>
    public void ConfigurarSlot(ServiceData servicio)
    {
        if (imagenBufo != null && servicio.icono != null)
        {
            imagenBufo.sprite = servicio.icono;
            imagenBufo.color = Color.white;
        }
        if (textoNombre != null)
            textoNombre.text = servicio.nombreDisplay;
        if (textoDescripcion != null)
            textoDescripcion.text = servicio.descripcion;
    }
}
