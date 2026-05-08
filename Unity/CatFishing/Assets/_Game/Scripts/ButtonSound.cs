using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Añade de forma automática un sonido general a cualquier botón pulsado.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    /// <summary>
    /// Configura el botón para que emita el sonido al hacer clic.
    /// </summary>
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(ReproducirSonidoClick);
        }
        else
        {
            Debug.LogWarning("No se encontró el componente Button en el objeto.");
        }
    }

    /// <summary>
    /// Pide al gestor de audio que reproduzca el sonido de la interfaz.
    /// </summary>
    private void ReproducirSonidoClick()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ReproducirClick();
        }
        else
        {
            Debug.LogWarning("SoundManager no existe para reproducir el clic.");
        }
    }
}
