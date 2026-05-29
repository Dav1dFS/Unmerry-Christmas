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

    [Header("Slide Animation")]
    [SerializeField] private float _slideOutX = -200f;
    [SerializeField] private float _slideDuration = 0.25f;

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

    // ---------------- STATE ----------------

   public void SetSelected(bool selected)
    {
        _isSelected = selected;

        StopSlide();
        StartSlide(selected ? _slideOutX : _originalPos.x);

        UpdateVisual();
    }

    public void OnSelect(BaseEventData eventData)
    {
        _isHighlighted = true;
        UpdateVisual();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _isHighlighted = false;
        UpdateVisual();
    }

    // ---------------- POINTER ----------------

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHighlighted = true;
        UpdateVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHighlighted = false;
        UpdateVisual();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        SetIconScale(_pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        UpdateVisual();
    }   

    // ---------------- ICON SCALE ----------------

    private void SetIconScale(float target)
    {
        if (_icon == null) return;

        if (_scaleRoutine != null)
            StopCoroutine(_scaleRoutine);

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
            float smoothed = Mathf.SmoothStep(0f, 1f, t);

            _icon.localScale = Vector3.Lerp(start, end, smoothed);
            yield return null;
        }

        _icon.localScale = end;
    }

    // ---------------- SLIDE ----------------

    private void StartSlide(float targetX)
    {
        _slideRoutine = StartCoroutine(SlideTo(targetX));
    }

    private void StopSlide()
    {
        if (_slideRoutine != null)
            StopCoroutine(_slideRoutine);
    }

    private void UpdateVisual()
    {
        if (_isSelected)
        {
            _tabImage.sprite = _selectedSprite;
            SetIconScale(_selectedScale);
            return;
        }

        if (_isHighlighted)
        {
            _tabImage.sprite = _hoverSprite;
            SetIconScale(_hoverScale);
            return;
        }

        _tabImage.sprite = _normalSprite;
        SetIconScale(_normalScale);
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
            float smoothed = Mathf.SmoothStep(0f, 1f, t);

            rt.anchoredPosition = Vector2.Lerp(start, end, smoothed);
            yield return null;
        }

        rt.anchoredPosition = end;
    }
}