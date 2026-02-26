using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("Zones")]
    [SerializeField] private RectTransform firstCubeZone;
    [SerializeField] private RectTransform stackRoot;

    [Header("Stack Settings")]
    [SerializeField] private float maxOffsetPercent = 0.5f;

    [Header("Rule Type")]
    [SerializeField] private PlacementRuleType ruleType;

    [SerializeField] private List<CubeController> _cubes = new();

    private ICubePlacementRule _firstRule;
    private ICubePlacementRule _placementRule;

    private float _currentOffsetX = 0f;
    private float _currentOffsetY = 0f;

    private void Awake()
    {
        _firstRule = new FirstCubeRule();
        _placementRule = CreateRule(ruleType);

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

    // ================= FIRST =================

    private void PlaceFirstCube(CubeController cube)
    {
        RectTransform rect = cube.View.RectTransform;
        rect.SetParent(stackRoot, true);
        cube.MarkAsTowerPart();
        _cubes.Add(cube);
    }

    // ================= STACK =================

    private void PlaceOnTop(CubeController cube)
    {
        Debug.Log("PlaceOnTop");

        RectTransform rect = cube.View.RectTransform;

        float width = cube.CubeModel.Size.x;
        float cubeHeight = cube.CubeModel.Size.y;

        float maxOffset = width * maxOffsetPercent;
        float randomOffset = Random.Range(-maxOffset, maxOffset);

        float baseX = 0f;
        float baseY = 0f;

        if (_cubes.Count > 0)
        {
            RectTransform lastRect = _cubes[^1].View.RectTransform;
            baseX = lastRect.anchoredPosition.x;
            baseY = lastRect.anchoredPosition.y;
        }

        _currentOffsetX = baseX + randomOffset;
        _currentOffsetY = baseY + cubeHeight;

        if (_currentOffsetY > Screen.currentResolution.height)
        {
            cube.PlayFailAnimation();
            return;
        }

        Vector2 targetAnchoredPos = new Vector2(
            _currentOffsetX,
            _currentOffsetY
        );

        // ставим в башню
        rect.SetParent(stackRoot, true);
        cube.PlayJumpAnimation(targetAnchoredPos);

        _cubes.Add(cube);
        cube.MarkAsTowerPart();
    }

    public void RemoveCubeFromTower(CubeController cube)
    {
        int removedIndex = _cubes.IndexOf(cube);
        if (removedIndex < 0)
            return;

        _cubes.RemoveAt(removedIndex);

        AnimateFallDownFromIndex(removedIndex);
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
}