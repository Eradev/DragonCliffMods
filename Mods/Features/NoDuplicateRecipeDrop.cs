using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace eradev.dragoncliff.Mods.Features
{
    internal class NoDuplicateRecipeDrop
    {
        private const string FeatureName = nameof(NoDuplicateRecipeDrop);

        private static ConfigEntry<bool> _isEnabled;

        public static void Register(ConfigFile config)
        {
            _isEnabled = config.Bind(FeatureName, "enabled", false, "Enable no duplicate recipe drops");
        }

        [HarmonyPatch(typeof(DropTable), "GetPotentialDrops")]
        public class DropTableGetPotentialDropsPatch
        {
            public static void Postfix(ref List<DropTableParameter> __result)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                var productionBuildings = GameWorld.instance.PlayerProfile.Buildings
                    .Where(x => x.Value is ProductionBuildingProfile)
                    .Select(x => x.Value as ProductionBuildingProfile)
                    .ToList();

                var unlockedRecipes = productionBuildings
                    .SelectMany(x => x.GetCapableRecipes())
                    .Select(x => x.Recipe.RecipeName)
                    .ToList();

                __result.RemoveAll(x => unlockedRecipes.Contains(x.ResourceType) && x.ResourceType.GetResourceCategory() == ResourceCategory.ProductionRecipe);
            }
        }

        [HarmonyPatch(typeof(DifficultyLevelMeasurement), nameof(DifficultyLevelMeasurement.GetStandardDropableCompleteTable))]
        public class DifficultyLevelMeasurementGetStandardDropableCompleteTablePatch
        {
            public static void Postfix(ref DropTable __result)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                var productionBuildings = GameWorld.instance.PlayerProfile.Buildings
                    .Where(x => x.Value is ProductionBuildingProfile)
                    .Select(x => x.Value as ProductionBuildingProfile)
                    .ToList();

                var unlockedRecipes = productionBuildings
                    .SelectMany(x => x.GetCapableRecipes())
                    .Select(x => x.Recipe.RecipeName)
                    .ToList();

                __result.DropTableParameters.RemoveAll(x => unlockedRecipes.Contains(x.ResourceType) && x.ResourceType.GetResourceCategory() == ResourceCategory.ProductionRecipe);
            }
        }
    }
}
