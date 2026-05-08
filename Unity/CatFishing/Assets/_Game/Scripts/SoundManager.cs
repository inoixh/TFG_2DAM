using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Reproduce toda la música y los efectos de sonido sin cortarse al cambiar de pantalla.
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
    /// Genera los altavoces virtuales y se protege para no morir al cambiar de menú.
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
    /// Detecta en qué mapa estás al nacer para poner la banda sonora correcta.
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
    /// Mantiene los altavoces sincronizados con la rueda de mezclas maestra.
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
    /// Apaga los pájaros marinos y te pone el tema suave de introducción.
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
    /// Combina la pista de la isla y el ruido del mar sonando a la vez.
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
    /// Altera qué zapato toca el suelo para que no suene a robot de forma repetitiva.
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
    /// Dispara por el altavoz cualquier fichero sonoro ajeno que le mandemos.
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
    /// Hace sonar el efecto de tirar el anzuelo al agua.
    /// </summary>
    public void SFX_Lanzar()
    {
        if (sonidoLanzar != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoLanzar, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de lanzar la caña.");
    }

    /// <summary>
    /// Enciende la cinta en bucle de la máquina del hilo girando.
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
    /// Calla de golpe el sonido continuado de tirar de la presa.
    /// </summary>
    public void SFX_PararForcejeo()
    {
        if (sfxLoopSource != null)
            sfxLoopSource.Stop();
    }

    /// <summary>
    /// Felicita el final feliz con un audio refrescante de pez saliendo del agua.
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
    /// Marca la derrota ahogando la línea con el sonido sombrío de rotura.
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
    /// Suena a ratón físico pinchando cualquier área de los cuadros interactivos.
    /// </summary>
    public void ReproducirClick()
    {
        if (sonidoClickBoton != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoClickBoton, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de click.");
    }

    /// <summary>
    /// Recrea el ruido de soltar monedas encima del tablón de mercaderías.
    /// </summary>
    public void ReproducirComprar()
    {
        if (sonidoComprar != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoComprar, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de comprar.");
    }

    /// <summary>
    /// Tintinea al adquirir beneficios financieros ganando dinero fresco.
    /// </summary>
    public void ReproducirVender()
    {
        if (sonidoVender != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoVender, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de vender.");
    }

    /// <summary>
    /// Dispara campanadas que reafirman la consecución de una nueva experiencia redonda.
    /// </summary>
    public void ReproducirSubirNivel()
    {
        if (sonidoSubirNivel != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoSubirNivel, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de subir nivel.");
    }

    /// <summary>
    /// Emula un líquido embotellado chocando para la ingesta de las bebidas potenciadoras.
    /// </summary>
    public void ReproducirConsumirTaberna()
    {
        if (sonidoConsumirTaberna != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoConsumirTaberna, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de taberna.");
    }

    /// <summary>
    /// Despliega el sonido eclesiástico cuando interactúas con la religión en la isla.
    /// </summary>
    public void ReproducirRezarIglesia()
    {
        if (sonidoRezarIglesia != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoRezarIglesia, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de rezar.");
    }

    /// <summary>
    /// Provoca una respuesta gatuna amistosa para recompensar las buenas obras del jugador.
    /// </summary>
    public void ReproducirGatoFeliz()
    {
        if (sonidoGatoFeliz != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoGatoFeliz, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de gato feliz.");
    }

    /// <summary>
    /// Enseña los dientes acústicamente como sanción frente al pescado equivocado.
    /// </summary>
    public void ReproducirGatoEnfadado()
    {
        if (sonidoGatoEnfadado != null && effectsSource != null)
            effectsSource.PlayOneShot(sonidoGatoEnfadado, volumenEfectos);
        else
            Debug.LogWarning("Falta el audio de gato enfadado.");
    }
}
