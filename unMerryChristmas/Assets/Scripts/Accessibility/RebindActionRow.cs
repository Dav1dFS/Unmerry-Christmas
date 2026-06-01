using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Drives one row in the Controls settings panel.
/// Each row shows the current binding for one action and has a Rebind button.
///
/// Inspector wiring:
///   _action             → InputActionReference for the action this row represents
///   _actionNameLabel    → TMP_Text showing the action name (e.g. "Jump")
///   _currentBindingLabel → TMP_Text showing current key (e.g. "Space")
///   _rebindButton       → The ↺ button players click to start rebinding
///   _rebindButtonLabel  → TMP_Text on the rebind button (shows "↺" or "Press a key…")
/// </summary>
public class RebindActionRow : MonoBehaviour
{
    [SerializeField] private InputActionReference _action;
    [SerializeField] private TMP_Text             _actionNameLabel;
    [SerializeField] private TMP_Text             _currentBindingLabel;
    [SerializeField] private Button               _rebindButton;
    [SerializeField] private TMP_Text             _rebindButtonLabel;

    private InputActionRebindingExtensions.RebindingOperation _rebindOp;

    private void OnEnable()  => RefreshBindingDisplay();
    private void OnDisable() => CleanupOperation();

    /// <summary>Updates the label to show the current effective binding path.</summary>
    public void RefreshBindingDisplay()
    {
        if (_action?.action == null || _currentBindingLabel == null) return;

        // Prefer the Keyboard&Mouse binding group; fall back to index 0
        int idx = _action.action.GetBindingIndex(InputBinding.MaskByGroup("Keyboard&Mouse"));
        if (idx < 0) idx = 0;
        if (idx >= _action.action.bindings.Count) return;

        _currentBindingLabel.text = InputControlPath.ToHumanReadableString(
            _action.action.bindings[idx].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }

    /// <summary>
    /// Called by the Rebind button's OnClick event.
    /// Disables the action, listens for a key press, then re-enables and saves.
    /// </summary>
    public void StartRebind()
    {
        if (_action?.action == null) return;

        _action.action.Disable();
        if (_rebindButton != null)      _rebindButton.interactable = false;
        if (_rebindButtonLabel != null) _rebindButtonLabel.text = "Press a key…";

        int idx = _action.action.GetBindingIndex(InputBinding.MaskByGroup("Keyboard&Mouse"));
        if (idx < 0) idx = 0;
        if (idx >= _action.action.bindings.Count) { FinishRebind(committed: false); return; }

        _rebindOp = _action.action
            .PerformInteractiveRebinding(idx)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(_ => FinishRebind(committed: true))
            .OnCancel(_  => FinishRebind(committed: false))
            .Start();
    }

    private void FinishRebind(bool committed)
    {
        _rebindOp?.Dispose();
        _rebindOp = null;

        _action.action.Enable();
        if (_rebindButton != null)      _rebindButton.interactable = true;
        if (_rebindButtonLabel != null) _rebindButtonLabel.text = "↺";

        RefreshBindingDisplay();

        if (committed)
            AccessibilityManager.Instance?.SaveBindings();
    }

    private void CleanupOperation()
    {
        _rebindOp?.Dispose();
        _rebindOp = null;
    }

#if UNITY_EDITOR
    /// <summary>Called by RebindRowSetupTool to wire the action reference and name.</summary>
    public void SetupFromEditor(InputActionReference actionRef, string actionName)
    {
        _action = actionRef;
        if (_actionNameLabel != null) _actionNameLabel.text = actionName;
    }
#endif
}
