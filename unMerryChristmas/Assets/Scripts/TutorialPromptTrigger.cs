using UnityEngine;

/// <summary>
/// Tutorial Prompt System — one-shot trigger that shows a TutorialToast when the
/// player first walks into the zone.
///
/// ARCHITECTURE
/// ─────────────
///  TutorialPromptTrigger (scene trigger zone)
///      └─▶ UIManager.ShowTutorialPrompt(text)
///              └─▶ TutorialToast.Show(text)   [fade-in → hold → fade-out]
///
/// SCENE SETUP (place one trigger per prompt):
///  1. Create an empty GameObject with a Trigger Collider (Box or Sphere).
///  2. Add this component.
///  3. Set PromptId (for your own reference) and fill in _promptText.
///  4. Ensure the Player GameObject is tagged "Player".
///  The trigger fires once and then deactivates itself.
///
/// PLANNED PROMPTS (add trigger GameObjects in the BackYard scene):
///   Movement          — near the spawn point, first few seconds
///   Running           — a few metres from spawn
///   Jumping           — near the garden step / ledge
///   GrabbingCarrying  — near the gnomes or chairs
///   BareHandsInteraction — near any IInteractable (shed, compost bin, etc.)
///   DrawingBook       — near the ability token or after first task completes
///   Pushing           — near the stone bench (after Pushing token spawns)
///   Rolling           — near the wooden ball (after Rolling token spawns)
///
/// TODO(HUD): If the HUD adds a "hint log" panel, also call
///            UIManager.Instance?.OpenBookToAbilitiesPage() here after the
///            DrawingBook prompt so the player sees the book open automatically.
/// </summary>
public class TutorialPromptTrigger : MonoBehaviour
{
    public enum PromptId
    {
        Movement,
        Running,
        Jumping,
        GrabbingCarrying,
        BareHandsInteraction,
        DrawingBook,
        Pushing,
        Rolling,
    }

    [SerializeField] private PromptId _promptId;
    [SerializeField] private string   _promptText;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        UIManager.Instance?.ShowTutorialPrompt(_promptText);
        Debug.Log($"[TutorialPrompt] {_promptId}: {_promptText}");

        // Deactivate after firing so it only triggers once per scene load
        gameObject.SetActive(false);
    }
}
