using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el movimiento físico del jugador, aplicando suavizado y gestionando el sonido de los pasos.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Velocidades")]
    public float velocidadCaminar = 6f;
    public float velocidadCorrer = 10f;

    [Header("Suavizado de Movimiento")]
    [Range(0.01f, 0.5f)]
    public float tiempoAceleracion = 0.1f;

    [Header("Audio Pasos")]
    public float frecuenciaPasosCaminar = 0.5f;
    public float frecuenciaPasosCorrer = 0.3f;

    [HideInInspector]
    public bool movimientoBloqueado = false;

    private CharacterController controller;
    private float cronometroPasos = 0f;
    private Vector2 movimientoActual;
    private Vector2 velocidadMovimientoSuavizado;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (movimientoBloqueado)
            return;

        ProcesarMovimiento();
        ProcesarAudioPasos();
    }

    /// <summary>
    /// Calcula el vector de dirección basado en el teclado y mueve al CharacterController.
    /// </summary>
    private void ProcesarMovimiento()
    {
        Vector2 movimientoObjetivo = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed)
                movimientoObjetivo.y += 1;
            if (Keyboard.current.sKey.isPressed)
                movimientoObjetivo.y -= 1;
            if (Keyboard.current.aKey.isPressed)
                movimientoObjetivo.x -= 1;
            if (Keyboard.current.dKey.isPressed)
                movimientoObjetivo.x += 1;
        }

        if (movimientoObjetivo.magnitude > 1f)
        {
            movimientoObjetivo.Normalize();
        }

        movimientoActual = Vector2.SmoothDamp(
            movimientoActual,
            movimientoObjetivo,
            ref velocidadMovimientoSuavizado,
            tiempoAceleracion
        );

        bool estaCorriendo =
            Keyboard.current.leftShiftKey.isPressed && movimientoActual.magnitude > 0.1f;
        float velocidadActual = estaCorriendo ? velocidadCorrer : velocidadCaminar;

        Vector3 mover =
            transform.right * movimientoActual.x + transform.forward * movimientoActual.y;
        mover.y = -9.8f;

        controller.Move(mover * velocidadActual * Time.deltaTime);
    }

    /// <summary>
    /// Gestiona los intervalos de tiempo para reproducir efectos de sonido al caminar o correr.
    /// </summary>
    private void ProcesarAudioPasos()
    {
        bool estaCorriendo = Keyboard.current.leftShiftKey.isPressed;

        if (controller.velocity.magnitude > 0.1f && movimientoActual.magnitude > 0.1f)
        {
            float tiempoEntrePasos = estaCorriendo ? frecuenciaPasosCorrer : frecuenciaPasosCaminar;
            cronometroPasos += Time.deltaTime;

            if (cronometroPasos >= tiempoEntrePasos)
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.ReproducirPaso();
                cronometroPasos = 0f;
            }
        }
        else
        {
            cronometroPasos = 0f;
        }
    }
}
