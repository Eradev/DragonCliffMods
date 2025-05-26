using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;

namespace eradev.dragoncliff.Mods.Features
{
    internal class FreeEndlessDungeon
    {
        private const string FeatureName = nameof(FreeEndlessDungeon);

        private static ConfigEntry<bool> _isEnabled;

        public static void Register(ConfigFile config)
        {
            _isEnabled = config.Bind(FeatureName, "enabled", false, "Enable free endless dungeon");
        }

        /*[HarmonyPatch(typeof(EndlessDungeonResolver), nameof(EndlessDungeonResolver.CanBeResolved))]
        public class EndlessDungeonResolverCanBeResolvedPatch
        {
            public static void Postfix(ref bool __result)
            {
                if (!_isEnabled.Value)
                {
                    return;

                }
                __result = true;
            }
        }*/

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.BatchResourceUpdate))]
        public class PlayerProfileBatchResourceUpdatePatch
        {
            public static void Prefix(ref List<ResourceUpdate> changes)
            {
                if (!_isEnabled.Value)
                {
                    return;

                }

                changes.RemoveAll(x => x.ResourceType == ResourceType.MysticKey && x.ChangeAmount < 0);
            }
        }
    }
}
