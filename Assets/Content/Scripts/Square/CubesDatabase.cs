using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Cubes Database")]
public class CubesDatabase : ScriptableObject
{
    public List<CubeConfig> Cubes;
}