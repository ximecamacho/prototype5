using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private MicrophoneInput _micInput;

    private bool _isPaused;
    private GameObject _pausePanel;
    private Toggle _debugToggle;
    private Toggle _pitchToggle;
    private Slider _sensitivitySlider;
    private Text _sensitivityLabel;
    private Slider _noiseGateSlider;
    private Text _noiseGateLabel;
    private Slider _smoothingSlider;
    private Text _smoothingLabel;

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

        if (_micInput != null)
        {
            _sensitivitySlider.SetValueWithoutNotify(_micInput.Sensitivity);
            _sensitivityLabel.text = _micInput.Sensitivity.ToString("F0");
            _noiseGateSlider.SetValueWithoutNotify(_micInput.NoiseGate);
            _noiseGateLabel.text = _micInput.NoiseGate.ToString("F4");
            _smoothingSlider.SetValueWithoutNotify(_micInput.Smoothing);
            _smoothingLabel.text = _micInput.Smoothing.ToString("F0");
            _pitchToggle.SetIsOnWithoutNotify(_micInput.UsePitch);
            _debugToggle.SetIsOnWithoutNotify(_micInput.DebugMode);
        }
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

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

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

        _sensitivitySlider = CreateSlider(_pausePanel.transform, "Sensitivity", new Vector2(0, -160),
            1f, 200f, _micInput != null ? _micInput.Sensitivity : 45f, out _sensitivityLabel);
        _sensitivitySlider.wholeNumbers = true;
        _sensitivitySlider.onValueChanged.AddListener(val =>
        {
            if (_micInput != null) _micInput.Sensitivity = val;
            _sensitivityLabel.text = val.ToString("F0");
        });

        _noiseGateSlider = CreateSlider(_pausePanel.transform, "Noise Gate", new Vector2(0, -230),
            0f, 0.05f, _micInput != null ? _micInput.NoiseGate : 0.005f, out _noiseGateLabel);
        _noiseGateSlider.onValueChanged.AddListener(val =>
        {
            if (_micInput != null) _micInput.NoiseGate = val;
            _noiseGateLabel.text = val.ToString("F4");
        });

        _smoothingSlider = CreateSlider(_pausePanel.transform, "Smoothing", new Vector2(0, -300),
            1f, 30f, _micInput != null ? _micInput.Smoothing : 5f, out _smoothingLabel);
        _smoothingSlider.wholeNumbers = true;
        _smoothingSlider.onValueChanged.AddListener(val =>
        {
            if (_micInput != null) _micInput.Smoothing = (int)val;
            _smoothingLabel.text = val.ToString("F0");
        });

        CreateLabel(_pausePanel.transform, "Debug:  Hold [,] = Yellow   Hold [.] = Red", 20, new Vector2(0, -360));
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
        t.raycastTarget = false;

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

    private static Slider CreateSlider(Transform parent, string label, Vector2 position,
        float min, float max, float initial, out Text valueLabel)
    {
        var go = new GameObject(label + "Slider", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(500, 50);

        CreateLabel(go.transform, label, 22, new Vector2(-200, 0));

        var sliderGO = DefaultControls.CreateSlider(new DefaultControls.Resources());
        sliderGO.transform.SetParent(go.transform, false);
        var sliderRT = sliderGO.GetComponent<RectTransform>();
        sliderRT.anchoredPosition = new Vector2(30, 0);
        sliderRT.sizeDelta = new Vector2(250, 20);

        sliderGO.transform.Find("Background").GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);
        sliderGO.transform.Find("Fill Area/Fill").GetComponent<Image>().color = new Color(0.3f, 0.7f, 0.9f, 1f);
        sliderGO.transform.Find("Handle Slide Area/Handle").GetComponent<Image>().color = Color.white;

        var slider = sliderGO.GetComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.SetValueWithoutNotify(initial);

        valueLabel = CreateLabel(go.transform, max <= 1f ? initial.ToString("F4") : initial.ToString("F0"),
            22, new Vector2(210, 0));

        return slider;
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
