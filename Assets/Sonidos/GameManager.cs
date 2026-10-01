using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Librería necesaria para usar TextMeshPro

// Esta clase define cómo es una pregunta para que Unity la entienda
[System.Serializable]
public class PreguntaTrivia
{
    [TextArea(2, 5)]
    public string textoPregunta;
    public string[] opciones = new string[4];
    public int indiceCorrecta; // 0=A, 1=B, 2=C, 3=D
}

public class GameManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelPreguntas; // Contiene la pregunta y los 4 botones
    public GameObject panelCartas;    // Contiene las cartas de la fase final

    [Header("Elementos de la Interfaz - Preguntas")]
    public TextMeshProUGUI textoPreguntaUI;
    public Button[] botonesOpciones; // Arrastra aquí los 4 botones
    public TextMeshProUGUI[] textosBotones; // Arrastra aquí los textos de esos 4 botones
    public TextMeshProUGUI textoProgresoUI; // Opcional: "Pregunta 3/12 - Aciertos: 2"

    [Header("Configuración del Juego")]
    public List<PreguntaTrivia> bancoPreguntas; // Agrega aquí TODAS las preguntas que quieras (10, 30, 100...)
    public int cantidadPreguntasAJugar = 10;    // Cuántas de esas preguntas se usarán en cada partida (elegidas al azar)
    private List<PreguntaTrivia> preguntasSeleccionadas; // Las preguntas que realmente se van a jugar esta partida
    private int preguntaActual = 0;
    private int aciertos = 0;
    private int fallos = 0;
    private bool esperandoSiguiente = false;
    private List<bool> respuestasCorrectas = new List<bool>(); // true = acertaste esa pregunta, en el orden que se respondieron

    [Header("Elementos de la Interfaz - Cartas")]
    public Button[] botonesCartas;   // Arrastra aquí las cartas boca abajo (ej: 5 cartas)
    public Image[] imagenesCartas;   // La imagen de cada carta (mismo orden que botonesCartas)
    public Sprite cartaTapada;       // Sprite del reverso de la carta
    public Sprite cartaGanadora;     // Sprite de carta "ganadora" (ej: corazón, carita feliz)
    public Sprite cartaPerdedora;    // Sprite de carta "perdedora" (ej: calavera, carita triste)
    public TextMeshProUGUI textoResultadoFinal;
    private bool cartaYaElegida = false;
    private List<int> preguntaLigadaACarta;

    [Header("Historias Finales (diálogo)")]
    public DialogueManager dialogueManager; // El mismo sistema de diálogo del inicio
    public List<DialogoLinea> dialogoFinalBueno; // Guion que se muestra si sale carta ganadora
    public List<DialogoLinea> dialogoFinalMalo;  // Guion que se muestra si sale carta perdedora

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoCorrecto;
    public AudioClip sonidoIncorrecto;
    public AudioClip sonidoVoltearCarta;
    public AudioClip sonidoCartaGanadora;
    public AudioClip sonidoCartaPerdedora;

    [Header("Colores de Feedback (preguntas)")]
    public Color colorCorrecto = Color.green;
    public Color colorIncorrecto = Color.red;
    private Color colorOriginal;

    void Start()
    {
        if (botonesOpciones.Length > 0)
        {
            colorOriginal = botonesOpciones[0].image.color;
        }

        PrepararPreguntas();

        // El panel de preguntas empieza OCULTO - se activa cuando termina el diálogo
        if (panelCartas != null) panelCartas.SetActive(false);
        if (panelPreguntas != null) panelPreguntas.SetActive(false);
    }

    // Llamado por el DialogueManager cuando el diálogo inicial termina
    public void IniciarPreguntas()
    {
        if (panelPreguntas != null) panelPreguntas.SetActive(true);
        MostrarPregunta();
    }

    void PrepararPreguntas()
    {
        // Hace una copia del banco completo para no desordenar la lista original
        List<PreguntaTrivia> copia = new List<PreguntaTrivia>(bancoPreguntas);

        // Mezcla la copia al azar (Fisher-Yates)
        for (int i = copia.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (copia[i], copia[j]) = (copia[j], copia[i]);
        }

        // Toma solo la cantidad que el jugador configuró (sin pasarse del total disponible)
        int cantidad = Mathf.Clamp(cantidadPreguntasAJugar, 1, copia.Count);
        preguntasSeleccionadas = copia.GetRange(0, cantidad);
    }

    // ---------------------------------------------------
    //  FASE 1: PREGUNTAS
    // ---------------------------------------------------

    void MostrarPregunta()
    {
        esperandoSiguiente = false;

        // Ya no depende de "vidas" - solo se acaba cuando no quedan preguntas
        if (preguntaActual < preguntasSeleccionadas.Count)
        {
            RestaurarBotones();
            ActualizarProgreso();

            textoPreguntaUI.text = preguntasSeleccionadas[preguntaActual].textoPregunta;

            for (int i = 0; i < botonesOpciones.Length; i++)
            {
                textosBotones[i].text = preguntasSeleccionadas[preguntaActual].opciones[i];

                botonesOpciones[i].onClick.RemoveAllListeners();
                int opcionSeleccionada = i;
                botonesOpciones[i].onClick.AddListener(() => EvaluarRespuesta(opcionSeleccionada));
            }
        }
        else
        {
            // Se acabaron las preguntas -> pasamos a la fase de cartas
            IniciarFaseCartas();
        }
    }

    public void EvaluarRespuesta(int opcionSeleccionada)
    {
        if (esperandoSiguiente) return;
        esperandoSiguiente = true;

        int indiceCorrecta = preguntasSeleccionadas[preguntaActual].indiceCorrecta;
        bool esCorrecta = opcionSeleccionada == indiceCorrecta;

        foreach (Button btn in botonesOpciones)
        {
            btn.interactable = false;
        }

        respuestasCorrectas.Add(esCorrecta);

        if (esCorrecta)
        {
            aciertos++;
            ReproducirSonido(sonidoCorrecto);
            botonesOpciones[opcionSeleccionada].image.color = colorCorrecto;
        }
        else
        {
            fallos++;
            ReproducirSonido(sonidoIncorrecto);
            botonesOpciones[opcionSeleccionada].image.color = colorIncorrecto;
            botonesOpciones[indiceCorrecta].image.color = colorCorrecto;
        }

        StartCoroutine(EsperarYSiguiente());
    }

    IEnumerator EsperarYSiguiente()
    {
        yield return new WaitForSeconds(1f);
        preguntaActual++;
        MostrarPregunta();
    }

    void RestaurarBotones()
    {
        foreach (Button btn in botonesOpciones)
        {
            btn.interactable = true;
            btn.image.color = colorOriginal;
        }
    }

    void ActualizarProgreso()
    {
        if (textoProgresoUI != null)
        {
            textoProgresoUI.text = "Pregunta " + (preguntaActual + 1) + "/" + preguntasSeleccionadas.Count
                + "   |   Aciertos: " + aciertos + "   Fallos: " + fallos;
        }
    }

    // ---------------------------------------------------
    //  FASE 2: CARTAS
    // ---------------------------------------------------

    void IniciarFaseCartas()
    {
        cartaYaElegida = false;

        if (panelPreguntas != null) panelPreguntas.SetActive(false);
        if (panelCartas != null) panelCartas.SetActive(true);

        // Arma una lista con los índices de TODAS las preguntas respondidas (0, 1, 2, ...)
        // y la mezcla al azar, para que cada carta represente una pregunta distinta
        // sin que el jugador sepa cuál es cuál.
        List<int> indicesDisponibles = new List<int>();
        for (int i = 0; i < respuestasCorrectas.Count; i++)
        {
            indicesDisponibles.Add(i);
        }
        for (int i = indicesDisponibles.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (indicesDisponibles[i], indicesDisponibles[j]) = (indicesDisponibles[j], indicesDisponibles[i]);
        }

        // Si hay más cartas en pantalla que preguntas respondidas, solo se activan
        // tantas cartas como preguntas haya disponibles para ligar
        int cartasActivas = Mathf.Min(botonesCartas.Length, indicesDisponibles.Count);

        preguntaLigadaACarta = new List<int>();

        for (int i = 0; i < botonesCartas.Length; i++)
        {
            bool activa = i < cartasActivas;
            botonesCartas[i].gameObject.SetActive(activa);

            if (!activa) continue;

            preguntaLigadaACarta.Add(indicesDisponibles[i]);

            if (imagenesCartas[i] != null && cartaTapada != null)
            {
                imagenesCartas[i].sprite = cartaTapada;
            }

            botonesCartas[i].interactable = true;
            botonesCartas[i].onClick.RemoveAllListeners();
            int indiceCarta = i;
            botonesCartas[i].onClick.AddListener(() => ElegirCarta(indiceCarta));
        }

        if (textoResultadoFinal != null)
        {
            textoResultadoFinal.text = "Elige una carta boca abajo...";
        }
    }

    public void ElegirCarta(int indiceCarta)
    {
        if (cartaYaElegida) return; // evita elegir dos cartas
        cartaYaElegida = true;

        // Bloquea todas las cartas para que no se pueda elegir otra
        foreach (Button btn in botonesCartas)
        {
            btn.interactable = false;
        }

        ReproducirSonido(sonidoVoltearCarta);

        // Esta carta representa una pregunta específica: si esa pregunta se falló,
        // sale el final malo; si se acertó, sale el final bueno.
        int preguntaRepresentada = preguntaLigadaACarta[indiceCarta];
        bool gano = respuestasCorrectas[preguntaRepresentada];

        StartCoroutine(RevelarCarta(indiceCarta, gano));
    }

    IEnumerator RevelarCarta(int indiceCarta, bool gano)
    {
        // Pequeña pausa como si la carta se estuviera "volteando"
        yield return new WaitForSeconds(0.5f);

        if (imagenesCartas[indiceCarta] != null)
        {
            imagenesCartas[indiceCarta].sprite = gano ? cartaGanadora : cartaPerdedora;
        }

        if (gano)
        {
            ReproducirSonido(sonidoCartaGanadora);
            MostrarFinalFeliz();
        }
        else
        {
            ReproducirSonido(sonidoCartaPerdedora);
            MostrarFinalMalo();
        }

        // Deja ver la carta revelada y el mensaje corto antes de pasar a la historia completa
        yield return new WaitForSeconds(1.5f);

        if (panelCartas != null) panelCartas.SetActive(false);

        if (dialogueManager != null)
        {
            dialogueManager.MostrarFinal(gano ? dialogoFinalBueno : dialogoFinalMalo);
        }
    }

    void MostrarFinalFeliz()
    {
        if (textoResultadoFinal != null)
        {
            textoResultadoFinal.text = "¡CARTA GANADORA!";
        }
    }

    void MostrarFinalMalo()
    {
        if (textoResultadoFinal != null)
        {
            textoResultadoFinal.text = "CARTA PERDEDORA";
        }
    }

    // ---------------------------------------------------
    //  UTILIDADES
    // ---------------------------------------------------

    void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}