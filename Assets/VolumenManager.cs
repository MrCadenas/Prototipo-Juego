using UnityEngine;


public class VolumenManager : MonoBehaviour
{
    public static VolumenManager Instancia;

    [Header("Música de fondo")]
    public AudioSource audioSourceMusica; // AudioSource exclusivo para la música
    public AudioClip musicaFondo;

    // Volumen de efectos actual, leído por GameManager y DialogueManager en cada sonido
    public static float VolumenEfectos { get; private set; } = 1f;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        if (audioSourceMusica != null && musicaFondo != null)
        {
            audioSourceMusica.clip = musicaFondo;
            audioSourceMusica.loop = true;
            audioSourceMusica.Play();
        }
    }

    // Para conectar desde el futuro menú de opciones de tu compañera
    public void CambiarVolumenMusica(float valor)
    {
        if (audioSourceMusica != null) audioSourceMusica.volume = valor;
    }

    // Para conectar desde el futuro menú de opciones de tu compañera
    public void CambiarVolumenEfectos(float valor)
    {
        VolumenEfectos = valor;
    }
}
