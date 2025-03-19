using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace WearAndTear.Code.Commands
{
    public static class Commands
    {
        //TODO command for cleaning durability off item
        public static void Register(ICoreServerAPI api) => api.ChatCommands
            .Create("WearAndTear")
                .WithDescription("Main Command prefix for WearAndTear")
                .RequiresPrivilege(Privilege.chat)
                .HandleWith(_ => TextCommandResult.Success($"WearAndTear version: {api.ModLoader.GetModSystem<WearAndTearModSystem>().Mod.Info.Version}"))
            .RegisterDurabilityCommands();
    }
}
