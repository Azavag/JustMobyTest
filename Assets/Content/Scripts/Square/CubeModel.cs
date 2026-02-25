using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CubeModel
{
    private Image _image;
    private Vector2 _size;
    private CubeConfig _cubeConfig;

    public Image Image => _image;
    public Vector2 Size => _size;
    public CubeConfig Config => _cubeConfig;

    public CubeModel(CubeConfig config)
    {
        _cubeConfig = config;
    }
}
