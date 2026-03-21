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
    [SerializeField] private InputAction _jump;
    [SerializeField] private InputAction _interact;
    [SerializeField] private InputAction _throw;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private Transform holdPoint;

    private Vector3 _input;
    private float _currentSpeed;
    private int _lockedH = 0;
    private int _lockedV = 0;
    private float jumpForce = 8f;
    private bool jump = false;
    private bool isGrounded = true;

    private float pickupRange = 1.5f;
    

    private PickupObject heldObject;

    private void OnEnable()
    {
        _moveLeft.Enable();
        _moveRight.Enable();
        _moveUp.Enable();
        _moveDown.Enable();
        _sprint.Enable();
        _jump.Enable();
        _interact.Enable();
        _throw.Enable();

        _jump.started += _ => jump = true;
        _interact.started += _ => checkHands();
        _throw.started += _ => throwObject();

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
        _jump.Disable();
        _interact.Disable();
        _throw.Disable();
    }

    void throwObject() 
    {
        if (heldObject != null)
        {
            
            Rigidbody objectRb = heldObject.GetComponent<Rigidbody>();
            if (objectRb != null)
            {
                DropObject();
                Vector3 throwDirection = transform.forward + Vector3.up * 0.5f;
                objectRb.AddForce(throwDirection.normalized * 10f, ForceMode.Impulse); 
                
            }
        }
    }

    void checkHands()
    {
        if (heldObject != null)
        {
            DropObject();
        }
        else
        {
            //In the future, insert here the logic to check the closest object layer to decide what kind of interaction do to
            TryPickup();
        }
    }

    void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);

        float closestDistance = Mathf.Infinity;
        PickupObject closestObject = null;
        Collider closestCollectable = null;

        foreach (Collider hit in hits)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance >= closestDistance) continue;

            if (hit.CompareTag("Collectable"))
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCollectable = hit;
                    closestObject = null; // Prioritize collectables over pickup objects
                }
            }
            else
            {
                PickupObject pickup = hit.GetComponent<PickupObject>();
                if (pickup != null)
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestObject = pickup;
                    }
                }
            }
        }
        if (closestCollectable != null)
        {
            CollectableManager.Instance.Collect();
            Destroy(closestCollectable.gameObject);
        }
        else if (closestObject != null)
        {
            heldObject = closestObject;
            heldObject.OnPickup(holdPoint);
        }
    }

    void DropObject()
    {
        heldObject.OnDrop();
        heldObject = null;
    }

    void Update()
    {
        //check if rb is on the ground and set isGrounded to true
        if (Physics.Raycast(transform.position, Vector3.down, 1.1f))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
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
        _rb.MovePosition(transform.position + (transform.forward * (_input != Vector3.zero ? 1f : 0f) * _currentSpeed * Time.fixedDeltaTime));
        if (jump && isGrounded)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jump = false;
            isGrounded = false;
        }
    }
}
