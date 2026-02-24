using UnityEngine;

public class MapSwitcher : MonoBehaviour
{
    [SerializeField] private MicrophoneInput _micInput;

    [Header("Platform Tilemaps (one per zone)")]
    [SerializeField] private GameObject _greenPlatforms;
    [SerializeField] private GameObject _yellowPlatforms;
    [SerializeField] private GameObject _redPlatforms;

    private int _lastZone = -1;
    private GameObject[] _zonePlatforms;

    private void Awake()
    {
        _zonePlatforms = new[] { _greenPlatforms, _yellowPlatforms, _redPlatforms };
    }

    private void Update()
    {
        if (_micInput == null) return;

        int zone = _micInput.Zone;
        if (zone == _lastZone) return;

        _lastZone = zone;

        for (int i = 0; i < _zonePlatforms.Length; i++)
        {
            if (_zonePlatforms[i] != null)
                _zonePlatforms[i].SetActive(i == zone);
        }
    }
}
