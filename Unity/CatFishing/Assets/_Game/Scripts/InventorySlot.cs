using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controla la imagen y el botón de cada hueco del inventario en la pantalla.
/// </summary>
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración")]
    public Image iconoImagen;
    public Image bordeSeleccion;
    public Button botonSlot;

    private int miIndice;
    private ItemData itemActual;

    /// <summary>
    /// Oculta el icono y el borde al iniciar la partida si la casilla está vacía.
    /// </summary>
    void Start()
    {
        if (iconoImagen != null && iconoImagen.sprite == null)
        {
            iconoImagen.enabled = false;
        }

        if (bordeSeleccion != null)
        {
            bordeSeleccion.enabled = false;
        }
    }

    /// <summary>
    /// Guarda el número de esta casilla y enlaza el clic del botón con el sistema general del inventario.
    /// </summary>
    public void ConfigurarSlot(int indice)
    {
        miIndice = indice;

        if (botonSlot != null)
        {
            botonSlot.onClick.RemoveAllListeners();
            botonSlot.onClick.AddListener(() => InventorySystem.Instance.ClickEnSlot(miIndice));
        }
        else
        {
            Debug.LogWarning("Falta el botón en la casilla del inventario.");
        }
    }

    /// <summary>
    /// Cambia la imagen de la casilla según el objeto que reciba o la oculta si no recibe nada.
    /// </summary>
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

    /// <summary>
    /// Enciende o apaga el marco visual que indica si el jugador tiene este objeto en la mano.
    /// </summary>
    public void Seleccionar(bool seleccionado)
    {
        if (bordeSeleccion != null)
        {
            bordeSeleccion.enabled = seleccionado;
        }
        else
        {
            Debug.LogWarning("Falta la imagen del borde de selección en la casilla.");
        }
    }

    /// <summary>
    /// Muestra un cartel de información con el nombre y rareza del objeto cuando el ratón pasa por encima.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        string textoInfo;

        if (itemActual != null && UIManager.Instance != null)
        {
            textoInfo =
                $"{itemActual.nombreDisplay} \n<size=70%>{itemActual.rareza.NombreFormateado()}</size>";
            UIManager.Instance.MostrarTooltipTemporal(textoInfo, 1.5f);
        }
    }

    /// <summary>
    /// Oculta el cartel de información cuando el ratón sale de la casilla.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OcultarTooltip();
        }
    }
}
