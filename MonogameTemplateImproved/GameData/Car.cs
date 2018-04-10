public class Car : SpriteBase
{
    public Car()
    {
    }

    public override SpriteType GetSpriteType()
    {
        return SpriteType.Car;
    }
    public override bool IsMovable()
    {
        return true;
    }
}
