using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CubeSaveData
{
    public string CubeId;      // уникальный ID
    public Sprite CubeSprite;      // уникальный ID
    public float PosX;
    public float PosY;
}

[System.Serializable]
public class TowerSaveData
{
    public List<CubeSaveData> Cubes = new();
}