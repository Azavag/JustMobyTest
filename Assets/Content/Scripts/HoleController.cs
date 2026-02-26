using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class HoleController : MonoBehaviour, IDropHandler
{
    [Header("Hole Settings")]
    [SerializeField] private float fallDuration = 0.5f;    // Время падения куба внутрь
    [SerializeField] private float rotationAngle = 360f;   // Вращение по Z
    [SerializeField] private RectTransform holeRect;
    [SerializeField] private RectTransform holeMask;

    /// <summary>
    /// Проверяет, попал ли куб в овальную дыру
    /// </summary>
    /// <param name="cubeRect"></param>
    /// <returns></returns>
    private bool IsCubeOverHole(RectTransform cubeRect)
    {
        Vector2 localPoint;

        // Переводим мировую позицию куба в локальные координаты дыры
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            holeRect,
            cubeRect.position,
            null,
            out localPoint
        );

        float halfWidth = holeRect.rect.width * 0.5f;
        float halfHeight = holeRect.rect.height * 0.5f;

        float normalized =
            (localPoint.x * localPoint.x) / (halfWidth * halfWidth) +
            (localPoint.y * localPoint.y) / (halfHeight * halfHeight);
        Debug.Log(normalized);

        return normalized <= 2f;
    }

    /// <summary>
    /// Погружение куба внутрь дыры с вращением
    /// </summary>
    public void ConsumeCube(CubeController cube)
    {
        RectTransform cubeRect = cube.View.RectTransform;
        cube.View.BlockRaycast(false);

        // Делаем куб дочерним Hole, чтобы маска скрывала его
        cubeRect.SetParent(holeMask, true);

        // Определяем конечную позицию 
        Vector2 targetPos = cubeRect.anchoredPosition - new Vector2(0, 300);

        Sequence seq = DOTween.Sequence();
        seq.Append(cubeRect.DOAnchorPos(targetPos, fallDuration).SetEase(Ease.InQuad));
        seq.Join(cubeRect.DORotate(new Vector3(0, 0, rotationAngle), fallDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.InQuad));

        // После анимации деактивируем куб
        seq.OnComplete(() =>
        {
            cube.gameObject.SetActive(false);
            Destroy(cube.gameObject);
        }).Play();
    }

    /// <summary>
    /// IDropHandler.OnDrop — вызывается автоматически при отпускании куба над  дырой
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrop(PointerEventData eventData)
    {
        CubeController cube = eventData.pointerDrag?.GetComponent<CubeController>();
        if (cube == null) return;

        if (IsCubeOverHole(cube.View.RectTransform))
        {
            ConsumeCube(cube);
        }
        else
        {
            cube.PlayFailAnimation();
        }
    }
}