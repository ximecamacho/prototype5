using UnityEngine;

public class MicrophoneInput : MonoBehaviour
{
    [Header("Microphone")]
    [Tooltip("Index into Microphone.devices. 0 = first/default mic.")]
    [SerializeField] private int _deviceIndex;

    [Header("Detection Mode")]
    [Tooltip("True = pitch-based (low hum vs high voice), False = volume-based (quiet vs loud)")]
    [SerializeField] private bool _usePitch = false;

    [Header("Pitch Range (Hz) — only used in Pitch mode")]
    [Tooltip("Lowest pitch to detect (e.g. deep hum)")]
    [SerializeField] private float _minPitch = 80f;
    [Tooltip("Highest pitch to detect (e.g. high whistle)")]
    [SerializeField] private float _maxPitch = 800f;

    [Header("Volume Sensitivity — only used in Volume mode")]
    [SerializeField, Range(1f, 200f)] private float _sensitivity = 45f;

    [Header("Shared Settings")]
    [Tooltip("Volume threshold below which input is ignored")]
    [SerializeField, Range(0f, 0.05f)] private float _noiseGate = 0.005f;
    [Tooltip("How fast the bar rises")]
    [SerializeField, Range(1f, 30f)] private float _riseSpeed = 10f;
    [Tooltip("How fast the bar falls")]
    [SerializeField, Range(1f, 30f)] private float _fallSpeed = 5f;
    [Tooltip("Number of frames to average over (1 = no smoothing, higher = smoother but laggier)")]
    [SerializeField, Range(1, 30)] private int _smoothing = 5;

    [Header("Zone Thresholds")]
    [Tooltip("Level above which we enter the Yellow zone")]
    [SerializeField, Range(0f, 1f)] private float _yellowThreshold = 0.33f;
    [Tooltip("Level above which we enter the Red zone")]
    [SerializeField, Range(0f, 1f)] private float _redThreshold = 0.66f;

    /// <summary>Normalized pitch level from 0 (low) to 1 (high). 0 when silent.</summary>
    public float Level { get; private set; }

    /// <summary>Current zone: 0 = Green (low/silent), 1 = Yellow (medium), 2 = Red (high).</summary>
    public int Zone { get; private set; }

    /// <summary>Detected pitch in Hz. 0 when silent.</summary>
    public float Pitch { get; private set; }

    /// <summary>When true, mic is ignored and comma/period keys control the level.</summary>
    public bool DebugMode { get; set; }

    public bool UsePitch
    {
        get => _usePitch;
        set => _usePitch = value;
    }

    public float Sensitivity
    {
        get => _sensitivity;
        set => _sensitivity = Mathf.Clamp(value, 1f, 200f);
    }

    public float NoiseGate
    {
        get => _noiseGate;
        set => _noiseGate = Mathf.Clamp(value, 0f, 0.05f);
    }

    public int Smoothing
    {
        get => _smoothing;
        set => _smoothing = Mathf.Clamp(value, 1, 30);
    }

    public float YellowThreshold => _yellowThreshold;
    public float RedThreshold => _redThreshold;

    private AudioClip _micClip;
    private string _micDevice;
    private float[] _sampleBuffer;
    private const int SampleRate = 44100;
    private const int SampleWindow = 2048;

    private float[] _smoothBuffer = new float[30];
    private int _smoothIndex;

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
        _micClip = Microphone.Start(_micDevice, true, 10, SampleRate);
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

            if (_usePitch)
            {
                float freq = DetectPitch();
                Pitch = freq;
                target = freq <= 0f ? 0f : Mathf.Clamp01(Mathf.InverseLerp(_minPitch, _maxPitch, freq));
            }
            else
            {
                float raw = GetRMSLevel();
                Pitch = 0f;
                float gated = raw < _noiseGate ? 0f : raw;
                target = Mathf.Clamp01(gated * _sensitivity);
            }
        }

        _smoothBuffer[_smoothIndex % _smoothBuffer.Length] = target;
        _smoothIndex++;
        int windowSize = Mathf.Clamp(_smoothing, 1, _smoothBuffer.Length);
        float sum = 0f;
        for (int i = 0; i < windowSize; i++)
        {
            int idx = (_smoothIndex - 1 - i + _smoothBuffer.Length * windowSize) % _smoothBuffer.Length;
            sum += _smoothBuffer[idx];
        }
        target = sum / windowSize;

        float speed = target > Level ? _riseSpeed : _fallSpeed;
        Level = Mathf.MoveTowards(Level, target, speed * Time.deltaTime);

        if (Level >= _redThreshold)
            Zone = 2;
        else if (Level >= _yellowThreshold)
            Zone = 1;
        else
            Zone = 0;
    }

    private float DetectPitch()
    {
        int micPos = Microphone.GetPosition(_micDevice) - SampleWindow + 1;
        if (micPos < 0) micPos += _micClip.samples;

        _micClip.GetData(_sampleBuffer, micPos);

        float rms = 0f;
        for (int i = 0; i < SampleWindow; i++)
            rms += _sampleBuffer[i] * _sampleBuffer[i];
        rms = Mathf.Sqrt(rms / SampleWindow);

        if (rms < _noiseGate) return 0f;

        int minLag = (int)(SampleRate / _maxPitch);
        int maxLag = (int)(SampleRate / _minPitch);
        maxLag = Mathf.Min(maxLag, SampleWindow / 2);

        float bestCorrelation = 0f;
        int bestLag = -1;

        for (int lag = minLag; lag <= maxLag; lag++)
        {
            float correlation = 0f;
            for (int i = 0; i < SampleWindow - lag; i++)
                correlation += _sampleBuffer[i] * _sampleBuffer[i + lag];

            if (correlation > bestCorrelation)
            {
                bestCorrelation = correlation;
                bestLag = lag;
            }
        }

        if (bestLag <= 0) return 0f;

        return (float)SampleRate / bestLag;
    }

    private float GetRMSLevel()
    {
        int micPos = Microphone.GetPosition(_micDevice) - SampleWindow + 1;
        if (micPos < 0) micPos += _micClip.samples;

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
