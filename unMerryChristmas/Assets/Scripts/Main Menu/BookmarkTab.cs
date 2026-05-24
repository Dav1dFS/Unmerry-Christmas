using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

public class BookmarkTab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Sprites")]
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _hoverSprite;
    [SerializeField] private Sprite _selectedSprite;

    [Header("References")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _tabImage;
    [SerializeField] private RectTransform _icon;       // arrasta o filho icon aqui

    [Header("Slide Animation")]
    [SerializeField] private float _slideOutX = -200f;
    [SerializeField] private float _slideDuration = 0.25f;

    [Header("Icon Scale")]
    [SerializeField] private float _normalScale = 1f;
    [SerializeField] private float _hoverScale = 0.85f;
    [SerializeField] private float _pressedScale = 0.9f;
    [SerializeField] private float _selectedScale = 0.8f;
    [SerializeField] private float _scaleSpeed = 12f;   // lerp speed

    private Action<int> _onClicked;
    private int _index;
    private bool _isSelected = false;
    private Vector2 _originalPos;
    private Coroutine _slideRoutine;
    private float _targetScale;
    private Coroutine _scaleRoutine;

    private void Awake()
    {
        _originalPos = GetComponent<RectTransform>().anchoredPosition;
        _targetScale = _normalScale;
    }

    public void Init(int index, Action<int> callback)
    {
        _index = index;
        _onClicked = callback;
        _button.onClick.AddListener(() => _onClicked(_index));
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        _tabImage.sprite = selected ? _selectedSprite : _normalSprite;
        _button.interactable = !selected;

        // Slide
        if (_slideRoutine != null) StopCoroutine(_slideRoutine);
        _slideRoutine = StartCoroutine(
            SlideTo(selected ? _slideOutX : _originalPos.x, _slideDuration));

        // Scale do icon
        SetIconScale(selected ? _selectedScale : _normalScale);
    }

    // Pointer events

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isSelected) return;
        _tabImage.sprite = _hoverSprite;
        SetIconScale(_hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isSelected) return;
        _tabImage.sprite = _normalSprite;
        SetIconScale(_normalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isSelected) return;
        SetIconScale(_pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isSelected) return;
        // Volta para hover porque o rato ainda está por cima
        SetIconScale(_hoverScale);
    }

    // Scale helper

    private void SetIconScale(float target)
    {
        if (_icon == null)
        {
            Debug.LogWarning($"[BookmarkTab] {gameObject.name}: _icon é null!", this);
            return;
        }
        if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);
        _scaleRoutine = StartCoroutine(ScaleTo(target));
    }

    private IEnumerator ScaleTo(float target)
    {
        Vector3 from = _icon.localScale;
        Vector3 to = Vector3.one * target;
        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            _icon.localScale = Vector3.LerpUnclamped(from, to, t);
            yield return null;
        }
        _icon.localScale = to;
    }

    // Slide helper

    private IEnumerator SlideTo(float targetX, float duration)
    {
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = new Vector2(targetX, startPos.y);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            rt.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, t);
            yield return null;
        }
        rt.anchoredPosition = endPos;
    }
}