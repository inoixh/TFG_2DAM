using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controla la lógica gráfica individual de cada casilla (slot) del inventario.
/// Gestiona la visualización temporal de tooltips al pasar el ratón.
/// </summary>
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración")]
    public Image iconoImagen;
    public Image bordeSeleccion;
    public Button botonSlot;

    private int miIndice;
    private ItemData itemActual;

    void Start()
    {
        if (iconoImagen != null && iconoImagen.sprite == null)
            iconoImagen.enabled = false;
        if (bordeSeleccion != null)
            bordeSeleccion.enabled = false;
    }

    public void ConfigurarSlot(int indice)
    {
        miIndice = indice;
        if (botonSlot != null)
        {
            botonSlot.onClick.RemoveAllListeners();
            botonSlot.onClick.AddListener(() => InventorySystem.Instance.ClickEnSlot(miIndice));
        }
    }

    public void ActualizarSlot(ItemData item)
    {
        itemActual = item;

        if (item != null)
        {
            iconoImagen.sprite = item.icono;
            iconoImagen.enabled = true;
            iconoImagen.color = Color.white;
        }
        else
        {
            iconoImagen.sprite = null;
            iconoImagen.enabled = false;
            iconoImagen.color = new Color(1, 1, 1, 0);
        }
    }

    public void Seleccionar(bool seleccionado)
    {
        if (bordeSeleccion != null)
            bordeSeleccion.enabled = seleccionado;
    }

    /// <summary>
    /// Extrae la información del objeto y solicita al UIManager que la muestre durante 1.5 segundos.
    /// Utiliza el método de extensión NombreFormateado para aplicar los colores y emojis.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemActual != null && UIManager.Instance != null)
        {
            string textoInfo =
                $"{itemActual.nombreDisplay} \n<size=70%>{itemActual.rareza.NombreFormateado()}</size>";
            UIManager.Instance.MostrarTooltipTemporal(textoInfo, 1.5f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OcultarTooltip();
    }
}
