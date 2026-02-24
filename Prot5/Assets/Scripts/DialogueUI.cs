using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [SerializeField] private float _charsPerSecond = 40f;

    private GameObject _canvasGO;
    private GameObject _dialogueRoot;
    private GameObject _panel;
    private Text _text;
    private bool _isShowing;
    private bool _textFullyRevealed;
    private string _fullText;
    private Coroutine _typewriterCoroutine;
    private Action _onDismiss;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        BuildUI();
        _dialogueRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!_isShowing) return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            if (!_textFullyRevealed)
            {
                if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
                _text.text = _fullText;
                _textFullyRevealed = true;
            }
            else
            {
                Hide();
            }
        }
    }

    public bool IsShowing => _isShowing;

    public void Show(string text, Action onDismiss = null)
    {
        _fullText = text;
        _onDismiss = onDismiss;
        _isShowing = true;
        _textFullyRevealed = false;
        _text.text = "";
        _dialogueRoot.SetActive(true);

        if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterRoutine());
    }

    private void Hide()
    {
        _isShowing = false;
        _dialogueRoot.SetActive(false);
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }

        var cb = _onDismiss;
        _onDismiss = null;
        cb?.Invoke();
    }

    private IEnumerator TypewriterRoutine()
    {
        int revealed = 0;
        float timer = 0f;
        float interval = 1f / Mathf.Max(_charsPerSecond, 1f);

        while (revealed < _fullText.Length)
        {
            timer += Time.unscaledDeltaTime;
            while (timer >= interval && revealed < _fullText.Length)
            {
                revealed++;
                timer -= interval;
            }
            _text.text = _fullText.Substring(0, revealed);
            yield return null;
        }

        _textFullyRevealed = true;
        _typewriterCoroutine = null;
    }

    private void BuildUI()
    {
        _canvasGO = new GameObject("DialogueCanvas");
        _canvasGO.transform.SetParent(transform);
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        var scaler = _canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var border = new GameObject("DialogueBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        border.transform.SetParent(_canvasGO.transform, false);
        var borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = new Vector2(0.05f, 0.02f);
        borderRT.anchorMax = new Vector2(0.95f, 0.22f);
        borderRT.offsetMin = new Vector2(-3, -3);
        borderRT.offsetMax = new Vector2(3, 3);
        border.GetComponent<Image>().color = Color.white;
        border.GetComponent<Image>().raycastTarget = false;
        _dialogueRoot = border;

        _panel = new GameObject("DialoguePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        _panel.transform.SetParent(border.transform, false);
        var panelRT = _panel.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = new Vector2(3, 3);
        panelRT.offsetMax = new Vector2(-3, -3);
        _panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.92f);

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textGO.transform.SetParent(_panel.transform, false);
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(30, 20);
        textRT.offsetMax = new Vector2(-30, -20);

        _text = textGO.GetComponent<Text>();
        _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _text.fontSize = 36;
        _text.color = Color.white;
        _text.alignment = TextAnchor.MiddleLeft;
        _text.horizontalOverflow = HorizontalWrapMode.Wrap;
        _text.verticalOverflow = VerticalWrapMode.Overflow;
        _text.raycastTarget = false;
    }
}
