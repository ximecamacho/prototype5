using UnityEngine;

public class Diamond : MonoBehaviour
{
    public int Zone { get; private set; }
    public bool IsCollected { get; private set; }
    public bool IsGiven { get; private set; }

    private MicrophoneInput _micInput;
    private Transform _playerTransform;
    private PlayerRespawn _playerRespawn;
    private SpriteRenderer _sr;
    private BoxCollider2D _collider;
    private Vector3 _originalPosition;
    private int _trailIndex;
    private float _trailSpacing = 1.0f;
    private float _trailYOffset = 0.5f;
    private float _followSpeed = 8f;

    public void Init(int zone, Color tint, Sprite sprite, MicrophoneInput micInput)
    {
        Zone = zone;
        _micInput = micInput;
        _originalPosition = transform.position;

        _sr = gameObject.AddComponent<SpriteRenderer>();
        _sr.sprite = MakeWhiteSprite(sprite);
        _sr.color = tint;
        _sr.sortingLayerName = "Platforms";
        _sr.sortingOrder = 10;

        _collider = gameObject.AddComponent<BoxCollider2D>();
        _collider.isTrigger = true;
        _collider.size = Vector2.one * 1.5f;

        var rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        var respawn = FindFirstObjectByType<PlayerRespawn>();
        if (respawn != null)
        {
            _playerRespawn = respawn;
            _playerTransform = respawn.transform;
            _playerRespawn.OnRespawn += HandleRespawn;
        }
    }

    private void Update()
    {
        if (IsGiven) return;

        if (!IsCollected)
        {
            bool visible = _micInput != null && _micInput.Zone == Zone;
            _sr.enabled = visible;
            _collider.enabled = visible;
        }
    }

    private void LateUpdate()
    {
        if (!IsCollected || IsGiven || _playerTransform == null) return;

        Vector3 target = _playerTransform.position + new Vector3(-_trailSpacing * (_trailIndex + 1), _trailYOffset, 0f);
        transform.position = Vector3.Lerp(transform.position, target, _followSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsCollected || IsGiven) return;
        if (!IsPlayer(other)) return;

        Collect();
    }

    public void Collect()
    {
        IsCollected = true;
        _collider.enabled = false;
        _sr.enabled = true;
    }

    public void SetTrailIndex(int index)
    {
        _trailIndex = index;
    }

    public void Give()
    {
        IsGiven = true;
        IsCollected = false;
        _sr.enabled = false;
        _collider.enabled = false;
        gameObject.SetActive(false);
    }

    private void HandleRespawn()
    {
        if (IsGiven) return;

        IsCollected = false;
        _collider.enabled = true;
        transform.position = _originalPosition;
    }

    private void OnDestroy()
    {
        if (_playerRespawn != null)
            _playerRespawn.OnRespawn -= HandleRespawn;
    }

    private static Sprite MakeWhiteSprite(Sprite src)
    {
        int w = (int)src.rect.width;
        int h = (int)src.rect.height;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = src.texture.filterMode;

        try
        {
            var pixels = src.texture.GetPixels((int)src.rect.x, (int)src.rect.y, w, h);
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color(1f, 1f, 1f, pixels[i].a);
            tex.SetPixels(pixels);
        }
        catch
        {
            var white = new Color[w * h];
            for (int i = 0; i < white.Length; i++)
                white[i] = Color.white;
            tex.SetPixels(white);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), src.pixelsPerUnit);
    }

    private static bool IsPlayer(Collider2D col)
    {
        return col.gameObject.layer == 6
            || col.GetComponent<PlayerRespawn>() != null
            || col.GetComponentInParent<PlayerRespawn>() != null;
    }
}
