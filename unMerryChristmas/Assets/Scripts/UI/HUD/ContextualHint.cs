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
        if (_label != null) _label.text = text;
        _group.alpha = 1f;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
