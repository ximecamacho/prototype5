using UnityEngine;
using UnityEngine.UI;

public class AudioLevelBar : MonoBehaviour
{
    [SerializeField] private MicrophoneInput _micInput;

    [Header("Fill Overlay (single filled image on top of colored background)")]
    [SerializeField] private Image _fillOverlay;

    private void Update()
    {
        if (_micInput == null) return;
        _fillOverlay.fillAmount = _micInput.Level;
    }
}
