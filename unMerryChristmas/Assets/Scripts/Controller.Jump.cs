using FMODUnity;
using UnityEngine;

public partial class Controller
{
    // ── Settings ─────────────────────────────────────────────────────────────
    [Header("Jump & Landing")]
    [SerializeField] private EventReference jumpSound;
    [SerializeField] private EventReference landSound;

    private readonly float jumpForce  = 6f; // tuned for bench/table clearance at mass 2.0
    private bool           jump       = false;
    private bool           isGrounded = true;
    private bool           _wasGrounded = true;

    // ── Ground detection & landing audio ─────────────────────────────────────

    private void UpdateGroundDetection()
    {
        bool currentlyGrounded = Physics.Raycast(
            transform.position, Vector3.down, out RaycastHit hit, 1.1f);

        // Capture the transition BEFORE updating state, then update state immediately.
        // State must advance regardless of whether audio plays — a null AudioManager
        // must never cause the landing sound to re-fire every subsequent frame.
        bool justLanded = currentlyGrounded && !_wasGrounded;
        isGrounded   = currentlyGrounded;
        _wasGrounded = isGrounded;

        if (justLanded)
        {
            // Map surface tag to FMOD parameter value.
            // Guard against a null collider: in rare cases (collider destroyed the
            // same frame, or certain trigger overlap results) hit.collider can be null
            // even when Physics.Raycast returns true.
            float surfaceValue = 0f;
            if (hit.collider != null)
            {
                surfaceValue = hit.collider.tag switch
                {
                    "Stone" => 1f,
                    "Metal" => 2f,
                    "Snow"  => 3f,
                    _       => 0f,  // "Wood" and everything else
                };
            }
            AudioManager.instance?.PlayOneShotWithParameter(
                landSound, transform.position, "SurfaceType", surfaceValue);
        }
    }
}
