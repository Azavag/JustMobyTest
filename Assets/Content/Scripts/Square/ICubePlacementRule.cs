
public interface ICubePlacementRule
{
    bool CanPlace(CubeModel cube, CubeModel target);
}

// Пример простой реализации — любой кубик можно ставить
public class AnyCubeRule : ICubePlacementRule
{
    public bool CanPlace(CubeModel cube, CubeModel target) => true;
}

public class SameColorRule : ICubePlacementRule
{
    public bool CanPlace(CubeModel cube, CubeModel target)
    {
        if (target == null) 
            return true;

        return cube.Config == target.Config;
    }
}