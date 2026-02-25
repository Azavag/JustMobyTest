using Unity.VisualScripting;
using UnityEngine;

public class CubeFactory : MonoBehaviour
{
    [SerializeField]
    private CubeController cubePrefab;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private Transform dragLayer;

    public CubeController Create(CubeConfig config, Transform parent)
    {
        CubeController cube = Instantiate(cubePrefab, parent);

        cube.Initialize(config, canvas, this, dragLayer, false);
        return cube;
    }
}