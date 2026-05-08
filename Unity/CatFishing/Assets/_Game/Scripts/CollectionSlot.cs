using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Muestra si un elemento de la colección está desbloqueado y reacciona al clic.
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
    /// Aplica los datos de un gato, mostrando una silueta si no está descubierto.
    /// </summary>
    public void ConfigurarGato(CatData gato, bool desbloqueado, Sprite iconoIncognita)
    {
        datosGato = gato;
        esGato = true;
        estaDesbloqueado = desbloqueado;

        if (iconoItem != null)
        {
            iconoItem.sprite = desbloqueado ? gato.icono : iconoIncognita;
        }
        else
        {
            Debug.LogWarning("No existe el componente Image para el icono en CollectionSlot.");
        }
    }

    /// <summary>
    /// Aplica los datos de un pez, mostrando una silueta si no está descubierto.
    /// </summary>
    public void ConfigurarPez(ItemData pez, bool desbloqueado, Sprite iconoIncognita)
    {
        datosPez = pez;
        esGato = false;
        estaDesbloqueado = desbloqueado;

        if (iconoItem != null)
        {
            iconoItem.sprite = desbloqueado ? pez.icono : iconoIncognita;
        }
        else
        {
            Debug.LogWarning("No existe el componente Image para el icono en CollectionSlot.");
        }
    }

    /// <summary>
    /// Informa al panel principal cuando el jugador pulsa sobre esta casilla.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (estaDesbloqueado)
        {
            if (esGato && datosGato != null && CollectionManager.Instance != null)
            {
                CollectionManager.Instance.SeleccionarGato(datosGato);
            }
            else if (!esGato && datosPez != null && CollectionManager.Instance != null)
            {
                CollectionManager.Instance.SeleccionarPez(datosPez);
            }
            else
            {
                Debug.LogWarning(
                    "Faltan datos o el CollectionManager no está disponible para procesar el clic."
                );
            }
        }
        else
        {
            Debug.LogWarning("El jugador ha clicado un objeto no desbloqueado, se ignora.");
        }
    }
}
