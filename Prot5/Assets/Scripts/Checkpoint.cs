using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MicrophoneInput _micInput;
    [SerializeField] private PlayerRespawn _playerRespawn;

    [Header("Color Sequence (0=Green, 1=Yellow, 2=Red)")]
    [SerializeField] private int[] _colorSequence = { 0, 1, 2 };

    [Header("Timing")]
    [Tooltip("Seconds the player must hold the correct zone to confirm a step")]
    [SerializeField] private float _confirmDuration = 0.5f;

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer _lockIcon;
    [SerializeField] private SpriteRenderer _flagTop;
    [SerializeField] private SpriteRenderer _flagBottom;

    [Header("Square Appearance")]
    [Tooltip("Sprite used for colored squares (e.g. a white square/circle)")]
    [SerializeField] private Sprite _squareSprite;
    [SerializeField] private float _squareSize = 0.5f;
    [SerializeField] private float _squareSpacing = 0.6f;
    [Tooltip("Height above the checkpoint where squares appear")]
    [SerializeField] private float _squareHeight = 1.5f;
    [SerializeField] private string _squareSortingLayer = "Default";
    [SerializeField] private int _squareSortingOrder = 10;

    private static readonly Color[] ZoneColors =
    {
        new(0f, 0.8f, 0.2f, 1f),
        new(1f, 0.85f, 0f, 1f),
        new(0.9f, 0.15f, 0.15f, 1f)
    };

    private static readonly Color DimColor = new(0.3f, 0.3f, 0.3f, 0.5f);
    private static readonly Color CompleteColor = new(1f, 1f, 1f, 0.4f);

    private SpriteRenderer[] _squares;
    private int _currentStep;
    private bool _isPlayerInZone;
    private bool _isComplete;
    private float _matchTimer;
    private Transform _sequenceParent;

    private void Start()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;

        _flagTop.gameObject.SetActive(false);
        _flagBottom.gameObject.SetActive(false);
        _lockIcon.gameObject.SetActive(true);

        BuildSquares();
        UpdateSquareVisuals();
    }

    private void BuildSquares()
    {
        _sequenceParent = new GameObject("ColorSequence").transform;
        _sequenceParent.SetParent(transform);
        _sequenceParent.localPosition = new Vector3(0f, _squareHeight, 0f);

        int count = _colorSequence.Length;
        _squares = new SpriteRenderer[count];
        float totalWidth = (count - 1) * _squareSpacing;

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"Square{i}");
            go.transform.SetParent(_sequenceParent);
            go.transform.localPosition = new Vector3(-totalWidth / 2f + i * _squareSpacing, 0f, 0f);
            go.transform.localScale = Vector3.one * _squareSize;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _squareSprite;
            sr.sortingLayerName = _squareSortingLayer;
            sr.sortingOrder = _squareSortingOrder;
            _squares[i] = sr;
        }
    }

    private void Update()
    {
        if (_isComplete || !_isPlayerInZone || _micInput == null) return;

        int targetZone = _colorSequence[_currentStep];

        if (_micInput.Zone == targetZone)
        {
            _matchTimer += Time.deltaTime;
            if (_matchTimer >= _confirmDuration)
                AdvanceStep();
        }
        else
        {
            _matchTimer = 0f;
        }

        UpdateSquareVisuals();
    }

    private void AdvanceStep()
    {
        _matchTimer = 0f;
        _currentStep++;

        if (_currentStep >= _colorSequence.Length)
            CompleteCheckpoint();
    }

    private void CompleteCheckpoint()
    {
        _isComplete = true;

        _lockIcon.gameObject.SetActive(false);
        _flagTop.gameObject.SetActive(true);
        _flagBottom.gameObject.SetActive(true);

        if (_playerRespawn != null)
            _playerRespawn.SetSpawnPoint(transform.position);

        UpdateSquareVisuals();
    }

    private void UpdateSquareVisuals()
    {
        for (int i = 0; i < _squares.Length; i++)
        {
            int zone = _colorSequence[i];
            Color baseColor = zone < ZoneColors.Length ? ZoneColors[zone] : Color.white;

            if (_isComplete)
            {
                _squares[i].color = CompleteColor;
            }
            else if (i < _currentStep)
            {
                _squares[i].color = baseColor * 0.5f;
            }
            else if (i == _currentStep && _isPlayerInZone)
            {
                float pulse = 0.7f + 0.3f * Mathf.Sin(Time.time * 5f);
                _squares[i].color = baseColor * pulse;
            }
            else
            {
                _squares[i].color = DimColor;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Checkpoint: OnTriggerEnter2D with {other.gameObject.name} (layer {other.gameObject.layer})");
        if (_isComplete) return;

        if (IsPlayer(other))
        {
            Debug.Log("Checkpoint: Player entered!");
            _isPlayerInZone = true;
            _matchTimer = 0f;
            UpdateSquareVisuals();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            _isPlayerInZone = false;
            _matchTimer = 0f;
            UpdateSquareVisuals();
        }
    }

    private static bool IsPlayer(Collider2D col)
    {
        return col.gameObject.layer == 6
            || col.GetComponent<PlayerRespawn>() != null
            || col.GetComponentInParent<PlayerRespawn>() != null;
    }
}
