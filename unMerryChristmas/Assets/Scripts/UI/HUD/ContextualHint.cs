using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ContextualHint : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;

    private CanvasGroup _group;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
    }

    public void Show(string text)
    {
        // Lazy-init guard: Awake() is skipped when a parent Canvas/Panel is inactive
        // at scene load. GetComponent is safe here — RequireComponent guarantees it exists.
        _group ??= GetComponent<CanvasGroup>();

        if (_label != null) _label.text = text;
        _group.alpha = 1f;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
