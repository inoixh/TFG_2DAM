using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Representa un objeto a la venta dentro de la interfaz del mercado.
/// Gestiona la visualización temporal de tooltips al pasar el ratón.
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
    /// Asigna la información del objeto a esta casilla y activa visualmente su icono.
    /// </summary>
    public void ConfigurarSlot(ItemData item)
    {
        datosItem = item;

        if (iconoItem != null && item != null)
        {
            iconoItem.sprite = item.icono;
            iconoItem.color = Color.white;
        }
    }

    /// <summary>
    /// Notifica al gestor del mercado que el jugador ha seleccionado este producto.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (datosItem != null && MarketManager.Instance != null)
        {
            MarketManager.Instance.SeleccionarItemTienda(datosItem);
        }
    }

    /// <summary>
    /// Extrae la información del objeto y solicita al UIManager que la muestre temporalmente.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (datosItem != null && UIManager.Instance != null)
        {
            string textoInfo =
                $"{datosItem.nombreDisplay} \n<size=70%>{datosItem.rareza.NombreFormateado()}</size>";
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
