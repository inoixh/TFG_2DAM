using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla la reproducción de toda la música, ambiente y efectos de sonido del juego.
/// [Relaciones: Ninguna]
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Mezclador de Volumen")]
    [Range(0f, 1f)]
    public float volumenMusica = 0.3f;

    [Range(0f, 1f)]
    public float volumenMar = 0.3f;

    [Range(0f, 1f)]
    public float volumenPasos = 0.5f;

    [Range(0f, 1f)]
    public float volumenEfectos = 1.0f;

    [Range(0f, 1f)]
    public float volumenCarrete = 0.6f;

    [Header("Clips de Audio - Ambiente")]
    public AudioClip musicaMenu;
    public AudioClip musicaJuego;
    public AudioClip sonidoMar;

    [Header("Clips de Audio - Pasos")]
    public AudioClip pasoA;
    public AudioClip pasoB;

    [Header("Clips de Audio - Pesca")]
    public AudioClip sonidoLanzar;
    public AudioClip sonidoPicarLoop;
    public AudioClip sonidoCaptura;
    public AudioClip sonidoPerder;

    [Header("Clips de Audio - Interfaz y Economía")]
    public AudioClip sonidoClickBoton;
    public AudioClip sonidoComprar;
    public AudioClip sonidoVender;
    public AudioClip sonidoSubirNivel;

    [Header("Clips de Audio - Gatos")]
    public AudioClip sonidoGatoFeliz;
    public AudioClip sonidoGatoEnfadado;

    [Header("Clips de Audio - Edificios")]
    public AudioClip sonidoConsumirTaberna;
    public AudioClip sonidoRezarIglesia;

    private AudioSource musicSource;
    private AudioSource ambienceSource;
    private AudioSource effectsSource;
    private AudioSource sfxLoopSource;
    private bool pasoAlternador = false;

    /// <summary>
    /// Configura el Singleton persistente entre escenas y crea las fuentes de audio base.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (Instance == this)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            ambienceSource = gameObject.AddComponent<AudioSource>();
            effectsSource = gameObject.AddComponent<AudioSource>();
            sfxLoopSource = gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
            ambienceSource.loop = true;
            sfxLoopSource.loop = true;
        }
    }

    /// <summary>
    /// Comprueba la escena actual para reproducir la melodía correspondiente al inicio.
    /// </summary>
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            ReproducirMusicaJuego();
        }
        else
        {
            ReproducirMusicaMenu();
        }
    }

    /// <summary>
    /// Mantiene sincronizados los volúmenes de las fuentes de audio con las variables públicas.
    /// </summary>
    void Update()
    {
        if (musicSource != null)
            musicSource.volume = volumenMusica;
        if (ambienceSource != null)
            ambienceSource.volume = volumenMar;
        if (sfxLoopSource != null)
            sfxLoopSource.volume = volumenCarrete;
    }

    /// <summary>
    /// Pone la música del menú principal y detiene los sonidos ambientales.
    /// </summary>
    public void ReproducirMusicaMenu()
    {
        if (ambienceSource != null)
        {
            ambienceSource.Stop();
        }

        if (musicSource != null && musicaMenu != null)
        {
            musicSource.clip = musicaMenu;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Faltan recursos para reproducir la canción de menú.");
        }
    }

    /// <summary>
    /// Pone la música de partida y activa los sonidos ambientales de mar continuo.
    /// </summary>
    public void ReproducirMusicaJuego()
    {
        if (ambienceSource != null && sonidoMar != null)
        {
            ambienceSource.clip = sonidoMar;
            ambienceSource.Play();
        }

        if (musicSource != null && musicaJuego != null)
        {
            musicSource.clip = musicaJuego;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Faltan recursos para reproducir el ambiente de juego.");
        }
    }

    /// <summary>
    /// Reproduce alternativamente dos sonidos de paso con una ligera alteración de tono.
    /// </summary>
    public void ReproducirPaso()
    {
        AudioClip clipAUsar;

        clipAUsar = pasoAlternador ? pasoA : pasoB;
        pasoAlternador = !pasoAlternador;

        if (clipAUsar != null && effectsSource != null)
        {
            effectsSource.pitch = Random.Range(0.95f, 1.05f);
            effectsSource.PlayOneShot(clipAUsar, volumenPasos);
            effectsSource.pitch = 1.0f;
        }
        else
        {
            Debug.LogWarning("No hay efectos de paso asignados.");
        }
    }

    /// <summary>
    /// Lanza un clip único provisto por parámetro mediante la fuente de efectos.
    /// </summary>
    public void ReproducirSonidoPersonalizado(AudioClip clip)
    {
        if (clip != null && effectsSource != null)
        {
            effectsSource.PlayOneShot(clip, volumenEfectos);
        }
        else
        {
            Debug.LogWarning("El clip entregado está vacío.");
        }
    }

    /// <summary>
    /// Reproduce el efecto sonoro del lanzamiento del anzuelo.
    /// </summary>
    public void SFX_Lanzar()
    {
        if (sonidoLanzar != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoLanzar, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de lanzar la caña.");
    }

    /// <summary>
    /// Inicia el sonido en bucle del carrete durante la fase de forcejeo.
    /// </summary>
    public void SFX_EmpezarForcejeo()
    {
        if (sonidoPicarLoop != null && sfxLoopSource != null)
        {
            sfxLoopSource.clip = sonidoPicarLoop;
            sfxLoopSource.Play();
        }
        else
            Debug.LogWarning("Falta el audio de picar.");
    }

    /// <summary>
    /// Detiene el bucle de sonido de forcejeo con el pez.
    /// </summary>
    public void SFX_PararForcejeo()
    {
        if (sfxLoopSource != null)
            sfxLoopSource.Stop();
    }

    /// <summary>
    /// Detiene el forcejeo y reproduce un sonido de victoria.
    /// </summary>
    public void SFX_Ganar()
    {
        SFX_PararForcejeo();
        if (sonidoCaptura != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoCaptura, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de ganar.");
    }

    /// <summary>
    /// Detiene el forcejeo y reproduce un sonido de derrota o sedal roto.
    /// </summary>
    public void SFX_Perder()
    {
        SFX_PararForcejeo();
        if (sonidoPerder != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoPerder, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de perder.");
    }

    /// <summary>
    /// Reproduce el efecto genérico al presionar cualquier botón de interfaz.
    /// </summary>
    public void ReproducirClick()
    {
        if (sonidoClickBoton != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoClickBoton, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de click.");
    }

    /// <summary>
    /// Emite el sonido asignado al gasto de dinero o adquisición de bienes.
    /// </summary>
    public void ReproducirComprar()
    {
        if (sonidoComprar != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoComprar, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de comprar.");
    }

    /// <summary>
    /// Emite el sonido asociado a la ganancia de monedas.
    /// </summary>
    public void ReproducirVender()
    {
        if (sonidoVender != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoVender, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de vender.");
    }

    /// <summary>
    /// Reproduce un efecto llamativo tras aumentar el nivel de jugador.
    /// </summary>
    public void ReproducirSubirNivel()
    {
        if (sonidoSubirNivel != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoSubirNivel, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de subir nivel.");
    }

    /// <summary>
    /// Dispara el efecto del consumo de una bebida en la cantina.
    /// </summary>
    public void ReproducirConsumirTaberna()
    {
        if (sonidoConsumirTaberna != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoConsumirTaberna, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de taberna.");
    }

    /// <summary>
    /// Reproduce la campana o efecto sacro vinculado a la recompensa diaria.
    /// </summary>
    public void ReproducirRezarIglesia()
    {
        if (sonidoRezarIglesia != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoRezarIglesia, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de rezar.");
    }

    /// <summary>
    /// Emite el ronroneo o maullido feliz de un felino satisfecho.
    /// </summary>
    public void ReproducirGatoFeliz()
    {
        if (sonidoGatoFeliz != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoGatoFeliz, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de gato feliz.");
    }

    /// <summary>
    /// Emite el bufido o gruñido de un gato rechazando su alimento.
    /// </summary>
    public void ReproducirGatoEnfadado()
    {
        if (sonidoGatoEnfadado != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoGatoEnfadado, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de gato enfadado.");
    }
}
