using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField] private float _recoilForceMultiplier = 1f;
    [SerializeField, Range(0, 1)] private float _aimRecoilReducer = 0.7f;
    [SerializeField, Range(0, 5)] private float _recoilSpeed = 0.5f;
    
    private float _recoilForce = 0;

    private Vector3 _currentRotation;
    private Vector3 _targetRotation;
    private ShooterController _shooterController;
    private FirstPersonController _firstPersonController;

    private void Start()
    {
        _shooterController = Player.Instance.shooterController;
    }

    private void Update()
    {
        _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, _recoilSpeed * Time.deltaTime);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, _recoilForce * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(_currentRotation);
    }

    public void RecoilFire(Vector3 recoil, float recoilForce, float recoilSpeed)
    {
        if (_shooterController.isAiming) {}
            recoil *= _aimRecoilReducer;

        _recoilForce = (_shooterController.isAiming ? recoilSpeed * _aimRecoilReducer : recoilForce) * _recoilForceMultiplier;
        _targetRotation += new Vector3(recoil.x, Random.Range(-recoil.y, recoil.y), Random.Range(-recoil.z, recoil.z));
    }
}
