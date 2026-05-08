using UnityEngine;

/// <summary>
/// Asigna rutinas de patrulla o espera a los gatos de la isla.
/// </summary>
public class CatBehavior : MonoBehaviour
{
    public enum Comportamiento
    {
        Quieto,
        Patrullando,
    }

    [Header("Tipo de Movimiento")]
    public Comportamiento comportamientoBase;
    public bool esFlotante = false;

    [Header("Configuración Paseo")]
    public float velocidadCaminar = 3f;
    public float radioDeMerodeo = 3f;
    public float esperaMinima = 1f;
    public float esperaMaxima = 4f;
    public int paseosPorZona = 3;

    [Header("Configuración Flotante")]
    public Transform modelo3D;
    public float velocidadFlote = 2f;
    public float alturaFlote = 0.2f;
    public float elevacionExtra = 1f;

    [Header("Rutas de Patrulla")]
    public Transform[] waypoints;

    [Header("Animaciones Disponibles")]
    public Animator animator;
    public bool tieneAnimWalk = true;
    public bool tieneAnimTalk = false;

    [Header("Interacción")]
    public bool mirarAlJugador = true;

    private float posYOriginalModelo;
    private int puntoActual = 0;
    private int contadorPaseos = 0;
    private bool estaInteractuando = false;
    private Transform jugadorTransform;
    private Vector3 posicionBase;
    private Vector3 destinoActual;
    private bool estaCaminando = false;
    private float temporizadorEspera;

    /// <summary>
    /// Guarda la posición inicial y busca las rutas disponibles.
    /// </summary>
    private void Start()
    {
        if (modelo3D != null)
        {
            posYOriginalModelo = modelo3D.localPosition.y;
        }

        posicionBase = transform.position;

        if (comportamientoBase == Comportamiento.Patrullando)
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                GameObject objetoRuta = GameObject.Find("RutaGeneral");
                if (objetoRuta != null && objetoRuta.transform.childCount > 0)
                {
                    waypoints = new Transform[objetoRuta.transform.childCount];
                    for (int i = 0; i < objetoRuta.transform.childCount; i++)
                    {
                        waypoints[i] = objetoRuta.transform.GetChild(i);
                    }
                }
            }

            if (waypoints != null && waypoints.Length > 0)
            {
                puntoActual = Random.Range(0, waypoints.Length);
            }

            FijarNuevoDestino();
        }
    }

    /// <summary>
    /// Evalúa en cada momento si el gato debe flotar, hablar o caminar.
    /// </summary>
    private void Update()
    {
        GestionarFlote();

        if (estaInteractuando && jugadorTransform != null)
        {
            if (mirarAlJugador)
            {
                GirarHacia(jugadorTransform.position);
            }
        }
        else if (comportamientoBase == Comportamiento.Patrullando)
        {
            GestionarPaseoLibre();
        }

        GestionarAnimaciones();
    }

    /// <summary>
    /// Sube y baja el modelo del gato constantemente para dar la sensación mágica.
    /// </summary>
    private void GestionarFlote()
    {
        if (esFlotante && modelo3D != null)
        {
            float nuevaY =
                posYOriginalModelo
                + elevacionExtra
                + Mathf.Sin(Time.time * velocidadFlote) * alturaFlote;
            modelo3D.localPosition = new Vector3(
                modelo3D.localPosition.x,
                nuevaY,
                modelo3D.localPosition.z
            );
        }
    }

    /// <summary>
    /// Decide hacia dónde moverse y cuánto esperar.
    /// </summary>
    private void GestionarPaseoLibre()
    {
        if (!estaCaminando)
        {
            temporizadorEspera -= Time.deltaTime;
            if (temporizadorEspera <= 0)
            {
                FijarNuevoDestino();
            }
        }
        else
        {
            GirarHacia(destinoActual);
            transform.position = Vector3.MoveTowards(
                transform.position,
                destinoActual,
                velocidadCaminar * Time.deltaTime
            );

            float distancia = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(destinoActual.x, 0, destinoActual.z)
            );

            if (distancia < 0.1f)
            {
                LlegarADestino();
            }
        }
    }

    /// <summary>
    /// Elige una coordenada al azar cercana para que el gato camine hacia ella.
    /// </summary>
    private void FijarNuevoDestino()
    {
        Vector3 centroZona = transform.position;

        if (waypoints != null && waypoints.Length > 0)
        {
            centroZona = waypoints[puntoActual].position;
        }
        else
        {
            centroZona = posicionBase;
        }

        Vector3 offsetAleatorio = new Vector3(
            Random.Range(-radioDeMerodeo, radioDeMerodeo),
            0,
            Random.Range(-radioDeMerodeo, radioDeMerodeo)
        );

        destinoActual = centroZona + offsetAleatorio;
        destinoActual.y = transform.position.y;
        estaCaminando = true;
    }

    /// <summary>
    /// Frena al gato y reinicia el reloj de espera.
    /// </summary>
    private void LlegarADestino()
    {
        estaCaminando = false;
        temporizadorEspera = Random.Range(esperaMinima, esperaMaxima);
        contadorPaseos++;

        if (contadorPaseos >= paseosPorZona && waypoints != null && waypoints.Length > 0)
        {
            contadorPaseos = 0;
            puntoActual++;
            if (puntoActual >= waypoints.Length)
            {
                puntoActual = 0;
            }
        }
    }

    /// <summary>
    /// Hace que el personaje mire gradualmente hacia la posición que le digamos.
    /// </summary>
    private void GirarHacia(Vector3 objetivo)
    {
        Vector3 direccion = (objetivo - transform.position).normalized;
        direccion.y = 0;

        if (direccion != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                Time.deltaTime * 5f
            );
        }
    }

    /// <summary>
    /// Cambia de animación de reposo a caminar o a hablar de forma segura.
    /// </summary>
    private void GestionarAnimaciones()
    {
        if (animator != null)
        {
            if (tieneAnimWalk)
            {
                animator.SetBool("Walk", estaCaminando && !estaInteractuando);
            }

            if (tieneAnimTalk)
            {
                animator.SetBool("Talk", estaInteractuando);
            }
        }
    }

    /// <summary>
    /// Avisa al gato de que el jugador quiere charlar y detiene su ruta.
    /// </summary>
    public void IniciarInteraccion(Transform jugador)
    {
        estaInteractuando = true;
        jugadorTransform = jugador;
    }

    /// <summary>
    /// Desbloquea al gato para que siga con su vida normal tras charlar.
    /// </summary>
    public void FinalizarInteraccion()
    {
        estaInteractuando = false;
        jugadorTransform = null;
        temporizadorEspera = Random.Range(1f, 3f);
    }
}
