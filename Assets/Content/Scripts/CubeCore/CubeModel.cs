using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CubeModel
{
    private Sprite _sprite;
    private Vector2 _size;
    private CubeConfig _cubeConfig;

    public Sprite Sprite => _sprite;
    public Vector2 Size => _size;
    public CubeConfig Config => _cubeConfig;

    public CubeModel(CubeConfig config)
    {
        _cubeConfig = config;
        _size = config.Size;
        _sprite = config.Sprite;
    }
}
