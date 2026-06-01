using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
/// <summary>
/// Procesa la entrada del teclado para el desplazamiento físico del jugador en el entorno.
/// [Relaciones: SoundManager]
/// </summary>
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

    /// <summary>
    /// Inicializa la referencia interna requerida del CharacterController físico.
    /// </summary>
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    /// <summary>
    /// Comprueba la restricción de bloqueo y, si está permitido, calcula los pasos por frame.
    /// </summary>
    void Update()
    {
        if (!movimientoBloqueado)
        {
            ProcesarMovimiento();
            ProcesarAudioPasos();
        }
    }

    /// <summary>
    /// Interpreta el vector de entrada y suaviza las fuerzas necesarias para mover la cápsula física.
    /// </summary>
    private void ProcesarMovimiento()
    {
        Vector2 movimientoObjetivo = Vector2.zero;
        bool estaCorriendo;
        float velocidadActual;
        Vector3 mover;

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

        estaCorriendo =
            Keyboard.current != null
            && Keyboard.current.leftShiftKey.isPressed
            && movimientoActual.magnitude > 0.1f;
        velocidadActual = estaCorriendo ? velocidadCorrer : velocidadCaminar;

        mover = transform.right * movimientoActual.x + transform.forward * movimientoActual.y;
        mover.y = -9.8f;

        if (controller != null)
        {
            controller.Move(mover * velocidadActual * Time.deltaTime);
        }
        else
        {
            Debug.LogWarning("Falta el CharacterController para mover al jugador.");
        }
    }

    /// <summary>
    /// Mide la distancia recorrida y acciona los eventos sonoros pertinentes al contactar con el suelo.
    /// </summary>
    private void ProcesarAudioPasos()
    {
        bool estaCorriendo;
        float tiempoEntrePasos;

        estaCorriendo = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        if (
            controller != null
            && controller.velocity.magnitude > 0.1f
            && movimientoActual.magnitude > 0.1f
        )
        {
            tiempoEntrePasos = estaCorriendo ? frecuenciaPasosCorrer : frecuenciaPasosCaminar;
            cronometroPasos += Time.deltaTime;

            if (cronometroPasos >= tiempoEntrePasos)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.ReproducirPaso();
                }
                cronometroPasos = 0f;
            }
        }
        else
        {
            cronometroPasos = 0f;
        }
    }
}
