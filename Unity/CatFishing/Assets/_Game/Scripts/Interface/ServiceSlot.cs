using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Representa visualmente un consumible ofrecido en la taberna.
/// [Relaciones: ServiceData, TavernManager, UIManager]
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
    /// Asigna los datos y la imagen del servicio disponible en la taberna.
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
            Debug.LogWarning(
                "Faltan los datos visuales para configurar el servicio de la taberna."
            );
        }
    }

    /// <summary>
    /// Envía el servicio seleccionado al gestor de la taberna al hacer clic.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (datosServicio != null && TavernManager.Instance != null)
        {
            TavernManager.Instance.SeleccionarServicio(datosServicio);
        }
        else
        {
            Debug.LogWarning("Datos del servicio inválidos o gestor de taberna no disponible.");
        }
    }

    /// <summary>
    /// Muestra un mensaje temporal con la información del servicio al pasar el cursor.
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
    /// Oculta el mensaje temporal al apartar el cursor.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OcultarTooltip();
        }
    }
}
