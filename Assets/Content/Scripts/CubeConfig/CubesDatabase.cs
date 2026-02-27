using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Cubes Database")]
public class CubesDatabase : ScriptableObject, ICubesDataSource
{
    [SerializeField]
    private List<CubeConfig> cubes;

    public List<CubeConfig> GetCubes()
    {
        return cubes;
    }
}