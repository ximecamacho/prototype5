using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothSpeed = 5f;
    [SerializeField] private Vector3 _offset = new(0, 1, -10);

    [Tooltip("Drag your background Tilemap here")]
    [SerializeField] private Tilemap _boundsTilemap;

    private Camera _cam;
    private Bounds _mapBounds;

    private void Awake()
    {
        _cam = GetComponent<Camera>();

        if (_boundsTilemap != null)
        {
            _boundsTilemap.CompressBounds();
            var local = _boundsTilemap.localBounds;
            var t = _boundsTilemap.transform;
            var worldMin = t.TransformPoint(local.min);
            var worldMax = t.TransformPoint(local.max);
            _mapBounds = new Bounds((worldMin + worldMax) / 2f, worldMax - worldMin);
        }
    }

    private void LateUpdate()
    {
        if (_target == null) return;
        var desired = _target.position + _offset;
        var pos = Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime);

        if (_boundsTilemap != null)
        {
            float halfHeight = _cam.orthographicSize;
            float halfWidth = halfHeight * _cam.aspect;

            float minX = _mapBounds.min.x + halfWidth;
            float maxX = _mapBounds.max.x - halfWidth;
            float minY = _mapBounds.min.y + halfHeight;
            float maxY = _mapBounds.max.y - halfHeight;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }

        transform.position = pos;
    }
}
