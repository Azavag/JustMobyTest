using UnityEngine;

[CreateAssetMenu(menuName = "Game/Cube Config")]
public class CubeConfig : ScriptableObject
{
    public string Id;
    public Sprite Sprite;
    public Vector2 Size = new Vector2(150, 150);
}