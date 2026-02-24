using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private MicrophoneInput _micInput;

    private bool _isPaused;
    private GameObject _pausePanel;
    private Toggle _debugToggle;
    private Toggle _pitchToggle;

    private void Start()
    {
        var micInput = _micInput;
        if (micInput == null)
            micInput = FindFirstObjectByType<MicrophoneInput>();
        _micInput = micInput;

        BuildUI();
        _pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused) Resume();
            else Pause();
        }
    }

    private void Pause()
    {
        _isPaused = true;
        _pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Resume()
    {
        _isPaused = false;
        _pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void BuildUI()
    {
        var canvasGO = new GameObject("PauseCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        _pausePanel = CreatePanel(canvasGO.transform);

        CreateLabel(_pausePanel.transform, "PAUSED", 60, new Vector2(0, 120));

        CreateButton(_pausePanel.transform, "Resume", new Vector2(0, 20), Resume);

        _debugToggle = CreateToggle(_pausePanel.transform, "Debug Mode", new Vector2(0, -50));
        _debugToggle.onValueChanged.AddListener(isOn =>
        {
            if (_micInput != null)
                _micInput.DebugMode = isOn;
        });

        _pitchToggle = CreateToggle(_pausePanel.transform, "Use Pitch (off = Volume)", new Vector2(0, -100));
        _pitchToggle.isOn = _micInput != null && _micInput.UsePitch;
        _pitchToggle.onValueChanged.AddListener(isOn =>
        {
            if (_micInput != null)
                _micInput.UsePitch = isOn;
        });

        CreateLabel(_pausePanel.transform, "Debug:  Hold [,] = Yellow   Hold [.] = Red", 20, new Vector2(0, -160));
    }

    private static GameObject CreatePanel(Transform parent)
    {
        var go = new GameObject("PausePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = go.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.75f);

        return go;
    }

    private static Text CreateLabel(Transform parent, string text, int fontSize, Vector2 position)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(600, fontSize + 20);

        var t = go.GetComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fontSize;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;

        return t;
    }

    private static Button CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(200, 50);

        var img = go.GetComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        var btn = go.GetComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.15f);
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        CreateLabel(go.transform, label, 28, Vector2.zero);

        return btn;
    }

    private static Toggle CreateToggle(Transform parent, string label, Vector2 position)
    {
        var go = new GameObject("DebugToggle", typeof(RectTransform), typeof(Toggle));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(300, 40);

        var bgGO = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgGO.transform.SetParent(go.transform, false);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchoredPosition = new Vector2(-120, 0);
        bgRT.sizeDelta = new Vector2(36, 36);
        bgGO.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);

        var checkGO = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        checkGO.transform.SetParent(bgGO.transform, false);
        var checkRT = checkGO.GetComponent<RectTransform>();
        checkRT.anchorMin = Vector2.zero;
        checkRT.anchorMax = Vector2.one;
        checkRT.offsetMin = new Vector2(4, 4);
        checkRT.offsetMax = new Vector2(-4, -4);
        checkGO.GetComponent<Image>().color = new Color(0.3f, 0.9f, 0.3f, 1f);

        CreateLabel(go.transform, label, 26, new Vector2(20, 0));

        var toggle = go.GetComponent<Toggle>();
        toggle.isOn = false;
        toggle.graphic = checkGO.GetComponent<Image>();
        toggle.targetGraphic = bgGO.GetComponent<Image>();

        return toggle;
    }
}
