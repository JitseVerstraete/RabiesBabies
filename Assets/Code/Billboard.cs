using System;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera _mainCamera;
    [SerializeField] private Vector2 _offset;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        // Make the GameObject face the camera
        FaceTheCamera();
    }

    private void FaceTheCamera()
    {
        if (_mainCamera == null) return;
        
        transform.position = transform.parent.position + new Vector3(_offset.x, 0.5f, _offset.y);
        transform.LookAt(-_mainCamera.transform.position * 10f, _mainCamera.transform.up);
        
    }
}