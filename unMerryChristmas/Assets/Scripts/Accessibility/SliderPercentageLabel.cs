using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the same GameObject as a Slider.
/// Keeps a TMP_Text label showing the current value as a percentage (0 – 100 %).
/// Wired automatically by BuildSettingsPrefab.
/// </summary>
[RequireComponent(typeof(Slider))]
public class SliderPercentageLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (_slider == null) return;
        _slider.onValueChanged.AddListener(Refresh);
        Refresh(_slider.value);
    }

    private void OnDisable()
    {
        if (_slider != null)
            _slider.onValueChanged.RemoveListener(Refresh);
    }

    private void Refresh(float value)
    {
        if (_label != null)
            _label.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
