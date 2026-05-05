using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Representa un servicio o consumible a la venta dentro de la interfaz de la Taberna.
/// Gestiona la comunicación con el TavernManager y los tooltips visuales.
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
    /// Asigna la información del consumible a esta casilla y activa visualmente su icono.
    /// </summary>
    public void ConfigurarSlot(ServiceData servicio)
    {
        datosServicio = servicio;
        if (iconoServicio != null && servicio != null)
        {
            iconoServicio.sprite = servicio.icono;
            iconoServicio.color = Color.white;
        }
    }

    /// <summary>
    /// Notifica al gestor de la taberna que el jugador ha seleccionado este consumible.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (datosServicio != null && TavernManager.Instance != null)
        {
            TavernManager.Instance.SeleccionarServicio(datosServicio);
        }
    }

    /// <summary>
    /// Extrae la información del consumible y solicita al UIManager que la muestre temporalmente.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (datosServicio != null && UIManager.Instance != null)
        {
            string textoInfo = $"{datosServicio.nombreDisplay} \n<size=70%>Consumible</size>";
            UIManager.Instance.MostrarTooltipTemporal(textoInfo, 1.5f);
        }
    }

    /// <summary>
    /// Oculta el tooltip de información al retirar el ratón de la casilla.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OcultarTooltip();
    }
}
