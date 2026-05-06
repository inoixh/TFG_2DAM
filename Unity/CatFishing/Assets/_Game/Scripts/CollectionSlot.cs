using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controla la lógica visual de una casilla en la colección, adaptándose a si contiene un pez o un gato.
/// Ignora las interacciones si el elemento no ha sido descubierto.
/// </summary>
public class CollectionSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Componentes Visuales")]
    public Image iconoItem;

    private CatData datosGato;
    private ItemData datosPez;
    private bool esGato;
    private bool estaDesbloqueado;

    /// <summary>
    /// Configura la casilla en modo Gato, mostrando su icono real si está descubierto o uno bloqueado en caso contrario.
    /// </summary>
    public void ConfigurarGato(CatData gato, bool desbloqueado, Sprite iconoIncognita)
    {
        datosGato = gato;
        esGato = true;
        estaDesbloqueado = desbloqueado;
        if (iconoItem != null)
            iconoItem.sprite = desbloqueado ? gato.icono : iconoIncognita;
    }

    /// <summary>
    /// Configura la casilla en modo Pez, mostrando su icono real si está descubierto o uno bloqueado en caso contrario.
    /// </summary>
    public void ConfigurarPez(ItemData pez, bool desbloqueado, Sprite iconoIncognita)
    {
        datosPez = pez;
        esGato = false;
        estaDesbloqueado = desbloqueado;
        if (iconoItem != null)
            iconoItem.sprite = desbloqueado ? pez.icono : iconoIncognita;
    }

    /// <summary>
    /// Envía la información al panel de Preview correspondiente si el elemento es público.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!estaDesbloqueado)
            return;

        if (esGato && datosGato != null && CollectionManager.Instance != null)
            CollectionManager.Instance.SeleccionarGato(datosGato);
        else if (!esGato && datosPez != null && CollectionManager.Instance != null)
            CollectionManager.Instance.SeleccionarPez(datosPez);
    }
}
