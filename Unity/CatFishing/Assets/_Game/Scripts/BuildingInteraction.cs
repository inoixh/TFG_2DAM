using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Detecta la presencia del jugador cerca de cualquier edificio interactuable
/// y delega la apertura de la interfaz al gestor correspondiente asegurando no superponerse.
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
    /// Escucha la interacción pero pide permiso al UIManager para evitar abrir varios menús a la vez.
    /// </summary>
    private void Update()
    {
        if (jugadorCerca && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (UIManager.Instance != null && !UIManager.Instance.ConsumirInteraccion())
                return;

            UIManager.Instance.OcultarInteraccion();

            switch (tipoDeEdificio)
            {
                case TipoEdificio.Mercado:
                    if (MarketManager.Instance != null && !MarketManager.Instance.mercadoAbierto)
                        MarketManager.Instance.AbrirMercado();
                    break;
                case TipoEdificio.Taberna:
                    if (TavernManager.Instance != null && !TavernManager.Instance.tabernaAbierta)
                        TavernManager.Instance.AbrirTaberna();
                    break;
                case TipoEdificio.Iglesia:
                    if (ChurchManager.Instance != null && !ChurchManager.Instance.iglesiaAbierta)
                        ChurchManager.Instance.AbrirIglesia();
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            if (UIManager.Instance != null)
                UIManager.Instance.MostrarInteraccion(
                    "Pulsa [ESPACIO] para entrar al " + tipoDeEdificio.ToString()
                );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (UIManager.Instance != null)
                UIManager.Instance.OcultarInteraccion();
        }
    }
}
