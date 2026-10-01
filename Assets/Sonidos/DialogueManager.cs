using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Necesario para leer clics con el nuevo Input System
using TMPro;

// Cada línea de diálogo: quién habla, qué dice, y su retrato
[System.Serializable]
public class DialogoLinea
{
    public string nombrePersonaje;
    [TextArea(2, 4)]
    public string texto;
    public Sprite spritePersonaje; // El retrato que aparece en la caja de diálogo
}

public class DialogueManager : MonoBehaviour
{
    [Header("Guion del diálogo INICIAL (antes de las preguntas)")]
    public List<DialogoLinea> dialogo;

    [Header("Elementos de la Interfaz")]
    public GameObject panelDialogo;       // El panel completo de la caja de diálogo
    public TextMeshProUGUI textoNombre;   // El texto sobre el cartel de madera ("Player")
    public TextMeshProUGUI textoDialogo;  // El texto dentro del cuadro blanco
    public Image imagenPersonaje;         // El retrato del personaje (a la derecha)

    [Header("Conexión con el juego")]
    public GameManager gameManager; // Se avisa a este script cuando el diálogo INICIAL termina

    [Header("Audio (opcional)")]
    public AudioSource audioSource;
    public AudioClip sonidoAvanzar;

    // Guion que se está mostrando ACTUALMENTE (puede ser el inicial o uno final)
    private List<DialogoLinea> guionActivo;
    private int lineaActual = 0;
    private bool dialogoActivo = false;

    // Qué hacer cuando el guion actual termina (varía según cuál guion se esté mostrando)
    private System.Action callbackAlTerminar;

    void Start()
    {
        // Al iniciar el juego, se reproduce el guion de la introducción,
        // y al terminar se avisa al GameManager para que arranquen las preguntas.
        ReproducirGuion(dialogo, () =>
        {
            if (gameManager != null) gameManager.IniciarPreguntas();
        });
    }

    void Update()
    {
        // Mientras el diálogo esté activo, cualquier clic avanza a la siguiente línea
        if (dialogoActivo && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            AvanzarDialogo();
        }
    }

    // Método genérico: reproduce cualquier lista de líneas y ejecuta "alTerminar" al acabar.
    // Lo usa tanto el diálogo inicial (arriba) como el GameManager para las historias finales.
    public void ReproducirGuion(List<DialogoLinea> lineas, System.Action alTerminar)
    {
        if (lineas == null || lineas.Count == 0)
        {
            // Si no hay líneas configuradas, ejecuta directamente lo que seguía después
            alTerminar?.Invoke();
            return;
        }

        guionActivo = lineas;
        lineaActual = 0;
        callbackAlTerminar = alTerminar;
        dialogoActivo = true;

        if (panelDialogo != null) panelDialogo.SetActive(true);

        MostrarLinea();
    }

    // Método que llama el GameManager para mostrar la historia final (buena o mala)
    public void MostrarFinal(List<DialogoLinea> lineasFinal)
    {
        // Al terminar la historia final, no hay nada más que hacer (el juego se queda ahí)
        ReproducirGuion(lineasFinal, null);
    }

    void MostrarLinea()
    {
        DialogoLinea linea = guionActivo[lineaActual];

        if (textoNombre != null) textoNombre.text = linea.nombrePersonaje;
        if (textoDialogo != null) textoDialogo.text = linea.texto;

        if (imagenPersonaje != null)
        {
            if (linea.spritePersonaje != null)
            {
                imagenPersonaje.sprite = linea.spritePersonaje;
                imagenPersonaje.enabled = true;
            }
            else
            {
                imagenPersonaje.enabled = false; // Oculta el retrato si esta línea no tiene
            }
        }
    }

    public void AvanzarDialogo()
    {
        if (audioSource != null && sonidoAvanzar != null)
        {
            audioSource.PlayOneShot(sonidoAvanzar);
        }

        lineaActual++;

        if (lineaActual < guionActivo.Count)
        {
            MostrarLinea();
        }
        else
        {
            TerminarGuion();
        }
    }

    void TerminarGuion()
    {
        dialogoActivo = false;

        if (panelDialogo != null) panelDialogo.SetActive(false);

        // Ejecuta lo que corresponda: iniciar preguntas (guion inicial) o nada (guion final)
        callbackAlTerminar?.Invoke();
    }
}