using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controla visualmente un producto que está a la venta en el escaparate del mercado.
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
    /// Recibe el catálogo del objeto y activa su imagen principal en la tienda.
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
    /// Avisa a la tienda general de que el jugador ha hecho clic en este producto.
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
    /// Crea un texto flotante temporal con el nombre del producto al pasar el ratón.
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
    /// Destruye el texto flotante cuando el ratón se aleja de la caja.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OcultarTooltip();
        }
    }
}
