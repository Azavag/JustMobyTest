using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CubeController : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private CubeView _view;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _canvasRect;

    private Vector2 _dragOffset;
    private Transform _originalParent;
    private Vector2 _originalPosition;
    private CubeModel _cubeModel;


    [SerializeField] private bool _isClone;
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

        _originalParent = _view.RectTransform.parent;

        // отключаем LayoutElement
        LayoutElement layout = GetComponent<LayoutElement>();
        if (layout != null)
            layout.ignoreLayout = true;

        _view.RectTransform.SetParent(_dragLayer, true);
    }

    // Метод для выставления копии под курсор
    public void SetPositionFromPointer(PointerEventData eventData)
    {
        if (_canvas == null || _view.RectTransform == null) return;

        RectTransform dragRect = _dragLayer as RectTransform;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            dragRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPos);

        _view.RectTransform.SetParent(dragRect, false);
        _view.RectTransform.position = worldPos;
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

        _view.RectTransform.position = worldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isClone) return;

        Debug.Log("OnEndDrag");

        Destroy(gameObject);
    }
}