using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void ToTreeAttributes(ITreeAttribute tree, WearAndTearPartProps props)
        {
            //Skip if default configuration
            if(ProtectionModifier == 1f && DecayModifier == 1f) return;

            var bonusTree = tree.GetOrAddTreeAttribute("WearAndTear-Bonuses").GetOrAddTreeAttribute(props.Name);
            
            bonusTree.SetFloat(nameof(DecayModifier), DecayModifier);
            bonusTree.SetFloat(nameof(ProtectionModifier), ProtectionModifier);
        }

        public void FromTreeAttributes(ITreeAttribute tree, WearAndTearPartProps props)
        {
            var bonusTree = tree.GetTreeAttribute("WearAndTear-Bonuses")?.GetTreeAttribute(props.Name);
            if(bonusTree == null) return;
            
            DecayModifier = bonusTree.GetFloat(nameof(DecayModifier), DecayModifier);
            ProtectionModifier = bonusTree.GetFloat(nameof(ProtectionModifier), ProtectionModifier);
        }

        public void UpdateForRepair(IWearAndTearPart part, ICoreAPI api, IPlayer player)
        {
            //Reset values to default
            DecayModifier = 1f;
            ProtectionModifier = 1f;
            if(part.Props.Name == "Wax")
            {
                SkillsAndAbilities.ApplyButterFingerBonus(this, api, player);
            }

            if (part.Props.Name.Contains("reinforcement", StringComparison.OrdinalIgnoreCase))
            {
                SkillsAndAbilities.ApplyReinforcerBonus(this, api, player);
            }
        }
    }
}
