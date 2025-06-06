using System;
using JetBrains.Annotations;
using Surface_System;
using UnityEngine;

public class FootstepHandler : MonoBehaviour
{
    [Header("Constraints")]
    [SerializeField, RequireInterface(typeof(IMoveable))] private MonoBehaviour _moveableReference;

    [Header("Settings")]
    [SerializeField] private float _playRateModifier = 0.2f;
    [SerializeField] private Vector2 _threshold = new Vector2(0.1f, 0.6f);
    [SerializeField] private float _groundCheckDistance = 2f;

    private IMoveable _moveable => _moveableReference as IMoveable;
    private float _currentPlayRate;
    private float _timer;

    private void Start()
    {
        _moveable.OnLanded += OnLanded;
    }

    private void OnDestroy()
    {
        _moveable.OnLanded -= OnLanded;
    }

    private void Update()
    {
        HandleSfx();
    }
    
    private void OnLanded()
    {
        var surfaceData = GetSurfaceData();

        if (surfaceData != null)
            new SurfaceImpactHandler(surfaceData).PlaySound(surfaceData.surfaceImpactSound.landingImpactSounds);
    }

    private void HandleSfx()
    {
        if (_moveable.speedHorizontal > 0 && _moveable.isGrounded)
        {
            _currentPlayRate = Mathf.Clamp(_playRateModifier / _moveable.speedHorizontal, _threshold.x, _threshold.y);

            if (_timer <= 0f)
            {
                _timer = _currentPlayRate;
                
                var surfaceData = GetSurfaceData();
                
                if (surfaceData != null && !surfaceData.textureSound.ignoreStepSound)
                {
                    new SurfaceImpactHandler(surfaceData)
                        .PlaySound(surfaceData.surfaceImpactSound.stepImpactSounds);
                }
            }

            _timer -= Time.deltaTime;
        }
        else
        {
            _timer = 0;
        }
    }

    [CanBeNull]
    private SurfaceData GetSurfaceData()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit[] hits = Physics.RaycastAll(ray, _groundCheckDistance);

        if (hits.Length > 0)
        {
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            return Player.Instance.surfaceManager.GetImpactDetails(hits[0]);
        }

        return null;
    }
}