using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public sealed class HW23DemoSceneBootstrap : MonoBehaviour
{
    [SerializeField] private WorldSaveLoadManager saveLoadManager;

    private bool initialized;
    private HW23DemoMover mover;
    private Text selectionText;
    private GameObject player;
    private GameObject box;
    private GameObject sphere;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        Debug.Log("[HW23 Bootstrap] Initializing save/load demo scene.");

        if (GameObject.Find("HW23_Player") != null)
        {
            Debug.Log("[HW23 Bootstrap] Demo objects already exist.");
            return;
        }

        if (saveLoadManager == null)
        {
            saveLoadManager = GetComponent<WorldSaveLoadManager>();
        }

        if (saveLoadManager == null)
        {
            saveLoadManager = gameObject.AddComponent<WorldSaveLoadManager>();
        }

        CreateWorld();
        Select(player.transform, "Player");
    }

    private void Reset()
    {
        if (saveLoadManager == null)
        {
            saveLoadManager = GetComponent<WorldSaveLoadManager>();
        }
    }

    private void CreateWorld()
    {
        CreateGround();

        player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "HW23_Player";
        player.transform.SetPositionAndRotation(new Vector3(0f, 1f, -2f), Quaternion.identity);
        player.GetComponent<Renderer>().material.color = new Color(0.1f, 0.45f, 0.95f);
        Register(player, "player");

        box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "HW23_SaveObject_Box";
        box.transform.SetPositionAndRotation(new Vector3(-2f, 0.5f, 1.5f), Quaternion.Euler(0f, 30f, 0f));
        box.GetComponent<Renderer>().material.color = new Color(0.95f, 0.32f, 0.22f);
        Register(box, "object_box");

        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "HW23_SaveObject_Sphere";
        sphere.transform.SetPositionAndRotation(new Vector3(2f, 0.5f, 1.5f), Quaternion.Euler(0f, -30f, 0f));
        sphere.GetComponent<Renderer>().material.color = new Color(0.12f, 0.75f, 0.42f);
        Register(sphere, "object_sphere");

        Camera.main.transform.SetPositionAndRotation(new Vector3(0f, 5f, -7f), Quaternion.Euler(35f, 0f, 0f));

        mover = gameObject.AddComponent<HW23DemoMover>();
    }

    private void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "HW23_Ground";
        ground.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        ground.GetComponent<Renderer>().material.color = new Color(0.55f, 0.55f, 0.55f);
    }

    private void Register(GameObject target, string saveId)
    {
        SaveableTransform saveable = target.AddComponent<SaveableTransform>();
        saveable.SetSaveId(saveId);
        saveLoadManager.RegisterTarget(saveable);
    }

    private void CreateUi()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            InputSystemUIInputModule inputModule = eventSystem.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }

        Canvas canvas = new GameObject("HW23_SaveLoad_Canvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<CanvasScaler>();
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.55f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(12f, -12f);
        panelRect.sizeDelta = new Vector2(360f, 320f);

        selectionText = CreateText(panel.transform, font, "Selected: Player", new Vector2(12f, -12f), new Vector2(330f, 32f), 20);
        Text help = CreateText(panel.transform, font, "WASD/Arrow: Move  Q/E: Rotate  R/F: Up/Down", new Vector2(12f, -44f), new Vector2(330f, 28f), 14);
        help.color = Color.white;

        CreateButton(panel.transform, font, "Player", new Vector2(12f, -82f), () => Select(player.transform, "Player"));
        CreateButton(panel.transform, font, "Box", new Vector2(126f, -82f), () => Select(box.transform, "Box"));
        CreateButton(panel.transform, font, "Sphere", new Vector2(240f, -82f), () => Select(sphere.transform, "Sphere"));
        CreateButton(panel.transform, font, "Save", new Vector2(12f, -130f), saveLoadManager.Save);
        CreateButton(panel.transform, font, "Load", new Vector2(126f, -130f), saveLoadManager.Load);
        CreateButton(panel.transform, font, "Forward", new Vector2(126f, -178f), () => mover.MoveSelectedForward());
        CreateButton(panel.transform, font, "Left", new Vector2(12f, -226f), () => mover.MoveSelectedLeft());
        CreateButton(panel.transform, font, "Back", new Vector2(126f, -226f), () => mover.MoveSelectedBack());
        CreateButton(panel.transform, font, "Right", new Vector2(240f, -226f), () => mover.MoveSelectedRight());
        CreateButton(panel.transform, font, "Rot -", new Vector2(12f, -274f), () => mover.RotateSelectedLeft());
        CreateButton(panel.transform, font, "Rot +", new Vector2(126f, -274f), () => mover.RotateSelectedRight());

        Text status = CreateText(panel.transform, font, "", new Vector2(12f, -356f), new Vector2(520f, 60f), 14);
        status.transform.SetParent(canvas.transform, false);
        RectTransform statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 0f);
        statusRect.anchorMax = new Vector2(1f, 0f);
        statusRect.pivot = new Vector2(0f, 0f);
        statusRect.anchoredPosition = new Vector2(12f, 12f);
        statusRect.sizeDelta = new Vector2(-24f, 60f);

        saveLoadManager.SetStatusText(status);
    }

    private Text CreateText(Transform parent, Font font, string value, Vector2 position, Vector2 size, int fontSize)
    {
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        RectTransform rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        return text;
    }

    private void CreateButton(Transform parent, Font font, string label, Vector2 position, UnityAction action)
    {
        GameObject buttonObject = new GameObject(label + " Button");
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.92f, 0.92f, 0.92f, 0.95f);
        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(action);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(102f, 36f);

        Text text = CreateText(buttonObject.transform, font, label, Vector2.zero, rectTransform.sizeDelta, 16);
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
    }

    private void Select(Transform target, string label)
    {
        mover.SelectTarget(target);
        if (selectionText != null)
        {
            selectionText.text = "Selected: " + label;
        }
    }

    private void OnGUI()
    {
        if (!initialized || mover == null || player == null || box == null || sphere == null)
        {
            return;
        }

        Rect windowRect = new Rect(Mathf.Max(20f, (Screen.width - 340f) * 0.5f), 24f, 340f, 250f);
        GUILayout.BeginArea(windowRect, "HW23 Save / Load", GUI.skin.window);

        GUILayout.Space(22f);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save", GUILayout.Height(34f))) saveLoadManager.Save();
        if (GUILayout.Button("Load", GUILayout.Height(34f))) saveLoadManager.Load();
        GUILayout.EndHorizontal();

        GUILayout.Label("Selected: " + mover.SelectedTarget.name);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Player", GUILayout.Height(30f))) Select(player.transform, "Player");
        if (GUILayout.Button("Box", GUILayout.Height(30f))) Select(box.transform, "Box");
        if (GUILayout.Button("Sphere", GUILayout.Height(30f))) Select(sphere.transform, "Sphere");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(110f);
        if (GUILayout.Button("Forward", GUILayout.Width(110f), GUILayout.Height(30f))) mover.MoveSelectedForward();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Left", GUILayout.Height(30f))) mover.MoveSelectedLeft();
        if (GUILayout.Button("Back", GUILayout.Height(30f))) mover.MoveSelectedBack();
        if (GUILayout.Button("Right", GUILayout.Height(30f))) mover.MoveSelectedRight();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Rot -", GUILayout.Height(30f))) mover.RotateSelectedLeft();
        if (GUILayout.Button("Rot +", GUILayout.Height(30f))) mover.RotateSelectedRight();
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }
}
