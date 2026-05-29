using UnityEngine;

public class LightsPullable : MonoBehaviour, IInteractable
{
    public enum LightsState { Hanging, Grabbed, TornDown, TangledBall }

    [SerializeField] private PickupObject _looseEnd;
    [SerializeField] private Rigidbody _lightsRigidbody;
    [SerializeField] private GameObject _tangledVisual;
    [SerializeField] private float _pullSpeedThreshold = 4f;
    [SerializeField] private float _tearThreshold = 1.5f;

    private LightsState _state = LightsState.Hanging;
    private float _tensionAccumulator;

    private void OnEnable()
    {
        _looseEnd.OnPickedUp += HandlePickedUp;
        _looseEnd.OnDropped += HandleDropped;
    }

    private void OnDisable()
    {
        _looseEnd.OnPickedUp -= HandlePickedUp;
        _looseEnd.OnDropped -= HandleDropped;
    }

    private void HandlePickedUp()
    {
        if (_state != LightsState.Hanging) return;
        _state = LightsState.Grabbed;
        _tensionAccumulator = 0f;
    }

    private void HandleDropped()
    {
        if (_state == LightsState.Grabbed)
            _state = LightsState.Hanging;
    }

    private void Update()
    {
        if (_state == LightsState.Grabbed)
            UpdateTension();
    }

    private void UpdateTension()
    {
        // Walk up: looseEnd → holdPoint → player → Rigidbody
        float speed = 0f;
        if (_looseEnd.transform.parent != null && _looseEnd.transform.parent.parent != null)
        {
            Rigidbody playerRb = _looseEnd.transform.parent.parent.GetComponent<Rigidbody>();
            if (playerRb != null) speed = playerRb.linearVelocity.magnitude;
        }

        if (speed >= _pullSpeedThreshold)
            _tensionAccumulator += Time.deltaTime;
        else
            _tensionAccumulator = Mathf.Max(0f, _tensionAccumulator - Time.deltaTime * 0.5f);

        if (_tensionAccumulator >= _tearThreshold)
            TearDown();
    }

    private void TearDown()
    {
        _state = LightsState.TornDown;

        // Directly detach loose end without calling PickupObject.OnDrop():
        // OnDrop() does NOT clear Controller's heldObject reference.
        // Controller will clear it naturally on the player's next interact press.
        Rigidbody looseRb = _looseEnd.GetComponent<Rigidbody>();
        if (looseRb != null) looseRb.isKinematic = false;
        Collider looseCol = _looseEnd.GetComponent<Collider>();
        if (looseCol != null) looseCol.enabled = true;
        _looseEnd.transform.SetParent(null);

        if (_lightsRigidbody != null) _lightsRigidbody.isKinematic = false;
        transform.SetParent(null);
    }

    // Called by Controller when player presses Interact near the fallen pile.
    // Requires: this GameObject is on the Pickup layer when in TornDown state.
    public void Interact()
    {
        if (_state != LightsState.TornDown) return;
        TangleIntoBall();
    }

    private void TangleIntoBall()
    {
        if (_tangledVisual == null)
        {
            Debug.LogError("[LightsPullable] _tangledVisual is not assigned.", this);
            return;
        }
        _state = LightsState.TangledBall;
        _tangledVisual.SetActive(true);
        gameObject.SetActive(false);
        BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.LightsOut);
    }
}
