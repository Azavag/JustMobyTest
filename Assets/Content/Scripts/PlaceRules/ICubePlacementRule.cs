
public interface ICubePlacementRule
{
    bool CanPlaceOnOther(CubeModel cube, CubeModel target = null);
}

// Пример простой реализации — любой кубик можно ставить
public class AnyCubeRule : ICubePlacementRule
{
    public bool CanPlaceOnOther(CubeModel cube, CubeModel target) => true;
}

public class FirstCubeRule : ICubePlacementRule
{
    public bool CanPlaceOnOther(CubeModel cube, CubeModel target)
    {
        return target == null;
    }
}

public class SameColorRule : ICubePlacementRule
{
    public bool CanPlaceOnOther(CubeModel cube, CubeModel target)
    {
        if (target == null) 
            return true;

        return cube.Config == target.Config;
    }
}