using UnityEngine;

public class MicrophoneInput : MonoBehaviour
{
    [Header("Microphone")]
    [Tooltip("Index into Microphone.devices. 0 = first/default mic.")]
    [SerializeField] private int _deviceIndex;

    [Header("Sensitivity")]
    [SerializeField, Range(1f, 200f)] private float _sensitivity = 50f;
    [SerializeField, Range(0f, 0.99f)] private float _smoothing = 0.3f;
    [Tooltip("Minimum raw level ignored as background noise")]
    [SerializeField, Range(0f, 0.05f)] private float _noiseGate = 0.005f;

    [Header("Zone Thresholds")]
    [Tooltip("Level above which we enter the Yellow zone")]
    [SerializeField, Range(0f, 1f)] private float _yellowThreshold = 0.33f;
    [Tooltip("Level above which we enter the Red zone")]
    [SerializeField, Range(0f, 1f)] private float _redThreshold = 0.66f;

    /// <summary>Normalized audio level from 0 to 1.</summary>
    public float Level { get; private set; }

    /// <summary>Current zone: 0 = Green (quiet), 1 = Yellow (medium), 2 = Red (loud).</summary>
    public int Zone { get; private set; }

    /// <summary>When true, mic is ignored and comma/period keys control the level.</summary>
    public bool DebugMode { get; set; }

    public float YellowThreshold => _yellowThreshold;
    public float RedThreshold => _redThreshold;

    private AudioClip _micClip;
    private string _micDevice;
    private float[] _sampleBuffer;
    private const int SampleRate = 44100;
    private const int SampleWindow = 256;

    private void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("MicrophoneInput: No microphone detected!");
            enabled = false;
            return;
        }

        for (int i = 0; i < Microphone.devices.Length; i++)
            Debug.Log($"MicrophoneInput: [{i}] {Microphone.devices[i]}");

        _deviceIndex = Mathf.Clamp(_deviceIndex, 0, Microphone.devices.Length - 1);
        _micDevice = Microphone.devices[_deviceIndex];
        _micClip = Microphone.Start(_micDevice, true, 1, SampleRate);
        _sampleBuffer = new float[SampleWindow];

        Debug.Log($"MicrophoneInput: Using [{_deviceIndex}] \"{_micDevice}\"");
    }

    private void Update()
    {
        float target;

        if (DebugMode)
        {
            if (Input.GetKey(KeyCode.Period))
                target = (_redThreshold + 1f) / 2f;
            else if (Input.GetKey(KeyCode.Comma))
                target = (_yellowThreshold + _redThreshold) / 2f;
            else
                target = 0f;
        }
        else
        {
            if (_micClip == null) return;

            float raw = GetRMSLevel();
            float gated = raw < _noiseGate ? 0f : raw;
            target = Mathf.Clamp01(gated * _sensitivity);
        }

        Level = Mathf.Lerp(target, Level, _smoothing);

        if (Level >= _redThreshold)
            Zone = 2;
        else if (Level >= _yellowThreshold)
            Zone = 1;
        else
            Zone = 0;
    }

    private float GetRMSLevel()
    {
        int micPos = Microphone.GetPosition(_micDevice) - SampleWindow + 1;
        if (micPos < 0) return 0f;

        _micClip.GetData(_sampleBuffer, micPos);

        float sum = 0f;
        for (int i = 0; i < SampleWindow; i++)
            sum += _sampleBuffer[i] * _sampleBuffer[i];

        return Mathf.Sqrt(sum / SampleWindow);
    }

    private void OnDestroy()
    {
        if (_micDevice != null && Microphone.IsRecording(_micDevice))
            Microphone.End(_micDevice);
    }
}
