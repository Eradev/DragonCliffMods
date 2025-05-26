using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace eradev.dragoncliff.Mods.Features
{
    internal class MissionaryAI
    {
        private const string FeatureName = nameof(MissionaryAI);

        private static ConfigEntry<bool> _isEnabled;
        private static ConfigEntry<bool> _autoCastFreeSkillsEnabled;
        private static ConfigEntry<bool> _autoClearDebuffEnabled;
        private static ConfigEntry<int> _minNumDebuffs;

        public static void Register(ConfigFile config)
        {
            _isEnabled = config.Bind(FeatureName, "enabled", false, "Enable missionary AI");

            _autoCastFreeSkillsEnabled = config.Bind(FeatureName, "autoCastFreeSkills", false, "Auto cast free skills");

            _autoClearDebuffEnabled = config.Bind(FeatureName, "autoCastSpellOfHoliness", false, "Auto cast Spell of Holiness");
            _minNumDebuffs = config.Bind(FeatureName, "minNumDebuffs", 8, "Number of debuffs before auto casting Spell of Holiness");
        }

        [HarmonyPatch(typeof(BattleEncounter), nameof(BattleEncounter.PerUpdateProcess))]
        public class BattleEncounterPerUpdateProcessPatch
        {
            public static void Postfix(BattleEncounter __instance)
            {
                if (!_isEnabled.Value ||
                    __instance.IsCompleted)
                {
                    return;
                }

                var aliveUnits = __instance.PlayerUnits.Where(x => x.IsAliveInBattle()).ToList();

                if (_autoCastFreeSkillsEnabled.Value)
                {
                    CastFreeSkills(aliveUnits);
                }

                if (_autoClearDebuffEnabled.Value)
                {
                    ClearDebuffs(aliveUnits);
                }
            }
        }
        private static void ClearDebuffs(List<IBattleUnit> aliveUnits)
        {
            var availableMissionaries = aliveUnits
                .Where(x => x.GetUnitType() == UnitClass.Missionary &&
                            x.Skills.Any(y => y.GetSkillLogic() is ActiveSkillLogicBase activeSkill && activeSkill.IsAutoCastable(y)))
                .ToList();

            if (availableMissionaries.Count == 0)
            {
                return;
            }

            var negativeDebuffsCount = aliveUnits
                .SelectMany(x => x.BattleEffects
                    .Where(y => y.BattleEffectNatureForWearer == BattleEffectNature.Negative &&
                                y.CanBeDispersed))
                .Count();

            if (negativeDebuffsCount < _minNumDebuffs.Value)
            {
                return;
            }

            var holiness = availableMissionaries.First().Skills
                .First(y => y.GetSkillLogic() is ActiveSkillLogicBase);

            DragonCliffPlugin.Log.LogDebug($"[{FeatureName}] Auto casting {holiness.Skill.SkillType} to clear {negativeDebuffsCount} debuffs");

            try
            {
                // ReSharper disable once GenericEnumeratorNotDisposed
                var enumerator2 = ((ActiveSkillLogicBase)holiness.GetSkillLogic())
                    .AutoCast(CandidateOrderringMetric.Random, OrderingType.Asc, holiness)
                    .GetEnumerator();

                BattleManager.instance.StartCoroutine(enumerator2);
            }
            catch
            {
                DragonCliffPlugin.Log.LogError($"[{FeatureName}] Failed to cast {holiness.Skill.SkillType}");
            }
        }

        private static void CastFreeSkills(List<IBattleUnit> aliveUnits)
        {
            foreach (var currentSkill in from playerUnit in aliveUnits
                                         where playerUnit.BattleEffects.OfType<FreeCastEffect>().Any()
                                         from currentSkill in playerUnit.Skills
                                         select currentSkill)
            {
                if (currentSkill.Skill.CommandType != SkillCommandType.Active ||
                    currentSkill.Skill.GetSkillLogic() is not ActiveSkillLogicBase activeSkill ||
                    currentSkill.InProgress ||
                    !activeSkill.IsAutoCastable(currentSkill))
                {
                    continue;
                }

                DragonCliffPlugin.Log.LogDebug($"[{FeatureName}] Auto casting free skill {currentSkill.Skill.SkillType}");

                try
                {
                    // ReSharper disable once GenericEnumeratorNotDisposed
                    var enumerator = activeSkill.AutoCast(CandidateOrderringMetric.Random, OrderingType.Asc, currentSkill).GetEnumerator();

                    BattleManager.instance.StartCoroutine(enumerator);
                }
                catch
                {
                    DragonCliffPlugin.Log.LogError($"[{FeatureName}] Failed to cast {currentSkill.Skill.SkillType}");
                }
            }
        }
    }
}
