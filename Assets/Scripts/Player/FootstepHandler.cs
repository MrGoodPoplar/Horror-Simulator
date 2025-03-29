using System;
using System.Linq;
using Audio_System;
using JetBrains.Annotations;
using Surface_System;
using UnityEngine;

public class FootstepHandler : MonoBehaviour
{
    [Header("Constraints")]
    [SerializeField, RequireInterface(typeof(IMoveable))] private MonoBehaviour _moveableReference;

    [Header("Settings")]
    [SerializeField] private float _playRate = 0.2f;
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
        var surfaceImpactHandler = new SurfaceImpactHandler(surfaceData);

        if (surfaceData != null)
            surfaceImpactHandler.PlaySound(surfaceData?.surfaceImpactSound.landingImpactSounds);
    }

    private void HandleSfx()
    {
        if (_moveable.velocity > 0 && _moveable.isGrounded)
        {
            _currentPlayRate = _playRate / _moveable.velocity;

            if (_timer <= 0f)
            {
                _timer = _currentPlayRate;
                
                var surfaceData = GetSurfaceData();
                var surfaceImpactHandler = new SurfaceImpactHandler(surfaceData);
        
                if (surfaceData != null)
                    surfaceImpactHandler.PlaySound(surfaceData?.surfaceImpactSound.stepImpactSounds);
            }

            _timer -= Time.deltaTime;
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