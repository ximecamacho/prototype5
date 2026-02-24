using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    private enum State { Idle, InDialogue, WaitingForDiamonds, Complete }

    [Header("References")]
    [SerializeField] private MicrophoneInput _micInput;
    [SerializeField] private Sprite _diamondSprite;

    [Header("Diamond Spawn Points (one per zone: G, Y, R)")]
    [SerializeField] private Transform[] _diamondSpawnPoints = new Transform[3];

    [Header("Zone Colors")]
    [SerializeField] private Color _greenTint = new Color(0.2f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color _yellowTint = new Color(0.95f, 0.85f, 0.15f, 1f);
    [SerializeField] private Color _redTint = new Color(0.9f, 0.2f, 0.2f, 1f);

    [Header("Interaction")]
    [SerializeField] private float _interactRange = 3f;

    private State _state = State.Idle;
    private SpriteRenderer _sr;
    private Transform _playerTransform;
    private bool _playerInRange;
    private bool _firstInteractionDone;
    private int _diamondsReturned;

    private Diamond[] _diamonds;
    private GameObject _wall;
    private GameObject _promptCanvasGO;
    private Text _promptText;
    private bool _dialogueActive;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();

        if (_micInput == null)
            _micInput = FindFirstObjectByType<MicrophoneInput>();

        _wall = transform.Find("Wall")?.gameObject;

        var respawn = FindFirstObjectByType<PlayerRespawn>();
        if (respawn != null)
            _playerTransform = respawn.transform;

        BuildPromptUI();
    }

    private void Update()
    {
        if (_state == State.Complete) return;

        UpdateColorTint();
        UpdatePlayerProximity();
        UpdatePrompt();
        HandleInteractInput();
        UpdateDiamondTrailIndices();
    }

    private void UpdateColorTint()
    {
        if (_sr == null || _micInput == null) return;
        _sr.color = GetZoneColor(_micInput.Zone);
    }

    private Color GetZoneColor(int zone)
    {
        switch (zone)
        {
            case 1: return _yellowTint;
            case 2: return _redTint;
            default: return _greenTint;
        }
    }

    private void UpdatePlayerProximity()
    {
        if (_playerTransform == null) return;
        float dist = Vector2.Distance(transform.position, _playerTransform.position);
        _playerInRange = dist <= _interactRange;
    }

    private void UpdatePrompt()
    {
        if (_promptText == null) return;

        bool show = _playerInRange && !_dialogueActive && _state != State.Complete;
        if (show && _state == State.WaitingForDiamonds)
            show = HasAnyCollectedDiamond();

        _promptCanvasGO.SetActive(show);
    }

    private void HandleInteractInput()
    {
        if (!_playerInRange) return;
        if (_dialogueActive) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (DialogueUI.Instance != null && DialogueUI.Instance.IsShowing) return;

        switch (_state)
        {
            case State.Idle:
                StartFirstInteraction();
                break;
            case State.WaitingForDiamonds:
                TryReturnDiamond();
                break;
        }
    }

    private void StartFirstInteraction()
    {
        _dialogueActive = true;
        _state = State.InDialogue;
        DialogueUI.Instance.Show("Please find our diamonds! One for each of us!", () =>
        {
            _dialogueActive = false;
            _firstInteractionDone = true;
            _state = State.WaitingForDiamonds;
            SpawnDiamonds();
        });
    }

    private void SpawnDiamonds()
    {
        _diamonds = new Diamond[3];
        Color[] colors = { _greenTint, _yellowTint, _redTint };

        for (int i = 0; i < 3; i++)
        {
            Vector3 pos = (_diamondSpawnPoints != null && i < _diamondSpawnPoints.Length && _diamondSpawnPoints[i] != null)
                ? _diamondSpawnPoints[i].position
                : transform.position + Vector3.right * (3 + i * 3);

            var go = new GameObject($"Diamond_{i}");
            go.transform.position = pos;
            var diamond = go.AddComponent<Diamond>();
            diamond.Init(i, colors[i], _diamondSprite, _micInput);
            _diamonds[i] = diamond;
        }
    }

    private void TryReturnDiamond()
    {
        if (_diamonds == null || _micInput == null) return;

        int currentZone = _micInput.Zone;
        Diamond matchingCollected = null;
        Diamond anyCollected = null;

        foreach (var d in _diamonds)
        {
            if (d == null || !d.IsCollected || d.IsGiven) continue;
            anyCollected = d;
            if (d.Zone == currentZone)
            {
                matchingCollected = d;
                break;
            }
        }

        if (anyCollected == null) return;

        _dialogueActive = true;

        if (matchingCollected != null)
        {
            matchingCollected.Give();
            _diamondsReturned++;

            if (_diamondsReturned >= 3)
            {
                DialogueUI.Instance.Show("Thank you thank you! You've returned them all!", () =>
                {
                    _dialogueActive = false;
                    CompleteNPC();
                });
            }
            else
            {
                DialogueUI.Instance.Show("Thank you thank you!", () =>
                {
                    _dialogueActive = false;
                });
            }
        }
        else
        {
            DialogueUI.Instance.Show("That's not my diamond. Maybe it's my brother's?", () =>
            {
                _dialogueActive = false;
            });
        }
    }

    private void CompleteNPC()
    {
        _state = State.Complete;
        if (_wall != null) _wall.SetActive(false);
        if (_promptCanvasGO != null) _promptCanvasGO.SetActive(false);
        gameObject.SetActive(false);
    }

    private bool HasAnyCollectedDiamond()
    {
        if (_diamonds == null) return false;
        foreach (var d in _diamonds)
        {
            if (d != null && d.IsCollected && !d.IsGiven)
                return true;
        }
        return false;
    }

    private void UpdateDiamondTrailIndices()
    {
        if (_diamonds == null) return;
        int idx = 0;
        foreach (var d in _diamonds)
        {
            if (d != null && d.IsCollected && !d.IsGiven)
            {
                d.SetTrailIndex(idx);
                idx++;
            }
        }
    }

    private void BuildPromptUI()
    {
        _promptCanvasGO = new GameObject("InteractPromptCanvas");
        var canvas = _promptCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;
        var scaler = _promptCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var textGO = new GameObject("PromptText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textGO.transform.SetParent(_promptCanvasGO.transform, false);
        var rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.3f, 0.45f);
        rt.anchorMax = new Vector2(0.7f, 0.55f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        _promptText = textGO.GetComponent<Text>();
        _promptText.text = "[E] to Interact";
        _promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _promptText.fontSize = 40;
        _promptText.color = Color.black;
        _promptText.alignment = TextAnchor.MiddleCenter;
        _promptText.raycastTarget = false;

        var shadow = textGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(2, -2);

        _promptCanvasGO.SetActive(false);
    }
}
