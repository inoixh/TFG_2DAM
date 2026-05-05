using UnityEngine;

/// <summary>
/// Detecta si el jugador entra o sale de una masa de agua y notifica al controlador de pesca.
/// </summary>
public class ZonaPesca : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PescaController caña = other.GetComponentInChildren<PescaController>(true);
            if (caña != null)
            {
                caña.EstablecerZonaDePesca(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PescaController caña = other.GetComponentInChildren<PescaController>(true);
            if (caña != null)
            {
                caña.EstablecerZonaDePesca(false);
            }
        }
    }
}
