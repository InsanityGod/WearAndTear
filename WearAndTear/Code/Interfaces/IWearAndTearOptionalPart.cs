namespace WearAndTear.Code.Interfaces
{
    public interface IWearAndTearOptionalPart : IWearAndTearPart
    {
        bool IsPresent => Durability != 0;
    }
}