using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonTextColor : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,
    IDeselectHandler
{
    [Header("Text Reference")]
    [SerializeField] private TextMeshProUGUI _text;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _hoverColor = Color.red;
    [SerializeField] private Color _pressedColor = Color.gray;

    private void Awake()
    {
        if (_text == null)
            _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        SetColor(_normalColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetColor(_hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetColor(_normalColor);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetColor(_pressedColor);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetColor(_hoverColor);
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetColor(_hoverColor);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetColor(_normalColor);
    }

    private void SetColor(Color color)
    {
        if (_text != null)
            _text.color = color;
    }
}