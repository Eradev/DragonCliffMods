using BepInEx.Configuration;
using HarmonyLib;

namespace eradev.dragoncliff.Mods.Features
{
    internal class AlwaysDropInvitation
    {
        private const string FeatureName = nameof(AlwaysDropInvitation);

        private static ConfigEntry<bool> _isEnabled;

        public static void Register(ConfigFile config)
        {
            _isEnabled = config.Bind(FeatureName, "enabled", false, "Enable always drop invitation");
        }

        [HarmonyPatch(typeof(BossUnitConfigurationBase), "GetCommonInvitationDropRate")]
        public class BossUnitConfigurationBaseGetCommonInvitationDropRatePatch
        {
            public static void Postfix(ref double __result)
            {
                if (!_isEnabled.Value)
                {
                    return;

                }
                __result = 1d;
            }
        }

        [HarmonyPatch(typeof(BossUnitConfigurationBase), "GetRareInvitationDropRate")]
        public class BossUnitConfigurationBaseGetRareInvitationDropRatePatch
        {
            public static void Postfix(ref double __result)
            {
                if (!_isEnabled.Value)
                {
                    return;

                }
                __result = 1d;
            }
        }

        [HarmonyPatch(typeof(BossUnitConfigurationBase), "GetEpicInvitationDropRate")]
        public class BossUnitConfigurationBaseGetEpicInvitationDropRatePatch
        {
            public static void Postfix(ref double __result)
            {
                if (!_isEnabled.Value)
                {
                    return;

                }
                __result = 1d;
            }
        }
    }
}
