#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TMPro;

// Herramienta de un solo uso: Tools > Crear HUD de Vidas y Game Over
// Crea 3 iconos de vida y el panel Game Over dentro de "Canvas" y los enlaza al GameManager.
public static class CrearHudVidas
{
    [MenuItem("Tools/Crear HUD de Vidas y Game Over")]
    public static void Crear()
    {
        GameObject canvas = GameObject.Find("Canvas");
        GameManager gm = Object.FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
        if (canvas == null || gm == null)
        {
            Debug.LogError("No se encontró el objeto 'Canvas' o el GameManager en la escena.");
            return;
        }
        Transform raiz = canvas.transform;

        // ---------- HUD de vidas ----------
        GameObject hud = NuevoUI("HUDVidas", raiz);
        RectTransform hudRt = hud.GetComponent<RectTransform>();
        Anclar(hudRt, new Vector2(0, 1), new Vector2(20, -20), new Vector2(180, 50));

        GameObject[] iconos = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject v = NuevoUI("Vida" + (i + 1), hud.transform);
            Image img = v.AddComponent<Image>();
            img.color = new Color(0.9f, 0.15f, 0.2f);
            Anclar(v.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(i * 60, 0), new Vector2(50, 50));
            iconos[i] = v;
        }

        // ---------- Panel Game Over ----------
        GameObject panel = NuevoUI("PanelGameOver", raiz);
        RectTransform prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0, 0, 0, 0.85f);

        GameObject titulo = NuevoUI("TextoGameOver", panel.transform);
        TextMeshProUGUI tt = titulo.AddComponent<TextMeshProUGUI>();
        tt.text = "GAME OVER";
        tt.fontSize = 72;
        tt.alignment = TextAlignmentOptions.Center;
        tt.color = new Color(0.9f, 0.15f, 0.2f);
        Anclar(titulo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(700, 120));

        GameObject botonGo = NuevoUI("BotonReintentar", panel.transform);
        Image bimg = botonGo.AddComponent<Image>();
        bimg.color = new Color(0.4f, 0.8f, 0.8f);
        Button boton = botonGo.AddComponent<Button>();
        boton.targetGraphic = bimg;
        Anclar(botonGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(260, 70));

        GameObject txtBtn = NuevoUI("Texto", botonGo.transform);
        TextMeshProUGUI bt = txtBtn.AddComponent<TextMeshProUGUI>();
        bt.text = "REINTENTAR";
        bt.fontSize = 32;
        bt.alignment = TextAlignmentOptions.Center;
        bt.color = Color.black;
        RectTransform brt = txtBtn.GetComponent<RectTransform>();
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;

        panel.SetActive(false); // el GameManager lo activa al perder

        // ---------- Enlazar con GameManager ----------
        SerializedObject so = new SerializedObject(gm);
        so.FindProperty("panelGameOver").objectReferenceValue = panel;
        so.FindProperty("botonReintentar").objectReferenceValue = boton;
        SerializedProperty arr = so.FindProperty("iconosVidas");
        arr.arraySize = 3;
        for (int i = 0; i < 3; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = iconos[i];
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("HUD de vidas y panel Game Over creados y enlazados. Guarda la escena (Ctrl+S).");
    }

    static GameObject NuevoUI(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Crear " + nombre);
        go.transform.SetParent(padre, false);
        return go;
    }

    static void Anclar(RectTransform rt, Vector2 ancla, Vector2 pos, Vector2 tam)
    {
        rt.anchorMin = ancla; rt.anchorMax = ancla; rt.pivot = ancla;
        rt.anchoredPosition = pos; rt.sizeDelta = tam;
    }
}
#endif
