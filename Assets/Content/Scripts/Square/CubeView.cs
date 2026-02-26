using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class CubeView : MonoBehaviour
{
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    [SerializeField] 
    private Image _image;

    public CanvasGroup CanvasGroup => _canvasGroup;
    public RectTransform RectTransform => _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Initialize(CubeModel model)
    {
        if (_image != null)
        {
            _image.sprite = model.Config.Sprite;
        }
        _rectTransform.sizeDelta = model.Config.Size;
    }

    // Анимация установки
    public void PlayPlaceAnimation()
    {
        _rectTransform.localScale = Vector3.zero;
        _rectTransform.DOScale(1.1f, 0.15f).OnComplete(() =>
            _rectTransform.DOScale(1f, 0.1f));
    }

    // Исчезновение
    public void PlayDisappearAnimation(Action onComplete = null)
    {
        _rectTransform.DOScale(0f, 0.2f);
        _canvasGroup.DOFade(0f, 0.2f).OnComplete(() => onComplete?.Invoke());
    }

    public void BlockRaycast(bool isBlock)
    {
        _canvasGroup.blocksRaycasts = isBlock;
    }

    public void MakeTransparent()
    {
        _canvasGroup.DOFade(0.25f, 0.2f)
            .Play();
    }
}