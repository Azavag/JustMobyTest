using UnityEngine;
using UnityEngine.EventSystems;

public class WrongDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        CubeController cube = eventData.pointerDrag?.GetComponent<CubeController>();
        if (cube == null)
            return;

        cube.PlayFailAnimation();
    }
}