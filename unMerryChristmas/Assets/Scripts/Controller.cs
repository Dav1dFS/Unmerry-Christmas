using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 10f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 15f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private InputAction _moveLeft;
    [SerializeField] private InputAction _moveRight;
    [SerializeField] private InputAction _moveUp;
    [SerializeField] private InputAction _moveDown;
    [SerializeField] private InputAction _sprint;

    private Vector3 _input;
    private float _currentSpeed;
    private int _lockedH = 0;
    private int _lockedV = 0;

    private void OnEnable()
    {
        _moveLeft.Enable();
        _moveRight.Enable();
        _moveUp.Enable();
        _moveDown.Enable();
        _sprint.Enable();

        _moveLeft.started  += _ => { if (_lockedH == 0) _lockedH = -1; };
        _moveLeft.canceled += _ => { if (_lockedH == -1) _lockedH = _moveRight.IsPressed() ? 1 : 0; };

        _moveRight.started  += _ => { if (_lockedH == 0) _lockedH = 1; };
        _moveRight.canceled += _ => { if (_lockedH == 1) _lockedH = _moveLeft.IsPressed() ? -1 : 0; };

        _moveUp.started  += _ => { if (_lockedV == 0) _lockedV = 1; };
        _moveUp.canceled += _ => { if (_lockedV == 1) _lockedV = _moveDown.IsPressed() ? -1 : 0; };

        _moveDown.started  += _ => { if (_lockedV == 0) _lockedV = -1; };
        _moveDown.canceled += _ => { if (_lockedV == -1) _lockedV = _moveUp.IsPressed() ? 1 : 0; };
    }

    private void OnDisable()
    {
        _moveLeft.Disable();
        _moveRight.Disable();
        _moveUp.Disable();
        _moveDown.Disable();
        _sprint.Disable();
    }

    void Update()
    {
        GatherInput();
        UpdateSpeed();
        Look();
    }

    void FixedUpdate()
    {
        Move();
    }

    void GatherInput()
    {
        _input = new Vector3(_lockedH, 0, _lockedV);
    }

    void UpdateSpeed()
    {
        float targetSpeed = _input == Vector3.zero ? 0f : (_sprint.IsPressed() ? _sprintSpeed : _walkSpeed);
        float rate = _input != Vector3.zero ? _acceleration : _deceleration;
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, rate * Time.deltaTime);
    }

    void Look()
    {
        if (_input != Vector3.zero)
        {
            var targetRot = Quaternion.LookRotation(_input.ToIso(), Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
        }
    }

    void Move()
    {
        _rb.MovePosition(transform.position + transform.forward * _currentSpeed * Time.fixedDeltaTime);
    }
}
