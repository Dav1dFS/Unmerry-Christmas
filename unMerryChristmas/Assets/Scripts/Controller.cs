using UnityEngine;
using UnityEngine.InputSystem;

public partial class Controller : MonoBehaviour
{
    // ── References ───────────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform holdPoint;

    // ── Layers ───────────────────────────────────────────────────────────────
    [Header("Layers")]
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private LayerMask pushableLayer;

    // ── Input actions ────────────────────────────────────────────────────────
    [Header("Input")]
    [SerializeField] private InputAction _moveLeft;
    [SerializeField] private InputAction _moveRight;
    [SerializeField] private InputAction _moveUp;
    [SerializeField] private InputAction _moveDown;
    [SerializeField] private InputAction _sprint;
    [SerializeField] private InputAction _jump;
    [SerializeField] private InputAction _interact;
    [SerializeField] private InputAction _roll;
    [SerializeField] private InputAction _spawnGift;
    [SerializeField] private InputAction _dropGift;

    // ── Shared input state (used by Movement) ────────────────────────────────
    private Vector3 _input;
    private float   _currentSpeed;
    private int     _lockedH = 0;
    private int     _lockedV = 0;

    // ── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Freeze physics rotation — we drive rotation through transform directly.
        _rb.freezeRotation = true;

        // Zero friction on every collider so PhysX static friction cannot pin
        // the player against vertical walls and prevent gravity from pulling them down.
        var frictionless = new PhysicsMaterial("PlayerFrictionless")
        {
            dynamicFriction = 0f,
            staticFriction  = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounciness      = 0f,
            bounceCombine   = PhysicsMaterialCombine.Minimum,
        };
        foreach (var col in GetComponentsInChildren<Collider>())
            col.sharedMaterial = frictionless;
    }

    private void OnEnable()
    {
        _moveLeft.Enable();   _moveRight.Enable();
        _moveUp.Enable();     _moveDown.Enable();
        _sprint.Enable();     _jump.Enable();
        _interact.Enable();   _roll.Enable();
        _spawnGift.Enable();  _dropGift.Enable();

        // Action bindings (Only tracking discrete event triggers)
        _jump.started      += _ => { if (isGrounded) jump = true; };
        _interact.started  += _ => StartInteractHold();
        _interact.canceled += _ => ReleaseInteractHold();
        _roll.started      += _ => TryStartRoll();
        _spawnGift.started += _ => TrySpawnGift();
        _dropGift.started  += _ => DropGift();
    }

    private void OnDisable()
    {
        _moveLeft.Disable();   _moveRight.Disable();
        _moveUp.Disable();     _moveDown.Disable();
        _sprint.Disable();     _jump.Disable();
        _interact.Disable();   _roll.Disable();
        _spawnGift.Disable();  _dropGift.Disable();
    }

    // ── Update / FixedUpdate dispatch ────────────────────────────────────────

    private void Update()
    {
        UpdateGroundDetection(); // Jump
        UpdateRoll();            // Roll
        UpdateInteraction();     // Interaction (hold-to-aim, trajectory, push constraints)
        GatherInput();           // Movement
        UpdateSpeed();           // Movement
        Look();                  // Movement
        UpdateContextHint();     // ContextHint
    }

    private void FixedUpdate() => Move(); // Movement

    // ── Shared utility ───────────────────────────────────────────────────────

    /// <summary>Clears in-progress ability state when the player grabs a pushable.</summary>
    private void ClearAbilityInputs()
    {
        jump     = false;
        isAiming = false;
        holdTime = 0f;
        StopChargeSound();
    }
}