using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _turnSpeed = 360f;
    [SerializeField] private Rigidbody _rb;
    private Vector3 _input;
    private float jumpForce = 5f;
    private bool jump = false;
    private bool isGrounded = true;

    void Update()
    {
        if (_rb.transform.position.y <= 0)
        {
            isGrounded = true;
        }
        GatherInput();
        Look();
    }

    void FixedUpdate()
    {
        Move();
    }

    void GatherInput()
    {
        _input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetKeyDown("space"))
        {
            jump = true;
        }
    }

    void Look()
    {
        if (_input != Vector3.zero)
        {

            var relative = (transform.position + _input.ToIso()) - transform.position;
            var rot= Quaternion.LookRotation(relative, Vector3.up);

            transform.rotation=Quaternion.RotateTowards(transform.rotation, rot, _turnSpeed * Time.deltaTime);
        }
    }

    void Move()
    {
        _rb.MovePosition(transform.position + (transform.forward * _input.magnitude * _speed * Time.fixedDeltaTime));
        if (jump && isGrounded)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jump = false;
            isGrounded = false;
        }
    }

    
}
