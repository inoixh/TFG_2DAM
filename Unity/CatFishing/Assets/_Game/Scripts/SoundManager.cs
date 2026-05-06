using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la reproducción de música de fondo, ambiente marino y efectos de sonido generales.
/// Implementa el patrón Singleton para persistir a lo largo de las distintas escenas del juego sin cortes.
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
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        ambienceSource = gameObject.AddComponent<AudioSource>();
        effectsSource = gameObject.AddComponent<AudioSource>();
        sfxLoopSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        ambienceSource.loop = true;
        sfxLoopSource.loop = true;
    }

    /// <summary>
    /// Detecta la escena actual al inicializarse para reproducir la música correcta de forma automática.
    /// Útil si el desarrollador arranca el juego directamente desde la escena de la isla para hacer pruebas.
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
    /// Detiene el sonido ambiental del mar y reproduce la pista relajante de los menús.
    /// </summary>
    public void ReproducirMusicaMenu()
    {
        if (ambienceSource != null)
            ambienceSource.Stop();

        if (musicSource != null && musicaMenu != null)
        {
            musicSource.clip = musicaMenu;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Activa el sonido ambiental del mar y reproduce la pista de exploración de la isla.
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
    }

    public void ReproducirPaso()
    {
        AudioClip clipAUsar = pasoAlternador ? pasoA : pasoB;
        pasoAlternador = !pasoAlternador;

        if (clipAUsar != null)
        {
            effectsSource.pitch = Random.Range(0.95f, 1.05f);
            effectsSource.PlayOneShot(clipAUsar, volumenPasos);
            effectsSource.pitch = 1.0f;
        }
    }

    public void ReproducirSonidoPersonalizado(AudioClip clip)
    {
        if (clip != null)
            effectsSource.PlayOneShot(clip, volumenEfectos);
    }

    // ==========================================
    // MÉTODOS DE PESCA
    // ==========================================
    public void SFX_Lanzar()
    {
        if (sonidoLanzar != null)
            effectsSource.PlayOneShot(sonidoLanzar, volumenEfectos);
    }

    public void SFX_EmpezarForcejeo()
    {
        if (sonidoPicarLoop != null)
        {
            sfxLoopSource.clip = sonidoPicarLoop;
            sfxLoopSource.Play();
        }
    }

    public void SFX_PararForcejeo()
    {
        sfxLoopSource.Stop();
    }

    public void SFX_Ganar()
    {
        SFX_PararForcejeo();
        if (sonidoCaptura != null)
            effectsSource.PlayOneShot(sonidoCaptura, volumenEfectos);
    }

    public void SFX_Perder()
    {
        SFX_PararForcejeo();
        if (sonidoPerder != null)
            effectsSource.PlayOneShot(sonidoPerder, volumenEfectos);
    }

    // ==========================================
    // MÉTODOS DE UI Y ECONOMÍA
    // ==========================================
    public void ReproducirClick()
    {
        if (sonidoClickBoton != null)
            effectsSource.PlayOneShot(sonidoClickBoton, volumenEfectos);
    }

    public void ReproducirComprar()
    {
        if (sonidoComprar != null)
            effectsSource.PlayOneShot(sonidoComprar, volumenEfectos);
    }

    public void ReproducirVender()
    {
        if (sonidoVender != null)
            effectsSource.PlayOneShot(sonidoVender, volumenEfectos);
    }

    public void ReproducirSubirNivel()
    {
        if (sonidoSubirNivel != null)
            effectsSource.PlayOneShot(sonidoSubirNivel, volumenEfectos);
    }

    public void ReproducirConsumirTaberna()
    {
        if (sonidoConsumirTaberna != null)
            effectsSource.PlayOneShot(sonidoConsumirTaberna, volumenEfectos);
    }

    public void ReproducirRezarIglesia()
    {
        if (sonidoRezarIglesia != null)
            effectsSource.PlayOneShot(sonidoRezarIglesia, volumenEfectos);
    }

    // ==========================================
    // MÉTODOS DE GATOS
    // ==========================================
    public void ReproducirGatoFeliz()
    {
        if (sonidoGatoFeliz != null)
            effectsSource.PlayOneShot(sonidoGatoFeliz, volumenEfectos);
    }

    public void ReproducirGatoEnfadado()
    {
        if (sonidoGatoEnfadado != null)
            effectsSource.PlayOneShot(sonidoGatoEnfadado, volumenEfectos);
    }
}
