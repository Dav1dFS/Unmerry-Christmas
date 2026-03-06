using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{

    private Vector3 _offset;
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothTime;
    private Vector3 _velocity = Vector3.zero;

    private void Awake()
    {
        _offset = transform.position - _target.position;
    }

    private void LateUpdate()
    {
        if (_offset.x + _offset.y + _offset.z > 1)
        {
            Vector3 targetPosition = _target.position + _offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);
        } 
    }

}
