using FMODUnity;
using UnityEngine;

public partial class Controller
{
    // ── Settings ─────────────────────────────────────────────────────────────
    [Header("Jump & Landing")]
    [SerializeField] private EventReference jumpSound;
    [SerializeField] private EventReference landSound;

    private readonly float jumpForce  = 6f;
    private bool           jump       = false;
    private bool           isGrounded = true;

    // ── Ground detection & landing audio ─────────────────────────────────────
    //
    // State machine: Grounded / Airborne.
    //
    // Grounded → Airborne : raycast stops detecting a floor surface.
    // Airborne → Grounded : raycast detects a floor AND vertical velocity ≤ 0
    //                        (player is descending or settled, not still rising).
    //
    // The ray is kept short (player half-height + tiny buffer) so "ground"
    // only registers when the feet are essentially touching the surface.
    // This removes the false-early-landing window that previously let a
    // queued jump fire while the player was still visibly airborne.

    private const float GroundRayLength = 0.65f; // just past half-height (0.5 m)

    private void UpdateGroundDetection()
    {
        bool hit = Physics.Raycast(
            transform.position, Vector3.down, out RaycastHit rayHit, GroundRayLength)
            && rayHit.normal.y > 0.7f;

        bool wasGrounded = isGrounded;

        if (isGrounded)
        {
            // Leave the ground as soon as the surface disappears below us.
            if (!hit) isGrounded = false;
        }
        else
        {
            // Only land when we are moving downward (or settled) AND the
            // floor is within reach — prevents a ceiling or ledge above
            // from being misread as a landing surface while jumping.
            if (hit && _rb.linearVelocity.y <= 0f)
                isGrounded = true;
        }

        if (isGrounded && !wasGrounded)          // just landed
        {
            float surfaceValue = 0f;
            if (rayHit.collider != null)
            {
                surfaceValue = rayHit.collider.tag switch
                {
                    "Stone" => 1f,
                    "Metal" => 2f,
                    "Snow"  => 3f,
                    _       => 0f,
                };
            }
            AudioManager.instance?.PlayOneShotWithParameter(
                landSound, transform.position, "SurfaceType", surfaceValue);
        }
    }
}
