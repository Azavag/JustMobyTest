using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CubeController : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    private CubeView _cubeView;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _canvasRect;

    private Vector2 _dragOffset;
    private Transform _originalParent;
    private Vector2 _originalPosition;
    private bool _isClone;
    private bool _isTowerPart;

    private CubeFactory _factory;
    private Transform _dragLayer;
    private CubeController _dragInstance;

    private CubeModel _cubeModel;
    public CubeModel CubeModel => _cubeModel;
    public CubeView View => _cubeView;
    public bool IsTowerPart => _isTowerPart;

    public event Action OnDragStart;


    private void Awake()
    {
        _cubeView = GetComponent<CubeView>();
        _canvas = GetComponentInParent<Canvas>();
    }

    public void Initialize(CubeConfig config, Canvas canvas,
                            CubeFactory factory,
                            Transform dragLayer,
                            bool isClone = false)
    {
        _cubeModel = new CubeModel(config);

        _canvas = canvas;
        _canvasRect = _canvas.transform as RectTransform;

        _factory = factory;
        _dragLayer = dragLayer;
        _isClone = isClone;

        _cubeView.Initialize(_cubeModel);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        DragEvents.RaiseDragStarted(this);
        if (!_isClone)
        {
            _dragInstance = _factory.Create(_cubeModel.Config, _dragLayer);
            _dragInstance.Initialize(
                _cubeModel.Config,
                _canvas,
                _factory,
                _dragLayer,
                true);

            _dragInstance.SetPositionFromPointer(eventData);

            eventData.pointerDrag = _dragInstance.gameObject;
            ExecuteEvents.Execute(
                _dragInstance.gameObject,
                eventData,
                ExecuteEvents.beginDragHandler);

            return;
        }

        _originalParent = _cubeView.RectTransform.parent;

        // отключаем LayoutElement
        LayoutElement layout = GetComponent<LayoutElement>();
        if (layout != null)
            layout.ignoreLayout = true;

        _cubeView.BlockRaycast(false);
        _cubeView.RectTransform.SetParent(_dragLayer, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isClone) return;

        RectTransform dragRect = _dragLayer as RectTransform;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            dragRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPos);

        _cubeView.RectTransform.position = worldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragEvents.RaiseDragEnded(this);
        if (!_isClone) return;           
        _cubeView.BlockRaycast(false);
    }

    // Метод для выставления копии под курсор
    public void SetPositionFromPointer(PointerEventData eventData)
    {
        if (_canvas == null || _cubeView.RectTransform == null) return;

        RectTransform dragRect = _dragLayer as RectTransform;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            dragRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPos);

        _cubeView.RectTransform.SetParent(dragRect, false);
        _cubeView.RectTransform.position = worldPos;
    }

    public void MarkAsTowerPart()
    {
        _isTowerPart = true;
        _cubeView.BlockRaycast(true);
    }

    public void PlayJumpAnimation(Vector2 targetAnchoredPos)
    {
        RectTransform rect = _cubeView.RectTransform;

        rect.DOAnchorPos(targetAnchoredPos, 0.3f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                rect.DOAnchorPos(targetAnchoredPos, 0.2f)
                    .SetEase(Ease.InQuad);
            })
            .Play();
    }

    public void PlayFailAnimation()
    {
        _cubeView.RectTransform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject))
            .Play();
    }

    public void OnDrop(PointerEventData eventData)
    {
        CubeController cube = eventData.pointerDrag?.GetComponent<CubeController>();
        if (cube == null)
            return;

        cube.PlayFailAnimation();
    }
}