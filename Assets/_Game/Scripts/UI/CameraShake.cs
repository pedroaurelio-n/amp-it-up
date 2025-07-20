using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    
    [SerializeField] float shakeDuration = 0.5f;
    [SerializeField] float shakeMagnitude = 0.3f;
    [SerializeField] float dampingSpeed = 1.0f;

    Vector3 _initialPosition;
    float _currentShakeDuration;
    float _shakeMagnitude;
    
    Camera _camera;

    void Awake ()
    {
        Instance = this;
    }

    void Start()
    {
        _camera = Camera.main;
        _initialPosition = _camera.transform.localPosition;
    }

    void Update()
    {
        if (_currentShakeDuration > 0)
        {
            _camera.transform.localPosition = _initialPosition + Random.insideUnitSphere * _shakeMagnitude;
            _currentShakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            _currentShakeDuration = 0f;
            _camera.transform.localPosition = _initialPosition;
        }
    }
    
    public void TriggerShake(float duration = -1f, float magnitude = -1f)
    {
        _currentShakeDuration = duration > 0 ? duration : shakeDuration;
        _shakeMagnitude = magnitude > 0 ? magnitude : shakeMagnitude;
    }
}
