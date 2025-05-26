using BepInEx.Configuration;
using HarmonyLib;

namespace eradev.dragoncliff.Mods.Features
{
    internal class AlwaysPerfectAdventurerRecruit
    {
        private const string FeatureName = nameof(AlwaysPerfectAdventurerRecruit);

        private static bool _ignore;
        private static bool _disableNotifications;
        private static bool _staticRandom;

        private static ConfigEntry<bool> _isEnabled;

        public static void Register(ConfigFile config)
        {
            _isEnabled = config.Bind(FeatureName, "enabled", false, "Enable always perfect adventurers");
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.InitPlayer))]
        public class PlayerProfileInitPlayerPatch
        {
            public static void Prefix()
            {
                _ignore = true;
            }

            public static void Postfix()
            {
                _ignore = false;
            }
        }

        [HarmonyPatch(typeof(UnitExtensions), "CreateAdventurer")]
        public class UnitExtensionsCreateAdventurerPatch
        {
            public static void Prefix(UnitClass @class, ref float quality, ref QualityGrade grade)
            {
                if (!_isEnabled.Value || _ignore)
                {
                    return;
                }

                quality = 7f;
                grade = QualityGrade.Ancient;

                _staticRandom = true;
            }

            public static void Postfix()
            {
                _staticRandom = false;
            }
        }

        [HarmonyPatch(typeof(UnityEngine.Random), nameof(UnityEngine.Random.Range), typeof(float), typeof(float))]
        public class RandomRangePatch
        {
            public static void Postfix(ref float __result, float max)
            {
                if (!_isEnabled.Value || !_staticRandom)
                {
                    return;
                }

                __result = max;
            }
        }

        // Disable flying text
        [HarmonyPatch(typeof(RecruitmentFacilityController), "Facility_CandidatesListUpdated")]
        public class RecruitmentFacilityControllerFacilityCandidatesListUpdatedPatch
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
