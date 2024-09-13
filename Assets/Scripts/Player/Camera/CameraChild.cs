using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraChild : MonoBehaviour
{
    [SerializeField] private Camera _parentCamera;
    
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        _camera.fieldOfView = _parentCamera.fieldOfView;
    }
}
