using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Define el comportamiento de un objeto individual mostrado en la interfaz del mercado.
/// [Relaciones: ItemData, MarketManager, UIManager]
/// </summary>
public class MarketSlot
    : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler
{
    [Header("Componentes Visuales")]
    public Image iconoItem;

    private ItemData datosItem;

    /// <summary>
    /// Carga y muestra los datos del objeto disponible para la venta.
    /// </summary>
    public void ConfigurarSlot(ItemData item)
    {
        datosItem = item;

        if (iconoItem != null && item != null)
        {
            iconoItem.sprite = item.icono;
            iconoItem.color = Color.white;
        }
        else
        {
            Debug.LogWarning(
                "Falta la imagen base o el objeto no tiene icono para mostrar en el mercado."
            );
        }
    }

    /// <summary>
    /// Envía los datos del objeto al gestor del mercado al ser seleccionado.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (datosItem != null && MarketManager.Instance != null)
        {
            MarketManager.Instance.SeleccionarItemTienda(datosItem);
        }
        else
        {
            Debug.LogWarning("Se hizo clic en un objeto del mercado pero no hay datos válidos.");
        }
    }

    /// <summary>
    /// Muestra una ventana emergente con el nombre y rareza del objeto al pasar el ratón.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        string textoInfo;

        if (datosItem != null && UIManager.Instance != null)
        {
            textoInfo =
                $"{datosItem.nombreDisplay} \n<size=70%>{datosItem.rareza.NombreFormateado()}</size>";
            UIManager.Instance.MostrarTooltipTemporal(textoInfo, 1.5f);
        }
    }

    /// <summary>
    /// Oculta la ventana emergente de información.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OcultarTooltip();
        }
    }
}
