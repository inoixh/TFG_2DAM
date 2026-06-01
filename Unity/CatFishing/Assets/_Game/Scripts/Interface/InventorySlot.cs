using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controla la lógica de la interfaz para una casilla individual del inventario del jugador.
/// [Relaciones: ItemData, InventorySystem, UIManager]
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
    /// Inicializa los estados visuales predeterminados de la casilla.
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
    /// Asigna el índice correspondiente y el evento de clic a la casilla.
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
    /// Muestra u oculta el objeto del inventario en función de los datos recibidos.
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
    /// Activa o desactiva el marco de selección visual de la casilla.
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
    /// Muestra la información detallada del objeto al pasar el cursor por encima.
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
    /// Oculta la información detallada del objeto al retirar el cursor.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OcultarTooltip();
        }
    }
}
