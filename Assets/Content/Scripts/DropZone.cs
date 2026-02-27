using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private PlacementRuleType ruleType;
    [SerializeField] private NotificationController notificationController;

    [Header("Zones")]
    [SerializeField] private RectTransform firstCubeZone;
    [SerializeField] private RectTransform stackRoot;

    [Header("Stack Settings")]
    [SerializeField] private float maxOffsetPercent = 0.5f;

    [Header("Rule Type")]
    [SerializeField] private List<CubeController> _cubes = new();

    private ICubePlacementRule _firstRule;
    private ICubePlacementRule _placementRule;

    private float _currentOffsetX = 0f;
    private float _currentOffsetY = 0f;

    [SerializeField] private CubeFactory cubeFactory; // префаб куба

    private void Awake()
    {
        _firstRule = new FirstCubeRule();
        _placementRule = CreateRule(ruleType);
    }

    private void Start()
    {
        // Загружаем сохранённую башню
        TowerSaveData savedData = TowerSaveSystem.LoadTower();

        if (savedData != null && savedData.Cubes.Count > 0)
        {
            LoadTowerFromData(savedData, cubeFactory);
        }
    }

    private void OnEnable()
    {
        DragEvents.OnDragStarted += HandleDragStarted;
        DragEvents.OnDragEnded += HandleDragEnded;
    }
    private void OnDisable()
    {
        DragEvents.OnDragStarted -= HandleDragStarted;
        DragEvents.OnDragEnded -= HandleDragEnded;
    }

    private void HandleDragStarted(CubeController controller)
    {
        if (controller.IsTowerPart)
        {
            RemoveCubeFromTower(controller);
        }

        SetStackRaycast(false);
    }

    private void HandleDragEnded(CubeController controller)
    {
        SetStackRaycast(true);
    } 

    private ICubePlacementRule CreateRule(PlacementRuleType type)
    {
        switch (type)
        {
            case PlacementRuleType.SameColor:
                return new SameColorRule();

            default:
                return new AnyCubeRule();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        CubeController cube = eventData.pointerDrag?.GetComponent<CubeController>();
        if (cube == null) return;

        if (cube.IsTowerPart)
        {
            cube.PlayFailAnimation();
            return;
        }
        
        CubeModel cubeModel = cube.CubeModel;

        bool canPlace;

        if (_cubes.Count == 0)
        {
            canPlace =
                _firstRule.CanPlaceOnOther(cubeModel, null) &&
                IsCubeOverTarget(firstCubeZone, cube.View.RectTransform);
        }
        else
        {
            CubeController topCube = _cubes[^1];
            CubeModel topModel = topCube.CubeModel;

            canPlace =
                _placementRule.CanPlaceOnOther(cubeModel, topModel) &&
                IsCubeOverStack(cube.View.RectTransform);
        }

        if (!canPlace)
        {
            cube.PlayFailAnimation();
            return;
        }

        if (_cubes.Count == 0)
            PlaceFirstCube(cube);
        else
            PlaceOnTop(cube);
    }

    private bool IsCubeOverStack(RectTransform draggedCube)
    {
        Rect draggedRect = GetWorldRect(draggedCube);

        foreach (CubeController placedCube in _cubes)
        {
            Rect targetRect = GetWorldRect(placedCube.View.RectTransform);
            //Не проверяем взятый куб          
            if (targetRect == draggedRect)
                continue;

            if (draggedRect.Overlaps(targetRect))
                return true;
        }

        return false;
    }
    private void SetStackRaycast(bool value)
    {
        foreach (CubeController cube in _cubes)
        {
            cube.View.BlockRaycast(value);
        }
    }

    private Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        return new Rect(
            corners[0].x,
            corners[0].y,
            corners[2].x - corners[0].x,
            corners[2].y - corners[0].y
        );
    }
    /// <summary>
    /// Постановка первого куба башни
    /// </summary>
    /// <param name="cube"></param>
    private void PlaceFirstCube(CubeController cube)
    {
        RectTransform rect = cube.View.RectTransform;
        rect.SetParent(stackRoot, true);

        float width = cube.CubeModel.Size.x;
        float targetX = rect.anchoredPosition.x;
        float targetY = rect.anchoredPosition.y;

        // Корректируем, чтобы куб полностью помещался
        targetX = CorrectCubePositionInsideStack(targetX, width);

        Vector2 targetAnchoredPos = new Vector2(targetX, targetY);
        rect.anchoredPosition = targetAnchoredPos;
        notificationController.NotifyCubePlaced();
        cube.MarkAsTowerPart();
        _cubes.Add(cube);
        AutoSaveTower();
    }

    /// <summary>
    /// Проверка на попадание куба в края зоны
    /// </summary>
    /// <param name="targetX"></param>
    /// <param name="cubeWidth"></param>
    /// <returns></returns>
    private float CorrectCubePositionInsideStack(float targetX, float cubeWidth)
    {
        float halfCube = cubeWidth * 0.5f;
        float rightBorder = stackRoot.rect.width;
        float leftBorder = 0;

        float leftCubeEdge = targetX - halfCube;
        float rightCubeEdge = targetX + halfCube;

        // Если левый край вышел за границу
        if (leftCubeEdge < leftBorder)
        {
            targetX += (leftBorder - leftCubeEdge); // двигаем вправо на недостающую дистанцию
        }
        // Если правый край вышел за границу
        else if (rightCubeEdge > rightBorder)
        {
            targetX -= (rightCubeEdge - rightBorder); // двигаем влево
        }

        return targetX;
    }

    /// <summary>
    /// Постановка куба в башню
    /// </summary>
    /// <param name="cube"></param>
    private void PlaceOnTop(CubeController cube)
    {
        RectTransform rect = cube.View.RectTransform;

        float width = cube.CubeModel.Size.x;
        float cubeHeight = cube.CubeModel.Size.y;

        float maxOffset = width * maxOffsetPercent;
        float randomOffset = Random.Range(-maxOffset, maxOffset);

        

        float baseX = 0f;
        float baseY = 0f;

        float targetX = baseX + randomOffset;

        // проверяем чтобы куб полностью помещался
        targetX = ClampCubeFullyInsideStack(targetX, width);

        if (_cubes.Count > 0)
        {
            RectTransform lastRect = _cubes[^1].View.RectTransform;
            baseX = lastRect.anchoredPosition.x;
            baseY = lastRect.anchoredPosition.y;
        }

        _currentOffsetX = baseX + randomOffset;
        _currentOffsetY = baseY + cubeHeight;

        _currentOffsetX = CorrectCubePositionInsideStack(_currentOffsetX, width);

        //Проверка на выход за пределы экрана
        float towerTopY = 0;
        if (_cubes.Count > 0)
{
            RectTransform lastRect = _cubes[^1].View.RectTransform;
            float lastHeight = _cubes[^1].CubeModel.Size.y;
            towerTopY = lastRect.anchoredPosition.y + lastHeight / 2;
        }
     
        float maxHeight = stackRoot.rect.height;

        if (towerTopY > maxHeight)
        {
            cube.PlayFailAnimation();
            notificationController.NotifyMaxHeight();
            return;
        }

        Vector2 targetAnchoredPos = new Vector2(
            _currentOffsetX,
            _currentOffsetY
        );

        // ставим в башню
        rect.SetParent(stackRoot, true);
        cube.PlayJumpAnimation(targetAnchoredPos);
        notificationController.NotifyCubeStacked();
        AutoSaveTower();
        _cubes.Add(cube);
        cube.MarkAsTowerPart();
    }

    public void RemoveCubeFromTower(CubeController cube)
    {
        int removedIndex = _cubes.IndexOf(cube);
        if (removedIndex < 0)
            return;

        _cubes.RemoveAt(removedIndex);
        notificationController.NotifyCubeRemoved();
        AnimateFallDownFromIndex(removedIndex);
        AutoSaveTower();
    }

    private void AnimateFallDownFromIndex(int startIndex)
    {
        if (_cubes.Count == 0)
            return;

        float cubeHeight = _cubes[0].CubeModel.Size.y;

        for (int i = startIndex; i < _cubes.Count; i++)
        {
            RectTransform rect = _cubes[i].View.RectTransform;

            float targetY = rect.anchoredPosition.y - cubeHeight;

            rect.DOAnchorPosY(targetY, 0.25f)
                .SetEase(Ease.OutBounce)
                .Play();
        }
        AutoSaveTower();
    }

    private bool IsCubeOverTarget(RectTransform target, RectTransform dragged)
    {
        Vector3 worldCenter = dragged.TransformPoint(dragged.rect.center);

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            null,
            worldCenter
        );

        return RectTransformUtility.RectangleContainsScreenPoint(
            target,
            screenPoint,
            null
        );
    }
    private void AutoSaveTower()
    {
        TowerSaveData data = GetTowerSaveData();
        TowerSaveSystem.SaveTower(data);
    }

    private float ClampCubeFullyInsideStack(float targetX, float cubeWidth)
    {
        float halfCube = cubeWidth * 0.5f;
        float halfStack = stackRoot.rect.width * 0.5f;
        // минимальный X, чтобы левый край куба не вышел
        float minX = -halfStack + halfCube;
        // максимальный X, чтобы правый край куба не вышел
        float maxX = halfStack - halfCube;

        return Mathf.Clamp(targetX, minX, maxX);
    }

    public TowerSaveData GetTowerSaveData()
    {
        TowerSaveData data = new TowerSaveData();

        foreach (var cube in _cubes)
        {
            CubeSaveData cubeData = new CubeSaveData
            {
                CubeId = cube.CubeModel.Config.Id,
                PosX = cube.View.RectTransform.anchoredPosition.x,
                PosY = cube.View.RectTransform.anchoredPosition.y
            };
            data.Cubes.Add(cubeData);
        }

        return data;
    }

    public void LoadTowerFromData(TowerSaveData data, CubeFactory cubeFactory)
    {
        foreach (CubeController cube in _cubes)
            Destroy(cube.gameObject);

        _cubes.Clear();

        foreach (CubeSaveData cubeData in data.Cubes)
        {
            CubeController cube = cubeFactory.CreateById(cubeData.CubeId, stackRoot, true);
            if (cube == null)
                continue;

            RectTransform rect = cube.View.RectTransform;
            rect.anchoredPosition = new Vector2(cubeData.PosX, cubeData.PosY);

            cube.MarkAsTowerPart();
            _cubes.Add(cube);
        }
    }

    private void OnApplicationQuit()
    {
        AutoSaveTower();
    }
}