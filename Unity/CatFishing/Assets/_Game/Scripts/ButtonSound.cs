using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Se adjunta a cualquier botón de la interfaz para reproducir automáticamente
/// el sonido de click general sin necesidad de programarlo uno por uno.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    /// <summary>
    /// Añade el evento de sonido al botón al iniciar.
    /// </summary>
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(ReproducirSonidoClick);
        }
    }

    /// <summary>
    /// Llama al SoundManager para emitir el efecto.
    /// </summary>
    private void ReproducirSonidoClick()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ReproducirClick();
        }
    }
}
