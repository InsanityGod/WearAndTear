using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using WearAndTear.Code.Behaviours;
using WearAndTear.Code.Interfaces;
using WearAndTear.Config.Props;

namespace WearAndTear.Code.XLib.Containers
{
    public class PartBonuses
    {
        public float ProtectionModifier = 1f;

        public float DecayModifier = 1f;

        public void ToTreeAttributes(ITreeAttribute tree, PartProps props)
        {
            //Skip if default configuration
            if (ProtectionModifier == 1f && DecayModifier == 1f) return;

            var bonusTree = tree.GetOrAddTreeAttribute("WearAndTear-Bonuses").GetOrAddTreeAttribute(props.Code);

            bonusTree.SetFloat(nameof(DecayModifier), DecayModifier);
            bonusTree.SetFloat(nameof(ProtectionModifier), ProtectionModifier);
        }

        public void FromTreeAttributes(ITreeAttribute tree, PartProps props)
        {
            var bonusTree = tree.GetTreeAttribute("WearAndTear-Bonuses")?.GetTreeAttribute(props.Code);
            if (bonusTree == null) return;

            DecayModifier = bonusTree.GetFloat(nameof(DecayModifier), DecayModifier);
            ProtectionModifier = bonusTree.GetFloat(nameof(ProtectionModifier), ProtectionModifier);
        }

        public void UpdateForRepair(Part part, ICoreAPI api, IPlayer player)
        {
            //Reset values to default
            DecayModifier = 1f;
            ProtectionModifier = 1f;
            if (part.Props.Code == "wearandtear:wax")
            {
                SkillsAndAbilities.ApplyButterFingerBonus(this, api, player);
            }

            if (part.Props.Code == "wearandtear:reinforcement")
            {
                SkillsAndAbilities.ApplyReinforcerBonus(this, api, player);
            }
        }
    }
}