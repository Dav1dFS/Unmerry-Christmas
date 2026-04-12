using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
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
    [SerializeField] private float _detectionAngle = 30f; // ângulo de visão (metade do cone)
    [SerializeField] private float _detectionRange = 4f;
    [SerializeField] private float _watchDuration = 3f; 
    [SerializeField] private float _alertDuration = 8f;
    [SerializeField] private float _lookAroundSpeed = 60f; // graus/segundo ao olhar à volta
    [SerializeField] private int _lookAroundTurns = 3; // quantas vezes vira a cabeça antes de se acalmar
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _obstacleMask; // paredes etc para o raycast de visão
    private float _watchTimer = 0f;

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _alertedWalkSpeed = 1f;    // slower when alerted
    [SerializeField] private float _arrivedThreshold = 0.4f;

    [Header("Distraction")]
    [SerializeField] private float _distractedDuration = 5f;

    [Header("References")]
    [SerializeField] private Transform _head;                 // transform da cabeça para rotação de visão
    [SerializeField] private Transform _player;               // referência ao player (pode auto-detetar)

    public NpcStates CurrentState { get; private set; } = NpcStates.Busy;
    public float DetectionProgress => _detectionBuildUpTime > 0f ?
    Mathf.Clamp01(_detectionTimer / _detectionBuildUpTime) : 0f;

    private NavMeshAgent _agent;
    private int _currentKeyItemIndex = 0;
    private float _stateTimer = 0f;
    private int _lookTurnsLeft = 0;
    private Coroutine _lookAroundCoroutine;
    private float _watchCooldown = 3f;      // campo privado
    private float _watchCooldownTimer = 0f; // timer do cooldown

    [SerializeField] private float _detectionBuildUpTime = 2f; // segundos até watching
    [SerializeField] private float _gracePeriod = 4f;          // segundos sem deteção após watching
    [SerializeField] private float _alertedDetectionBuildUpTime = 1f; // 1 segundo quando alerted
    private float _detectionTimer = 0f;   // acumula enquanto te vê
    private float _gracePeriodTimer = 0f; // conta o grace period
    private float CurrentDetectionBuildUpTime =>
    CurrentState == NpcStates.Alerted ? _alertedDetectionBuildUpTime : _detectionBuildUpTime;
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
    void EnterBusy()
    {
        ChangeState(NpcStates.Busy);
        _agent.isStopped = true;
        _stateTimer = _busyDuration + Random.Range(-_busyDurationVariance, _busyDurationVariance);
        OnEnterBusy(keyItems[_currentKeyItemIndex]);
    }

    void EnterWalking()
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

    void EnterWatching()
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
        if (CurrentState == NpcStates.Disabled) return;
        EnterAlerted();
    }

    // State Machine — Update 

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
            EnterWalking(); // volta à rotina normal
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

        if (_watchTimer <= 0f || !CanSeePlayer())
        {
            PlayerFreezeManager.Instance?.UnFreeze();
            _gracePeriodTimer = _gracePeriod; // inicia grace period
            _currentKeyItemIndex = (_currentKeyItemIndex + 1) % keyItems.Count;
            EnterWalking();
        }

        OnUpdateWatching();
    }

    void UpdateDistracted()
    {
        OnUpdateDistracted();

        if (_stateTimer <= 0f)
            EnterAlerted(); // após distraído fica alerted antes de voltar ao normal
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

        if (distance > _detectionRange) return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer.normalized);
        if (angle > _detectionAngle) return false;

        // Raycast para verificar que não há parede à frente
        if (Physics.Raycast(origin.position, dirToPlayer.normalized, distance, _obstacleMask))
            return false;

        return true;
    }

    // Look Around Coroutine

    IEnumerator LookAroundRoutine()
    {
        while (_lookTurnsLeft > 0)
        {
            // Roda para um lado
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

    // Key Item Disruption (chamado externamente)

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

    // Virtuais — override nas subclasses para comportamentos específicos

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

    // Chamado sempre que o estado muda — útil para animações
    protected virtual void OnStateChanged(NpcStates previous, NpcStates next) { }

    // Gizmos

    void OnDrawGizmosSelected()
    {
        // Cone de visão
        Gizmos.color = Color.yellow;
        Vector3 leftDir = Quaternion.Euler(0, -_detectionAngle, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, _detectionAngle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDir * _detectionRange);
        Gizmos.DrawRay(transform.position, rightDir * _detectionRange);
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        // Key items
        if (keyItems == null) return;
        Gizmos.color = Color.cyan;
        foreach (var item in keyItems)
            if (item != null)
                Gizmos.DrawLine(transform.position, item.position);
    }
}