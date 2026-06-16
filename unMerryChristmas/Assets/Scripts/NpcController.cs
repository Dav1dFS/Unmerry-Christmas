using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NpcController : MonoBehaviour
{
    [Header("Key Items")]
    public List<Transform> keyItems = new();
    [SerializeField] private float _busyDuration = 10f; // seconds on each item
    [SerializeField] private float _busyDurationVariance = 5f;

    [Header("Detection")]
    public float DetectionAngle = 30f;
    public float DetectionRange = 6f;
    public float DetectionMinHeight = 0.2f; // minimum height difference to detect player (to prevent seeing through windows)
    [SerializeField] private float _detectionBuildUpTime = 2f; // seconds needed to detect player
    [SerializeField] private float _gracePeriod = 4f;
    [SerializeField] private float _alertedDetectionBuildUpTime = 1f; // when alerted, detect faster
    [SerializeField] private float _alertDuration = 6f;

    [SerializeField] private float _watchDuration = 3f;
    [SerializeField] private float _lookAroundSpeed = 45f; // degrees per second while looking around
    [SerializeField] private int _lookAroundTurns = 3; // number of times to look around when alerted
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _obstacleMask; // walls/obstacles that block vision

    public float DetectionProgress => _detectionBuildUpTime > 0f ?
    Mathf.Clamp01(_detectionTimer / _detectionBuildUpTime) : 0f;

    [Header("Hit Stop")]
    [SerializeField] private float _hitStopDuration = 0.1f;  // ajustável no Inspector

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _alertedWalkSpeed = 1f; // slower when alerted
    [SerializeField] private float _arrivedThreshold = 1.5f;

    [Header("Distraction")]
    [SerializeField] private float _distractedDuration = 5f;

    [Header("References")]
    [SerializeField] private Transform _head; // head position for raycasting (can be null, then uses body)
    [SerializeField] private Transform _player; // player reference (can be set in inspector or auto-found)

    public NpcStates CurrentState { get; private set; } = NpcStates.Busy;

    private NavMeshAgent _agent;
    private int _currentKeyItemIndex = 0;
    private float _stateTimer = 0f;
    private float _detectionTimer = 0f;
    private float _gracePeriodTimer = 0f;
    private float CurrentDetectionBuildUpTime =>
    CurrentState == NpcStates.Alerted ? _alertedDetectionBuildUpTime : _detectionBuildUpTime;

    private float _watchCooldown = 3f;
    private float _watchCooldownTimer = 0f;
    private float _watchTimer = 0f;
    private int _lookTurnsLeft = 0;
    private Coroutine _lookAroundCoroutine;

    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _walkSpeed;
        _agent.stoppingDistance = _arrivedThreshold;

        if (_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;
        }
    }

    protected virtual void Start()
    {
        if (keyItems.Count > 0)
            EnterWalking();
    }

    protected virtual void Update()
    {
        if (_player != null)
        {
            float playerHeight = _player.position.y - transform.position.y;
           // Debug.Log($"playerHeight: {playerHeight:F2} | DetectionMinHeight: {DetectionMinHeight}");
        }
        _stateTimer -= Time.deltaTime;
        if (_gracePeriodTimer > 0f) _gracePeriodTimer -= Time.deltaTime;

        switch (CurrentState)
        {
            case NpcStates.Busy: UpdateBusy(); break;
            case NpcStates.Walking: UpdateWalking(); break;
            case NpcStates.Alerted: UpdateAlerted(); break;
            case NpcStates.Watching: UpdateWatching(); break;
            case NpcStates.Distracted: UpdateDistracted(); break;
        }

        if (CurrentState != NpcStates.Watching && CurrentState != NpcStates.Disabled)
            CheckPlayerDetection();
    }

    //State Machine
    public void EnterBusy()
    {
        ChangeState(NpcStates.Busy);
        _agent.isStopped = true;
        _stateTimer = _busyDuration + Random.Range(-_busyDurationVariance, _busyDurationVariance);
        OnEnterBusy(keyItems[_currentKeyItemIndex]);
    }

    public void EnterWalking()
    {
        ChangeState(NpcStates.Walking);
        _agent.isStopped = false;
        _agent.speed = _walkSpeed;
        _agent.SetDestination(keyItems[_currentKeyItemIndex].position);
        OnEnterWalking(keyItems[_currentKeyItemIndex]);
    }

    public void EnterAlerted()
    {
        if (CurrentState == NpcStates.Disabled) return;

        if (_lookAroundCoroutine != null) StopCoroutine(_lookAroundCoroutine);

        ChangeState(NpcStates.Alerted);
        _agent.isStopped = true;
        _agent.speed = _alertedWalkSpeed;
        _stateTimer = _alertDuration;
        _lookTurnsLeft = _lookAroundTurns;
        _lookAroundCoroutine = StartCoroutine(LookAroundRoutine());
        OnEnterAlerted();
    }

    public void EnterWatching()
    {
        if (_lookAroundCoroutine != null) StopCoroutine(_lookAroundCoroutine);

        ChangeState(NpcStates.Watching);
        _agent.isStopped = true;
        _watchTimer = _watchDuration;
        _detectionTimer = 0f;
        OnEnterWatching();

        PlayerFreezeManager.Instance?.Freeze();
    }

    public void EnterDistracted()
    {
        if (CurrentState == NpcStates.Disabled) return;

        if (_lookAroundCoroutine != null) StopCoroutine(_lookAroundCoroutine);

        ChangeState(NpcStates.Distracted);
        _agent.isStopped = true;
        _stateTimer = _distractedDuration;
        OnEnterDistracted();
    }

    public void EnterDisabled()
    {
        ChangeState(NpcStates.Disabled);
        _agent.isStopped = true;
        if (_lookAroundCoroutine != null) StopCoroutine(_lookAroundCoroutine);
        OnEnterDisabled();
    }

    public void OnHit()
    {
        GetComponent<NpcHitFlash>()?.TriggerFlash();
        //if (CurrentState == NpcStates.Disabled) return;
        //EnterAlerted();
    }
    public void ApplyHitStop()
    {
        Animator anim = GetComponentInChildren<Animator>();
        if (anim == null) return;
        StartCoroutine(PauseAnimation(anim));
    }
    private IEnumerator PauseAnimation(Animator anim)
    {
        // Save current rotation so it doesn't change during hitstop
        Quaternion savedRotation = transform.rotation;

        anim.speed = 0f;  // full pause
        yield return new WaitForSecondsRealtime(_hitStopDuration);
        anim.speed = 1f;

        // Restore rotation in case physics moved it
        transform.rotation = savedRotation;
    }

    // State Machine Updates

    void UpdateBusy()
    {
        OnUpdateBusy();

        if (_stateTimer <= 0f)
        {
            _currentKeyItemIndex = (_currentKeyItemIndex + 1) % keyItems.Count;
            EnterWalking();
        }
    }

    void UpdateWalking()
    {
        OnUpdateWalking();

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            EnterBusy();
    }

    void UpdateAlerted()
    {
        OnUpdateAlerted();

        if (_stateTimer <= 0f)
        {
            if (_lookAroundCoroutine != null) StopCoroutine(_lookAroundCoroutine);
            EnterWalking(); // back to normal routine
        }
    }

    void UpdateWatching()
    {
        if (_player != null)
        {
            Vector3 dir = (_player.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(dir), 5f * Time.deltaTime);
        }

        _watchTimer -= Time.deltaTime;

        if (_watchTimer <= 0f)
        {
            PlayerFreezeManager.Instance?.UnFreeze();
            _gracePeriodTimer = _gracePeriod;
            _currentKeyItemIndex = (_currentKeyItemIndex + 1) % keyItems.Count;
            EnterWalking();
        }

        OnUpdateWatching();
    }

    void UpdateDistracted()
    {
        OnUpdateDistracted();

        if (_stateTimer <= 0f)
            EnterAlerted(); // after distraction ends, go to alerted state
    }

    // Detection

    void CheckPlayerDetection()
    {
        if (_player == null) return;
        if (_gracePeriodTimer > 0f) return;

        if (CanSeePlayer())
        {
            _detectionTimer += Time.deltaTime;
            if (_detectionTimer >= CurrentDetectionBuildUpTime)
                EnterWatching();
        }
        else
        {
            _detectionTimer = 0f;
        }
    }

    bool CanSeePlayer()
    {
        if (_player == null) return false;

        Transform origin = _head != null ? _head : transform;
        Vector3 dirToPlayer = (_player.position - origin.position);
        float distance = dirToPlayer.magnitude;

        if (distance > DetectionRange) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer.normalized);
        if (angle > DetectionAngle) return false;

        // height check ignored when alerted — NPC looks at any height
        if (CurrentState != NpcStates.Alerted)
        {
            float playerHeight = _player.position.y - transform.position.y;
            if (playerHeight < -DetectionMinHeight) return false;
        }

        if (Physics.Raycast(origin.position, dirToPlayer.normalized, distance, _obstacleMask))
            return false;

        return true;
    }

    // Look Around Coroutine

    IEnumerator LookAroundRoutine()
    {
        while (_lookTurnsLeft > 0)
        {
            float targetAngle = Random.Range(30f, 80f) * (Random.value > 0.5f ? 1f : -1f);
            yield return RotateByAngle(targetAngle);
            yield return new WaitForSeconds(0.5f);
            _lookTurnsLeft--;
        }
    }

    IEnumerator RotateByAngle(float angleDelta)
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, angleDelta, 0);
        float duration = Mathf.Abs(angleDelta) / _lookAroundSpeed;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
    }

    // Key Item Disruption (called externally)

    public void OnKeyItemDisrupted()
    {
        if (CurrentState == NpcStates.Disabled) return;
        EnterAlerted();
    }

    // Helpers

    void ChangeState(NpcStates newState)
    {
        NpcStates previous = CurrentState;
        CurrentState = newState;
        OnStateChanged(previous, newState);
    }

    // Override on enter/update methods in subclasses for specific behaviors or animations

    protected virtual void OnEnterBusy(Transform keyItem) { }
    protected virtual void OnEnterWalking(Transform keyItem) { }
    protected virtual void OnEnterAlerted() { }
    protected virtual void OnEnterWatching() { }
    protected virtual void OnEnterDistracted() { }
    protected virtual void OnEnterDisabled() { }

    protected virtual void OnUpdateBusy() { }
    protected virtual void OnUpdateWalking() { }
    protected virtual void OnUpdateAlerted() { }
    protected virtual void OnUpdateWatching() { }
    protected virtual void OnUpdateDistracted() { }

    // Called whenever state changes, can be used for debugging or triggering global events
    protected virtual void OnStateChanged(NpcStates previous, NpcStates next) { }

    // Gizmos

    void OnDrawGizmosSelected()
    {
        // Vision Cone
        Gizmos.color = Color.yellow;
        Vector3 leftDir = Quaternion.Euler(0, -DetectionAngle, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, DetectionAngle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDir * DetectionRange);
        Gizmos.DrawRay(transform.position, rightDir * DetectionRange);
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
        // Key items
        if (keyItems == null) return;
        Gizmos.color = Color.cyan;
        foreach (var item in keyItems)
            if (item != null)
                Gizmos.DrawLine(transform.position, item.position);
    }
}