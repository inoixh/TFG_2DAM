using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra la información visual de un bufo o servicio disponible en la iglesia.
/// [Relaciones: ServiceData]
/// </summary>
public class ChurchSlot : MonoBehaviour
{
    [Header("Componentes Visuales")]
    public Image imagenBufo;
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoDescripcion;

    /// <summary>
    /// Asigna los datos del servicio a los elementos visuales de la interfaz.
    /// </summary>
    public void ConfigurarSlot(ServiceData servicio)
    {
        if (imagenBufo != null && servicio.icono != null)
        {
            imagenBufo.sprite = servicio.icono;
            imagenBufo.color = Color.white;
        }
        else
        {
            Debug.LogWarning(
                "Falta la imagen o el icono del servicio en la casilla de la iglesia."
            );
        }

        if (textoNombre != null)
        {
            textoNombre.text = servicio.nombreDisplay;
        }
        else
        {
            Debug.LogWarning("Falta el texto del nombre en la casilla de la iglesia.");
        }

        if (textoDescripcion != null)
        {
            textoDescripcion.text = servicio.descripcion;
        }
        else
        {
            Debug.LogWarning("Falta el texto de la descripción en la casilla de la iglesia.");
        }
    }
}
