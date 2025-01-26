using UnityEngine;

public class rotate : MonoBehaviour
{
    [SerializeField] private float _speed = 30f;
    
    void Update()
    {
        transform.Rotate(0f, _speed * Time.deltaTime, 0f);
    }
}
