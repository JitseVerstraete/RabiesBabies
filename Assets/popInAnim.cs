using System;
using System.Collections;
using UnityEngine;

public class popInAnim : MonoBehaviour
{
    [SerializeField] private bool _animateOnAwake = true;
    
    [SerializeField] private AnimationCurve _scaleCurve;
    [SerializeField] private float _duration;
    [SerializeField] private float _minScale;
    [SerializeField] private float _maxScale;

    private float _timer;

    private void Awake()
    {
        if (_animateOnAwake)
        {
            PopIn();
        }
    }

    public void PopIn()
    {
        StartCoroutine(PopInRoutine());
    }

    private IEnumerator PopInRoutine()
    {
        while (_timer < _duration)
        {
            _timer += Time.deltaTime;
            float currentScale = Mathf.Lerp(_minScale, _maxScale, _scaleCurve.Evaluate(_timer / _duration));
            transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return null;
        }

        _timer = 0;
    }
}
    