using BepInEx.Configuration;
using HarmonyLib;

namespace eradev.dragoncliff.Mods.Features
{
    internal class AlwaysAncientGradeResidentRecruit
    {
        private const string FeatureName = nameof(AlwaysAncientGradeResidentRecruit);

        private static bool _disableNotifications;

        private static ConfigEntry<bool> _isEnabled;

        public static void Register(ConfigFile config)
        {
            _isEnabled = config.Bind(FeatureName, "enabled", false, "Enable always ancient grade residents");
        }

        [HarmonyPatch(typeof(ResidentsExtensions), nameof(ResidentsExtensions.CreateResidentByCoeff))]
        public class ResidentsExtensionsCreateResidentByCoeffPatch
        {
            public static void Prefix(ref double determinedQualityCoef)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                determinedQualityCoef = 1d;
            }
        }

        // Disable SFX
        [HarmonyPatch(typeof(CityTownController), nameof(CityTownController.PlayGoodResidentClip))]
        public class CityTownControllerPlayGoodResidentClipPatch
        {
            public static bool Prefix()
            {
                return !_isEnabled.Value;
            }
        }

        // Disable flying text
        [HarmonyPatch(typeof(TownManager), "ResidentCandidateAdded")]
        public class TownManagerResidentCandidateAddedPatch
        {
            public static void Prefix()
            {
                _disableNotifications = true;
            }

            public static void Postfix()
            {
                _disableNotifications = false;
            }
        }

        [HarmonyPatch(typeof(SceneExtention), nameof(SceneExtention.DisplyMovingNotification))]
        public class SceneExtentionDisplyMovingNotificationPatch
        {
            public static bool Prefix()
            {
                return !_isEnabled.Value || !_disableNotifications;
            }
        }
    }
}
