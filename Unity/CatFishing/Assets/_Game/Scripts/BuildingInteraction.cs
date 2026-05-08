using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Comprueba si el jugador está cerca de un edificio para permitirle interactuar.
/// </summary>
public class BuildingInteraction : MonoBehaviour
{
    public enum TipoEdificio
    {
        Mercado,
        Taberna,
        Iglesia,
    }

    [Header("Configuración del Edificio")]
    public TipoEdificio tipoDeEdificio;

    private bool jugadorCerca = false;

    /// <summary>
    /// Abre el menú correspondiente si el jugador pulsa la tecla estando cerca y ningún otro menú está abierto.
    /// </summary>
    private void Update()
    {
        if (jugadorCerca && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (UIManager.Instance != null && UIManager.Instance.ConsumirInteraccion())
            {
                UIManager.Instance.OcultarInteraccion();

                switch (tipoDeEdificio)
                {
                    case TipoEdificio.Mercado:
                        if (
                            MarketManager.Instance != null
                            && !MarketManager.Instance.mercadoAbierto
                        )
                            MarketManager.Instance.AbrirMercado();
                        break;
                    case TipoEdificio.Taberna:
                        if (
                            TavernManager.Instance != null
                            && !TavernManager.Instance.tabernaAbierta
                        )
                            TavernManager.Instance.AbrirTaberna();
                        break;
                    case TipoEdificio.Iglesia:
                        if (
                            ChurchManager.Instance != null
                            && !ChurchManager.Instance.iglesiaAbierta
                        )
                            ChurchManager.Instance.AbrirIglesia();
                        break;
                }
            }
            else
            {
                Debug.LogWarning("Interacción ignorada: Ya hay un panel abierto.");
            }
        }
    }

    /// <summary>
    /// Muestra el texto de ayuda cuando el jugador entra en la zona del edificio.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.MostrarInteraccion(
                    "Pulsa [ESPACIO] para entrar al " + tipoDeEdificio.ToString()
                );
            }
        }
    }

    /// <summary>
    /// Oculta el texto de ayuda cuando el jugador se aleja del edificio.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OcultarInteraccion();
            }
        }
    }
}
