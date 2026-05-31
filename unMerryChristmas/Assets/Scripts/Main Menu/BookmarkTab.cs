using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

public class BookmarkTab : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,
    IDeselectHandler
{
    [Header("Sprites")]
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _hoverSprite;
    [SerializeField] private Sprite _selectedSprite;

    [Header("References")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _tabImage;
    [SerializeField] private RectTransform _icon;
    [SerializeField] private RectTransform _description;

    [Header("Slide Animation")]
    [SerializeField] private float _slideOutX = -200f;
    [SerializeField] private float _slideDuration = 0.25f;

    [Header("Icon Position Per State")]
    [SerializeField] private float _iconNormalX = 0f;
    [SerializeField] private float _iconHoverX = 0f;
    [SerializeField] private float _iconSelectedX = 0f;

    [Header("Description Position Per State")]
    [SerializeField] private float _descNormalX = 0f;
    [SerializeField] private float _descHoverX = 0f;
    [SerializeField] private float _descSelectedX = 0f;

    [Header("Icon Scale")]
    [SerializeField] private float _normalScale = 1f;
    [SerializeField] private float _hoverScale = 0.85f;
    [SerializeField] private float _pressedScale = 0.9f;
    [SerializeField] private float _selectedScale = 0.8f;

    public int Index { get; private set; }

    private Action<int> _onClicked;
    private bool _isSelected;
    private bool _isHighlighted;
    private Vector2 _originalPos;
    private Coroutine _slideRoutine;
    private Coroutine _scaleRoutine;
    private Coroutine _iconPosRoutine;
    private Coroutine _descPosRoutine;

    private void Awake()
    {
        _originalPos = GetComponent<RectTransform>().anchoredPosition;
    }

    public void Init(int index, Action<int> callback)
    {
        Index = index;
        _onClicked = callback;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => _onClicked?.Invoke(Index));

        SetSelected(false);
    }

    // State

    public void SetSelected(bool selected)
    {
        _isSelected = selected;

        StopSlide();
        StartSlide(selected ? _slideOutX : _originalPos.x);

        UpdateVisual();
    }

    public void OnSelect(BaseEventData eventData) { _isHighlighted = true; UpdateVisual(); }
    public void OnDeselect(BaseEventData eventData) { _isHighlighted = false; UpdateVisual(); }

    // Pointer

    public void OnPointerEnter(PointerEventData eventData) { _isHighlighted = true; UpdateVisual(); }
    public void OnPointerExit(PointerEventData eventData) { _isHighlighted = false; UpdateVisual(); }
    public void OnPointerDown(PointerEventData eventData) { SetIconScale(_pressedScale); }
    public void OnPointerUp(PointerEventData eventData) { UpdateVisual(); }

    // Visual — updates sprite, icon scale and positions based on current state

    private void UpdateVisual()
    {
        if (_isSelected)
        {
            _tabImage.sprite = _selectedSprite;
            SetIconScale(_selectedScale);
            AnimatePosX(_icon, ref _iconPosRoutine, _iconSelectedX);
            AnimatePosX(_description, ref _descPosRoutine, _descSelectedX);
            return;
        }

        if (_isHighlighted)
        {
            _tabImage.sprite = _hoverSprite;
            SetIconScale(_hoverScale);
            AnimatePosX(_icon, ref _iconPosRoutine, _iconHoverX);
            AnimatePosX(_description, ref _descPosRoutine, _descHoverX);
            return;
        }

        _tabImage.sprite = _normalSprite;
        SetIconScale(_normalScale);
        AnimatePosX(_icon, ref _iconPosRoutine, _iconNormalX);
        AnimatePosX(_description, ref _descPosRoutine, _descNormalX);
    }

    // Icon scale animation

    private void SetIconScale(float target)
    {
        if (_icon == null) return;
        if (_scaleRoutine != null) StopCoroutine(_scaleRoutine);
        _scaleRoutine = StartCoroutine(ScaleTo(target));
    }

    private IEnumerator ScaleTo(float target)
    {
        Vector3 start = _icon.localScale;
        Vector3 end = Vector3.one * target;
        float t = 0f;
        float duration = 0.15f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            _icon.localScale = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        _icon.localScale = end;
    }

    // Smooth Pos X animation for icon and description

    private void AnimatePosX(RectTransform rt, ref Coroutine routine, float targetX)
    {
        if (rt == null) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SlidePosX(rt, targetX));
    }

    private IEnumerator SlidePosX(RectTransform rt, float targetX)
    {
        Vector2 start = rt.anchoredPosition;
        Vector2 end = new Vector2(targetX, start.y);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / _slideDuration;
            rt.anchoredPosition = Vector2.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        rt.anchoredPosition = end;
    }

    // Tab slide in/out

    private void StartSlide(float targetX)
    {
        _slideRoutine = StartCoroutine(SlideTo(targetX));
    }

    private void StopSlide()
    {
        if (_slideRoutine != null) StopCoroutine(_slideRoutine);
    }

    private IEnumerator SlideTo(float targetX)
    {
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 start = rt.anchoredPosition;
        Vector2 end = new Vector2(targetX, start.y);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / _slideDuration;
            rt.anchoredPosition = Vector2.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        rt.anchoredPosition = end;
    }
}