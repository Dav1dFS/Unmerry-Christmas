using UnityEngine;

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
        Debug.Log($"[PROMPT] {_promptId}: {_promptText}");

        gameObject.SetActive(false);
    }
}
