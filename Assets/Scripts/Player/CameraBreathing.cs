using UnityEngine;

public class CameraBreathing : MonoBehaviour
{
    [Header("Breathing Settings")]
    [SerializeField] private Vector2 _amplitude = new (1.0f, 0.5f);
    [SerializeField, Range(0, 5)] private float _frequency = 1.0f;

    private Vector3 _initialRotation;

    private void Start()
    {
        _initialRotation = transform.localEulerAngles;
    }

    private void Update()
    {
        Vector3 breathingEffect = GetBreathingEffect();
        transform.localRotation = Quaternion.Euler(_initialRotation + breathingEffect);
    }

    private Vector3 GetBreathingEffect()
    {
        float breathingX = Mathf.Sin(Time.time * _frequency) * _amplitude.x;
        float breathingY = Mathf.Cos(Time.time * _frequency) * _amplitude.y;
        return new Vector3(breathingX, breathingY, 0);
    }
}