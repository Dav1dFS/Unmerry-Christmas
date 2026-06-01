using UnityEngine;

/// <summary>
/// Attach to any task-relevant GameObject to provide an accessibility hint.
/// When the player holds the Hint key (default: H) while nearby, the hint
/// text is shown in the ContextualHint display.
///
/// Fill _hintText in the Inspector. Use HintProviderEditor's Preview button
/// in Play Mode to see exactly how the hint renders.
/// </summary>
public class HintProvider : MonoBehaviour
{
    [Header("Accessibility Hint")]
    [SerializeField, TextArea(3, 6)] private string _hintText = "";

    /// <summary>The hint text to display when the player requests help.</summary>
    public string HintText => _hintText;
}
