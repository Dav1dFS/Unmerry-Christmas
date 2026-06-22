using UnityEngine;

public partial class Controller
{
    // ── Settings ─────────────────────────────────────────────────────────────
    [Header("Roll")]
    [SerializeField] private float _rollForce    = 8f;
    [SerializeField] private float _rollDuration = 0.4f;
    [SerializeField] private float _rollCooldown = 1.5f;

    // ── State ────────────────────────────────────────────────────────────────
    private bool    isRolling         = false;
    private float   rollTimer         = 0f;
    private float   rollCooldownTimer = 0f;
    private Vector3 rollDirection;

    /// <summary>Used by IceSlideTracker to detect a rolling impact on the compost bin.</summary>
    public bool IsRolling => isRolling;

    // ── Per-frame update ──────────────────────────────────────────────────────

    private void UpdateRoll()
    {
        if (rollCooldownTimer > 0f)
            rollCooldownTimer -= Time.deltaTime;

        if (!isRolling) return;

        rollTimer -= Time.deltaTime;

        if (rollTimer <= 0f)
        {
            isRolling = false;
            rollCooldownTimer = _rollCooldown;

            // opcional mas recomendado:
            _rb.linearVelocity = Vector3.zero;
        }
    }

    // ── Roll initiation ───────────────────────────────────────────────────────

    private void TryStartRoll()
    {
        if (_pushedObject != null) return;

        if (!AbilityTokenManager.Instance.IsUnlocked(PlayerAbility.Rolling))
            return;
        StartRoll();
    }

    private void StartRoll()
    {
        if (isRolling || rollCooldownTimer > 0f) return;

        rollDirection = transform.forward;
        isRolling = true;
        rollTimer = _rollDuration;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _animator.SetTrigger("Roll");
    }
}
