using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;

namespace eradev.dragoncliff.Mods.Features
{
    internal class AlwaysAncientGradeGeneration
    {
        private const string FeatureName = nameof(AlwaysAncientGradeGeneration);

        private static bool _disableNotifications;
        private static readonly List<string> _staticRandom = [];

        private static ConfigEntry<bool> _isEnabled;

        public static void Register(ConfigFile config)
        {
            _isEnabled = config.Bind(FeatureName, "enabled", false, "Enable always ancient grade item generation");
        }

        [HarmonyPatch(typeof(GenerationDistribution), nameof(GenerationDistribution.GetGrade))]
        public class GenerationDistributionGetGradePatch
        {
            public static void Postfix(ref QualityGrade __result)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                __result = QualityGrade.Ancient;
            }
        }

        [HarmonyPatch(typeof(UnityEngine.Random), nameof(UnityEngine.Random.Range), typeof(float), typeof(float))]
        public class RandomRangePatch
        {
            public static void Postfix(ref float __result, float max)
            {
                if (!_isEnabled.Value || _staticRandom.Count == 0)
                {
                    return;
                }

                __result = max;
            }
        }

        [HarmonyPatch(typeof(ItemExtensions), nameof(ItemExtensions.ItemGenerate))]
        public class ItemExtensionsItemGeneratePatch
        {
            public static void Prefix(ref ItemGenerationQuality quality)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                quality = ItemGenerationQuality.CreateStar();

                _staticRandom.Add(nameof(ItemExtensionsItemGeneratePatch));
            }

            public static void Postfix()
            {
                _staticRandom.Remove(nameof(ItemExtensionsItemGeneratePatch));
            }
        }

        [HarmonyPatch(typeof(ItemExtensions), "GenerateScroll")]
        public class ItemExtensionsGenerateScrollPatch
        {
            public static void Prefix(ref bool isStar)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                isStar = true;

                _staticRandom.Add(nameof(ItemExtensionsGenerateScrollPatch));
            }

            public static void Postfix()
            {
                _staticRandom.Remove(nameof(ItemExtensionsGenerateScrollPatch));
            }
        }

        [HarmonyPatch(typeof(ItemExtensions), nameof(ItemExtensions.RandomGenerateAttributeModifier_Enchanting))]
        public class ItemExtensionsRandomGenerateAttributeModifierEnchantingPatch
        {
            public static void Prefix()
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                _staticRandom.Add(nameof(ItemExtensionsRandomGenerateAttributeModifierEnchantingPatch));
            }

            public static void Postfix()
            {
                _staticRandom.Remove(nameof(ItemExtensionsRandomGenerateAttributeModifierEnchantingPatch));
            }
        }

        [HarmonyPatch(typeof(ItemExtensions), nameof(ItemExtensions.RandomGenerateAttributeModifier_ItemGeneration))]
        public class ItemExtensionsRandomGenerateAttributeModifierItemGenerationPatch
        {
            public static void Prefix()
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                _staticRandom.Add(nameof(ItemExtensionsRandomGenerateAttributeModifierItemGenerationPatch));
            }

            public static void Postfix()
            {
                _staticRandom.Remove(nameof(ItemExtensionsRandomGenerateAttributeModifierItemGenerationPatch));
            }
        }

        [HarmonyPatch(typeof(ItemExtensions), nameof(ItemExtensions.RandomGenerateAttributeModifier_Reforging))]
        public class ItemExtensionsRandomGenerateAttributeModifierReforgingPatch
        {
            public static void Prefix()
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                _staticRandom.Add(nameof(ItemExtensionsRandomGenerateAttributeModifierReforgingPatch));
            }

            public static void Postfix()
            {
                _staticRandom.Remove(nameof(ItemExtensionsRandomGenerateAttributeModifierReforgingPatch));
            }

            // Disable flying text
            [HarmonyPatch(typeof(ProductionBuildingController), "CompleteRecipe")]
            public class ProductionBuildingControllerCompleteRecipePatch
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
}