using UnityEngine;
using UnityEngine.EventSystems;

public class CubeController : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private CubeView _view;
    private Canvas _canvas;
    private RectTransform _canvasRect;

    private Vector2 _dragOffset;
    private Transform _originalParent;
    private Vector2 _originalPosition;
    private CubeModel _cubeModel;


    private bool _isClone;
    private CubeFactory _factory;
    private Transform _dragLayer;
    private CubeController _dragInstance;

    private void Awake()
    {
        _view = GetComponent<CubeView>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasRect = _canvas.transform as RectTransform;
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

        _view.Initialize(_cubeModel);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Если это оригинал — создаём копию
        if (!_isClone)
        {
            _dragInstance = _factory.Create(_cubeModel.Config, _dragLayer);

            _dragInstance.Initialize(
                _cubeModel.Config,
                _canvas,
                _factory,
                _dragLayer,
                true);

            // ❗ Ставим позицию копии в точку курсора
            _dragInstance.SetPositionFromPointer(eventData);

            // Начинаем drag у копии
            _dragInstance.OnBeginDrag(eventData);
            return;
        }

        _originalParent = _view.RectTransform.parent;
        _originalPosition = _view.RectTransform.anchoredPosition;

        // Перемещаем куб на верхний слой
        _view.RectTransform.SetParent(_canvasRect);
        _view.RectTransform.SetAsLastSibling();

        _view.CanvasGroup.blocksRaycasts = false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        _dragOffset = _view.RectTransform.anchoredPosition - localPoint;
    }

    // Метод для выставления копии под курсор
    public void SetPositionFromPointer(PointerEventData eventData)
    {
        if (_canvas == null || _view.RectTransform == null) return;

        // Преобразуем экранные координаты в локальные координаты DragLayer
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _dragLayer as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        _view.RectTransform.SetParent(_dragLayer); // переносим в drag слой
        _view.RectTransform.anchoredPosition = localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isClone) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        _view.RectTransform.anchoredPosition = localPoint + _dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isClone) return;

        Destroy(gameObject);
    }
}