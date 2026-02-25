using UnityEngine;

public class CubesPanelController : MonoBehaviour
{
    [SerializeField]
    private CubeFactory _factory;
    [SerializeField]
    private Transform _container;
    [SerializeField]
    private CubesDatabase _database;

    private void Start()
    {
        foreach (CubeConfig config in _database.Cubes)
        {
            _factory.Create(config, _container);
        }
    }
}