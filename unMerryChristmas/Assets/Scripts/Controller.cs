using UnityEngine;
using UnityEngine.InputSystem;

public partial class Controller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform holdPoint;

    [Header("Layers")]
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private LayerMask pushableLayer;

    [Header("Input")]
    // Optional explicit asset reference. When left null, the project-wide actions
    // asset (InputSystem.actions == InputSystem_Actions) is used — the SAME asset the
    // settings rebind menu edits and AccessibilityManager loads overrides into, so
    // rebinds take effect in-game and persist.
    [SerializeField] private InputActionAsset _inputActions;

    // Gift actions have no equivalent in InputSystem_Actions, so they stay inline
    // (self-contained, not rebindable through the settings menu).
    [SerializeField] private InputAction _spawnGift;
    [SerializeField] private InputAction _dropGift;

    // Resolved from the shared asset in Awake (see ResolveActions).
    private InputAction _move, _sprint, _jump, _interact, _roll, _toggleCamera, _hint;

    private Vector3 _input;
    private float   _currentSpeed;
    private int     _lockedH = 0;
    private int     _lockedV = 0;

    private void Awake()
    {
        ResolveActions();

        _rb.freezeRotation = true;

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
        _move?.Enable();      _sprint?.Enable();       _jump?.Enable();
        _interact?.Enable();  _roll?.Enable();         _toggleCamera?.Enable();
        _hint?.Enable();
        _spawnGift.Enable();  _dropGift.Enable();

        // Named handlers (not lambdas) so they can be unsubscribed below — these
        // actions are shared and persist across scene loads, so leaked subscriptions
        // would fire input callbacks multiple times.
        if (_jump != null)         _jump.started += OnJumpStarted;
        if (_interact != null)   { _interact.started += OnInteractStarted;
                                   _interact.canceled += OnInteractCanceled; }
        if (_roll != null)         _roll.started += OnRollStarted;
        if (_toggleCamera != null) _toggleCamera.started += OnToggleCameraStarted;
        _spawnGift.started += OnSpawnGiftStarted;
        _dropGift.started  += OnDropGiftStarted;

        AccessibilityManager.OnHighlightChanged += OnHighlightSettingChanged;
    }

    private void OnDisable()
    {
        if (_jump != null)         _jump.started -= OnJumpStarted;
        if (_interact != null)   { _interact.started -= OnInteractStarted;
                                   _interact.canceled -= OnInteractCanceled; }
        if (_roll != null)         _roll.started -= OnRollStarted;
        if (_toggleCamera != null) _toggleCamera.started -= OnToggleCameraStarted;
        _spawnGift.started -= OnSpawnGiftStarted;
        _dropGift.started  -= OnDropGiftStarted;

        _move?.Disable();      _sprint?.Disable();      _jump?.Disable();
        _interact?.Disable();  _roll?.Disable();        _toggleCamera?.Disable();
        _hint?.Disable();
        _spawnGift.Disable();  _dropGift.Disable();

        AccessibilityManager.OnHighlightChanged -= OnHighlightSettingChanged;
    }

    // Resolve the rebindable actions from the shared (project-wide) asset so the
    // settings menu's rebinds + saved overrides drive gameplay. _spawnGift/_dropGift
    // are deliberately excluded — they have no equivalent in the asset.
    private void ResolveActions()
    {
        var asset = _inputActions != null ? _inputActions : InputSystem.actions;
        var map   = asset != null ? asset.FindActionMap("Player", false) : null;
        if (map == null)
        {
            Debug.LogError("[Controller] Could not resolve the 'Player' action map. " +
                           "Assign _inputActions, or set the project-wide Input Actions.", this);
            return;
        }
        _move         = map.FindAction("Move");
        _sprint       = map.FindAction("Sprint");
        _jump         = map.FindAction("Jump");
        _interact     = map.FindAction("Interact");
        _roll         = map.FindAction("Roll");
        _toggleCamera = map.FindAction("Toggle Camera");
        _hint         = map.FindAction("Hint");
    }

    // ── Input callback handlers (named so OnDisable can unsubscribe them) ───────
    private void OnJumpStarted(InputAction.CallbackContext _)         { if (isGrounded) jump = true; }
    private void OnInteractStarted(InputAction.CallbackContext _)     => StartInteractHold();
    private void OnInteractCanceled(InputAction.CallbackContext _)    => ReleaseInteractHold();
    private void OnRollStarted(InputAction.CallbackContext _)         => TryStartRoll();
    private void OnToggleCameraStarted(InputAction.CallbackContext _) => ToggleCameraOffset();
    private void OnSpawnGiftStarted(InputAction.CallbackContext _)    => TrySpawnGift();
    private void OnDropGiftStarted(InputAction.CallbackContext _)     => DropGift();

    private void Update()
    {
        UpdateGroundDetection();
        UpdateRoll();            
        UpdateInteraction();     
        GatherInput();           
        UpdateSpeed();           
        Look();                  
        UpdateContextHint();     
    }

    private void FixedUpdate() => Move();

    private void ClearAbilityInputs()
    {
        jump     = false;
        isAiming = false;
        holdTime = 0f;
        StopChargeSound();
    }
}