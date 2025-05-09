using WearAndTear.Code.Enums;

namespace WearAndTear.Code.Extensions
{
    public static class EnumExtensions
    {
        public static bool IsFullfilled(this EXLibPrescenceRequirement requirement) => requirement switch
        {
            EXLibPrescenceRequirement.Disabled => false,
            EXLibPrescenceRequirement.OnlyWithXLib => WearAndTearModSystem.XlibEnabled,
            EXLibPrescenceRequirement.OnlyWithoutXLib => !WearAndTearModSystem.XlibEnabled,
            EXLibPrescenceRequirement.Irrelevant => true,
            _ => false,
        };
    }
}