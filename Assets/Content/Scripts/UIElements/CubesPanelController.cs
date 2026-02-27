using UnityEngine;

public class CubesPanelController : MonoBehaviour
{
    [SerializeField]
    private CubeFactory _factory;

    [SerializeField]
    private Transform _container;

    [SerializeField]
    private ScriptableObject _dataSourceObject; 

    private ICubesDataSource _dataSource;

    private void Awake()
    {
        _dataSource = _dataSourceObject as ICubesDataSource;

        if (_dataSource == null)
            Debug.LogError("DataSource does not implement ICubesDataSource");
    }

    private void Start()
    {
        foreach (CubeConfig config in _dataSource.GetCubes())
        {
            _factory.Create(config, _container);
        }
    }
}