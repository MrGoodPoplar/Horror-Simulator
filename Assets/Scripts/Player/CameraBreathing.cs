using System;
using UnityEngine;

public class CameraBreathing : MonoBehaviour
{
    [Header("Breathing Settings")]
    [SerializeField] private Vector2 _amplitude = new(1.0f, 0.5f);
    [SerializeField, Range(0, 5)] private float _speed = 1.0f;

    [Header("Dyspneic Settings")]
    [SerializeField] private float _dyspneicCooldown = 15f;
    [SerializeField] private float _dyspneicSpeed = 3f;
    [SerializeField] private Vector2 _dyspneicAmplitude = new(2.0f, 1.0f);
    [SerializeField] private float _dyspneicDuration = 3f;

    [Header("Transition Settings")]
    [SerializeField] private float _transitionDuration = 0.5f;

    private Vector2 _currentAmplitude;
    private Vector3 _initialRotation;
    private float _lastDyspneicTime;
    private float _currentSpeed;
    private float _currentPhase;
    private bool _isDyspneic;

    private float _transitionTimer;
    private float _transitionStartSpeed;
    private Vector2 _transitionStartAmplitude;
    private float _transitionProgress;

    private FirstPersonController _firstPersonController;

    private void Awake()
    {
        _currentAmplitude = _amplitude;
        _currentSpeed = _speed;
    }

    private void Start()
    {
        _initialRotation = transform.localEulerAngles;
        _firstPersonController = FirstPersonController.instance;
        _firstPersonController.OnExhausted += OnExhaustedPerformed;
    }

    private void OnDestroy()
    {
        _firstPersonController.OnExhausted -= OnExhaustedPerformed;
    }

    private void Update()
    {
        float targetSpeed = _isDyspneic ? _dyspneicSpeed : _speed;
        Vector2 targetAmplitude = _isDyspneic ? _dyspneicAmplitude : _amplitude;

        if (_transitionTimer < _transitionDuration)
        {
            _transitionTimer += Time.deltaTime;
            _transitionProgress = Mathf.Clamp01(_transitionTimer / _transitionDuration); // Calculate progress (0 to 1)
        }

        _currentSpeed = Mathf.Lerp(_transitionStartSpeed, targetSpeed, _transitionProgress);
        _currentAmplitude = Vector2.Lerp(_transitionStartAmplitude, targetAmplitude, _transitionProgress);
        _currentPhase += Time.deltaTime * _currentSpeed;

        Vector3 breathingEffect = GetBreathingEffect(_currentPhase);
        transform.localRotation = Quaternion.Euler(_initialRotation + breathingEffect);

        if (_isDyspneic && Time.time > _lastDyspneicTime + _dyspneicDuration)
        {
            _isDyspneic = false;
            StartTransition();
        }
    }

    private Vector3 GetBreathingEffect(float phase)
    {
        float breathingX = Mathf.Sin(phase) * _currentAmplitude.x;
        float breathingY = Mathf.Cos(phase) * _currentAmplitude.y;
        return new Vector3(breathingX, breathingY, 0);
    }

    private void OnExhaustedPerformed()
    {
        if (_lastDyspneicTime == 0 || Time.time > _lastDyspneicTime + _dyspneicCooldown)
        {
            _lastDyspneicTime = Time.time;
            _isDyspneic = true;
            
            StartTransition();
        }
    }

    private void StartTransition()
    {
        _transitionTimer = 0f;
        _transitionStartSpeed = _currentSpeed;
        _transitionStartAmplitude = _currentAmplitude;
    }
}
