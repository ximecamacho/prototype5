using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerRespawn : MonoBehaviour
{
    [Tooltip("Same background Tilemap used for camera bounds")]
    [SerializeField] private Tilemap _boundsTilemap;

    [Tooltip("How far above the spawn point the player reappears")]
    [SerializeField] private float _respawnHeightOffset = 5f;

    private Vector3 _spawnPoint;
    private float _killY;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spawnPoint = transform.position;

        if (_boundsTilemap != null)
        {
            _boundsTilemap.CompressBounds();
            var local = _boundsTilemap.localBounds;
            var t = _boundsTilemap.transform;
            _killY = t.TransformPoint(local.min).y;
        }
    }

    private void Update()
    {
        if (transform.position.y < _killY)
            Respawn();
    }

    private void Respawn()
    {
        _rb.linearVelocity = Vector2.zero;
        transform.position = _spawnPoint + Vector3.up * _respawnHeightOffset;
    }
}
