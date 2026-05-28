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

        if (currentlyGrounded && !_wasGrounded)
        {
            // Map surface tag to FMOD parameter value
            float surfaceValue = hit.collider.tag switch
            {
                "Stone" => 1f,
                "Metal" => 2f,
                "Snow"  => 3f,
                _       => 0f,  // "Wood" and everything else
            };
            AudioManager.instance.PlayOneShotWithParameter(
                landSound, transform.position, "SurfaceType", surfaceValue);
        }

        isGrounded   = currentlyGrounded;
        _wasGrounded = isGrounded;
    }
}
