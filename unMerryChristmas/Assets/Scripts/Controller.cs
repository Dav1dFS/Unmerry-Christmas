using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 10f;
    [SerializeField] private float _turnSpeed = 360f;
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private InputAction _moveAction;
    [SerializeField] private InputAction _sprintAction;

    private Vector3 _input;
    private float _currentSpeed;

    private void OnEnable()
    {
        _moveAction.Enable();
        _sprintAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _sprintAction.Disable();
    }

    void Update()
    {
        GatherInput();
        Look();
    }

    void FixedUpdate()
    {
        Move();
    }

    void GatherInput()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        _input = new Vector3(moveInput.x, 0, moveInput.y);
        _currentSpeed = _sprintAction.IsPressed() ? _sprintSpeed : _walkSpeed;
    }

    void Look()
    {
        if (_input != Vector3.zero)
        {
            var relative = (transform.position + _input.ToIso()) - transform.position;
            var rot = Quaternion.LookRotation(relative, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, _turnSpeed * Time.deltaTime);
        }
    }

    void Move()
    {
        _rb.MovePosition(transform.position + (transform.forward * _input.magnitude * _currentSpeed * Time.fixedDeltaTime));
    }
}
