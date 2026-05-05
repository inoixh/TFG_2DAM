using UnityEngine;

/// <summary>
/// Gestiona la reproducción de música de fondo, ambiente marino y efectos de sonido generales.
/// Implementa el patrón Singleton para persistir a lo largo de las distintas escenas del juego.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Mezclador de Volumen")]
    [Range(0f, 1f)] public float volumenMusica = 0.3f;
    [Range(0f, 1f)] public float volumenMar = 0.3f;
    [Range(0f, 1f)] public float volumenPasos = 0.5f;
    [Range(0f, 1f)] public float volumenEfectos = 1.0f;
    [Range(0f, 1f)] public float volumenCarrete = 0.6f;

    [Header("Clips de Audio - Ambiente")]
    public AudioClip backgroundMusic;
    public AudioClip sonidoMar; 

    [Header("Clips de Audio - Pasos")]
    public AudioClip pasoA;
    public AudioClip pasoB;

    [Header("Clips de Audio - Pesca")]
    public AudioClip sonidoLanzar;
    public AudioClip sonidoPicarLoop; 
    public AudioClip sonidoCaptura;   
    public AudioClip sonidoPerder;    

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

    void Start()
    {
        PlayBackgroundMusic();
        PlayAmbience();
    }

    void Update()
    {
        if (musicSource != null) musicSource.volume = volumenMusica;
        if (ambienceSource != null) ambienceSource.volume = volumenMar;
        if (sfxLoopSource != null) sfxLoopSource.volume = volumenCarrete;
    }

    /// <summary>
    /// Reproduce la pista de música de fondo asignada de forma ininterrumpida.
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Reproduce el sonido de ambiente asignado (ej. olas del mar) de forma ininterrumpida.
    /// </summary>
    public void PlayAmbience()
    {
        if (sonidoMar != null)
        {
            ambienceSource.clip = sonidoMar;
            ambienceSource.Play();
        }
    }

    /// <summary>
    /// Reproduce de forma alterna los sonidos de pasos del jugador aplicando una leve variación de tono para mayor realismo.
    /// </summary>
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

    /// <summary>
    /// Reproduce una sola vez el efecto de sonido correspondiente al lanzamiento de la caña.
    /// </summary>
    public void SFX_Lanzar()
    {
        if (sonidoLanzar != null) effectsSource.PlayOneShot(sonidoLanzar, volumenEfectos);
    }

    /// <summary>
    /// Inicia la reproducción en bucle del sonido del carrete durante el forcejeo del minijuego de pesca.
    /// </summary>
    public void SFX_EmpezarForcejeo() 
    {
        if (sonidoPicarLoop != null)
        {
            sfxLoopSource.clip = sonidoPicarLoop;
            sfxLoopSource.Play(); 
        }
    }

    /// <summary>
    /// Detiene la reproducción en bucle del sonido del carrete.
    /// </summary>
    public void SFX_PararForcejeo() 
    {
        sfxLoopSource.Stop();
    }

    /// <summary>
    /// Detiene el sonido de forcejeo y reproduce el efecto sonoro de captura exitosa.
    /// </summary>
    public void SFX_Ganar()
    {
        SFX_PararForcejeo();
        if (sonidoCaptura != null) effectsSource.PlayOneShot(sonidoCaptura, volumenEfectos);
    }

    /// <summary>
    /// Detiene el sonido de forcejeo y reproduce el efecto sonoro de pez escapado.
    /// </summary>
    public void SFX_Perder()
    {
        SFX_PararForcejeo();
        if (sonidoPerder != null) effectsSource.PlayOneShot(sonidoPerder, volumenEfectos);
    }
}