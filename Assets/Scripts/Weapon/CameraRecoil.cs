using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraRecoil : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _snappiness;
    [SerializeField] private float _returnSpeed;
    [SerializeField, Range(0, 1)] private float _aimRecoilReducer = 0.7f;
    
    private Vector3 _currentRotation;
    private Vector3 _targetRotation;
    private ShooterController _shooterController;

    private void Start()
    {
        _shooterController = ShooterController.instance;
    }

    private void Update()
    {
        _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, _returnSpeed * Time.deltaTime);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, _snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(_currentRotation);
    }

    public void RecoilFire(Vector3 recoil)
    {
        if (_shooterController.isAiming)
            recoil *= _aimRecoilReducer;
        
        _targetRotation += new Vector3(recoil.x, Random.Range(-recoil.y, recoil.y), Random.Range(-recoil.z, recoil.z));
    }
    
}
