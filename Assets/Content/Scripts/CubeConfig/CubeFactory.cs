using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CubeFactory : MonoBehaviour
{
    [SerializeField] private CubeController cubePrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform dragLayer;

    [SerializeField] private List<CubeConfig> cubeConfigs;

    private Dictionary<string, CubeConfig> _configLookup;

    private void Awake()
    {
        _configLookup = new Dictionary<string, CubeConfig>();
        foreach (CubeConfig config in cubeConfigs)
        {
            if (!_configLookup.ContainsKey(config.Id))
                _configLookup.Add(config.Id, config);
        }
    }

    
    public CubeController Create(CubeConfig config, Transform parent)
    {
        CubeController cube = Instantiate(cubePrefab, parent);
        cube.Initialize(config, canvas, this, dragLayer, false);
        cube.name = config.name;
        return cube;
    }

    // Новый метод — создание по ID
    public CubeController CreateById(string id, Transform parent, bool isRestored = false)
    {
        if (!_configLookup.TryGetValue(id, out CubeConfig config))
        {
            Debug.LogError($"CubeFactory: Config с ID {id} не найден!");
            return null;
        }

        CubeController cube = Create(config, parent);

        if (isRestored)
        {
            cube.IsTowerPart = true;
            cube.IsClone = true;
        }

        return cube;
    }
}