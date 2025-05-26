using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;

namespace eradev.dragoncliff.Mods.Features
{
    internal class FreeCrafting
    {
        private const string FeatureName = nameof(FreeCrafting);

        private static ConfigEntry<bool> _isEnabled;

        public static void Register(ConfigFile config)
        {
            _isEnabled = config.Bind(FeatureName, "enabled", false, "Enable free crafting");
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.BatchResourceUpdate))]
        public class PlayerProfileBatchResourceUpdatePatch
        {
            public static void Prefix(ref List<ResourceUpdate> changes)
            {
                if (!_isEnabled.Value)
                {
                    return;

                }

                changes.RemoveAll(x => x.ResourceType.IsSocketBatcher() && x.ChangeAmount < 0);
            }
        }

        [HarmonyPatch(typeof(School), nameof(School.GetCostForCombineScroll))]
        public class SchoolGetCostForCombineScrollPatch
        {
            public static void Postfix(ref List<ResourceConsumptionRequirement> __result)
            {
                if (!_isEnabled.Value)
                {
                    return;

                }
                __result = [];
            }
        }

        [HarmonyPatch(typeof(Item), nameof(Item.GetEnchantingCost))]
        public class ItemGetEnchantingCostPatch
        {
            public static void Postfix(ref List<ResourceConsumptionRequirement> __result)
            {
                if (!_isEnabled.Value)
                {
                    return;

                }
                __result = [];
            }
        }

        [HarmonyPatch(typeof(Item), nameof(Item.GetReforgingCost))]
        public class ItemGetReforgingCostPatch
        {
            public static void Postfix(ref List<ResourceConsumptionRequirement> __result)
            {
                if (!_isEnabled.Value)
                {
                    return;

                }
                __result = [];
            }
        }

        [HarmonyPatch(typeof(Item), nameof(Item.ExtractGemCost))]
        public class ItemExtractGemCostPatch
        {
            public static void Postfix(ref int __result)
            {
                if (!_isEnabled.Value)
                {
                    return;

                }
                __result = 0;
            }
        }

        [HarmonyPatch(typeof(Item), nameof(Item.GetEffectEnchantingCost))]
        public class ItemGetEffectEnchantingCostPatch
        {
            public static void Postfix(ref List<ResourceConsumptionRequirement> __result)
            {
                if (!_isEnabled.Value)
                {
                    return;

                }
                __result = [];
            }
        }

        [HarmonyPatch(typeof(Item), nameof(Item.TeamSetUpgradeRequirements))]
        public class ItemTeamSetUpgradeRequirementsPatch
        {
            public static void Postfix(ref List<ResourceConsumptionRequirement> __result)
            {
                if (!_isEnabled.Value)
                {
                    return;

                }
                __result = [];
            }
        }
    }
}
