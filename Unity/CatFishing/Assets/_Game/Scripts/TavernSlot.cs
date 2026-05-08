using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Muestra un botón con la imagen de la bebida en el menú de la taberna.
/// </summary>
public class ServiceSlot
    : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler
{
    [Header("Componentes Visuales")]
    public Image iconoServicio;

    private ServiceData datosServicio;

    /// <summary>
    /// Vincula la materia prima real a su silueta representativa y la ilumina.
    /// </summary>
    public void ConfigurarSlot(ServiceData servicio)
    {
        datosServicio = servicio;

        if (iconoServicio != null && servicio != null)
        {
            iconoServicio.sprite = servicio.icono;
            iconoServicio.color = Color.white;
        }
        else
        {
            Debug.LogWarning("Incapacidad visual para componer la carta de consumo de la taberna.");
        }
    }

    /// <summary>
    /// Traspasa su alma de contenido a la previsualización al recibir un estímulo primario táctil del jugador.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (datosServicio != null && TavernManager.Instance != null)
        {
            TavernManager.Instance.SeleccionarServicio(datosServicio);
        }
        else
        {
            Debug.LogWarning("O la bebida carece de consistencia o el tabernero desapareció.");
        }
    }

    /// <summary>
    /// Ensancha textualmente su identificación temporal por la presencia superpuesta.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        string textoInfo;

        if (datosServicio != null && UIManager.Instance != null)
        {
            textoInfo = $"{datosServicio.nombreDisplay} \n<size=70%>Consumible</size>";
            UIManager.Instance.MostrarTooltipTemporal(textoInfo, 1.5f);
        }
    }

    /// <summary>
    /// Borra cualquier señal indicativa sobrante cuando el jugador despeja su mirada central.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OcultarTooltip();
        }
    }
}
