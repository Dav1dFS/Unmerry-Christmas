using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Drives one row in the Controls settings panel.
///
/// A row shows one logical control as:
///   [ Action name ]   [ Keyboard key button ]   [ Gamepad button ]
///
/// The current binding is shown ON the button itself; clicking the button starts
/// an interactive rebind of that specific binding. Either slot may be empty (e.g.
/// the keyboard direction rows of a composite have no gamepad equivalent, and the
/// gamepad-stick row has no keyboard key) — empty slots show a dash and are disabled.
///
/// Each slot targets an absolute binding index into <see cref="_action"/>'s
/// bindings, assigned by SetupSettingsAll at edit time. Indices are stable because
/// they reflect the order baked into the .inputactions asset.
/// </summary>
public class RebindActionRow : MonoBehaviour
{
    [SerializeField] private InputActionReference _action;
    [SerializeField] private TMP_Text             _actionNameLabel;

    [Header("Keyboard slot")]
    [SerializeField] private Button   _keyboardButton;
    [SerializeField] private TMP_Text _keyboardLabel;
    [SerializeField] private int      _keyboardBindingIndex = -1;

    [Header("Gamepad slot")]
    [SerializeField] private Button   _gamepadButton;
    [SerializeField] private TMP_Text _gamepadLabel;
    [SerializeField] private int      _gamepadBindingIndex = -1;

    private InputActionRebindingExtensions.RebindingOperation _rebindOp;

    private void OnEnable()
    {
        // Wire buttons here rather than relying on prefab OnClick wiring.
        if (_keyboardButton != null) _keyboardButton.onClick.AddListener(StartKeyboardRebind);
        if (_gamepadButton  != null) _gamepadButton.onClick.AddListener(StartGamepadRebind);
        RefreshBindingDisplay();
    }

    private void OnDisable()
    {
        if (_keyboardButton != null) _keyboardButton.onClick.RemoveListener(StartKeyboardRebind);
        if (_gamepadButton  != null) _gamepadButton.onClick.RemoveListener(StartGamepadRebind);
        CleanupOperation();
    }

    /// <summary>Updates both slot buttons to show their current effective binding.</summary>
    public void RefreshBindingDisplay()
    {
        UpdateSlot(_keyboardButton, _keyboardLabel, _keyboardBindingIndex);
        UpdateSlot(_gamepadButton,  _gamepadLabel,  _gamepadBindingIndex);
    }

    private void UpdateSlot(Button btn, TMP_Text label, int bindingIndex)
    {
        bool valid = _action?.action != null
                     && bindingIndex >= 0
                     && bindingIndex < _action.action.bindings.Count;

        // Keep the button in place so columns stay aligned across rows, but disable
        // it (and show a dash) when this slot has no binding.
        if (btn != null)
        {
            btn.interactable = valid;
            // Also suppress the hover/press tint so an empty "—" doesn't look clickable.
            var btc = btn.GetComponent<ButtonTextColor>();
            if (btc != null) btc.enabled = valid;
        }
        if (label == null) return;

        label.text = valid
            ? InputControlPath.ToHumanReadableString(
                  _action.action.bindings[bindingIndex].effectivePath,
                  InputControlPath.HumanReadableStringOptions.OmitDevice)
            : "—";
    }

    // ── Rebind entry points (wired to the two buttons) ─────────────────────────
    public void StartKeyboardRebind() =>
        StartRebind(_keyboardBindingIndex, _keyboardButton, _keyboardLabel, "<Keyboard>");

    public void StartGamepadRebind() =>
        StartRebind(_gamepadBindingIndex, _gamepadButton, _gamepadLabel, "<Gamepad>");

    private void StartRebind(int bindingIndex, Button btn, TMP_Text label, string deviceFilter)
    {
        if (_action?.action == null) return;
        if (bindingIndex < 0 || bindingIndex >= _action.action.bindings.Count) return;
        if (_rebindOp != null) return; // a rebind is already in progress

        _action.action.Disable();
        if (btn != null)   btn.interactable = false;
        if (label != null) label.text = "...";

        _rebindOp = _action.action
            .PerformInteractiveRebinding(bindingIndex)
            .WithControlsHavingToMatchPath(deviceFilter)   // only accept the right device
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(_ => FinishRebind(btn))
            .OnCancel(_  => FinishRebind(btn))
            .Start();
    }

    private void FinishRebind(Button btn)
    {
        _rebindOp?.Dispose();
        _rebindOp = null;

        _action.action.Enable();
        if (btn != null) btn.interactable = true;

        RefreshBindingDisplay();
        AccessibilityManager.Instance?.SaveBindings();
    }

    private void CleanupOperation()
    {
        _rebindOp?.Dispose();
        _rebindOp = null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Called by SetupSettingsAll to configure the row. <paramref name="kbIndex"/>
    /// / <paramref name="gpIndex"/> are absolute binding indices, or -1 if absent.
    /// </summary>
    public void SetupFromEditor(InputActionReference actionRef, string displayName,
                                int kbIndex, int gpIndex)
    {
        _action               = actionRef;
        _keyboardBindingIndex = kbIndex;
        _gamepadBindingIndex  = gpIndex;
        if (_actionNameLabel != null) _actionNameLabel.text = displayName;
    }
#endif
}
