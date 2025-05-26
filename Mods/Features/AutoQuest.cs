using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace eradev.dragoncliff.Mods.Features
{
    internal class AutoQuest
    {
        private const string FeatureName = nameof(AutoQuest);

        private static ConfigEntry<bool> _isEnabled;

        private static AutoAdventureState _currentState = AutoAdventureState.Finished;

        public static void Register(ConfigFile config)
        {
            _isEnabled = config.Bind(FeatureName, "enabled", false, "Enable auto quest");

            _isEnabled.SettingChanged += OnSettingsChanged;

            DragonCliffPlugin.UpdateEvt += OnUpdate;
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                _isEnabled.Value = !_isEnabled.Value;
            }
        }

        private static void OnSettingsChanged(object sender, EventArgs e)
        {
            if (!_isEnabled.Value)
            {
                DragonCliffPlugin.Log.LogDebug("[AutoQuest] Disabled");

                return;
            }

            DragonCliffPlugin.Log.LogDebug("[AutoQuest] Enabled");

            _currentState = GameWorld.instance.AdventureInProgress() || BattleManager.instance.IsInCombat
                ? AutoAdventureState.InBattle
                : AutoAdventureState.Finished;

            SearchAndStart();

            // sourceUnit.BattleEffects.OfType<FreeCastEffect>().Any<FreeCastEffect>()
        }

        private static void SearchAndStart()
        {
            if (!_isEnabled.Value ||
                    GameWorld.instance == null ||
                    GameWorld.instance.AdventureInProgress() ||
                    BattleManager.instance == null ||
                    BattleManager.instance.IsInCombat ||
                    _currentState != AutoAdventureState.Finished)
            {
                return;
            }

            var currentQuests = GameWorld.instance.PlayerProfile.GetProgress().Quests;

            var completedQuests = currentQuests.Where(x => x.Completed && !x.Rewarded).ToList();

            foreach (var completedQuest in completedQuests)
            {
                GameWorld.instance.PlayerProfile.CompleteQuest(completedQuest);
            }

            var validQuests = currentQuests
                .Where(x =>
                    x.QuestRequirements.Any(y => y.CorrespondingQuestRequirementType
                        is QuestRequirementType.DungeonCompletion or QuestRequirementType.CustomizedDungeonHuntRequirement) &&
                    !x.HasExpired() &&
                    !x.Completed
                )
                .OrderBy(x => x.EndingGameDay)
                .ToList();

            //.OrderByDescending(x => x.EndingGameDay.HasValue)
            // .ThenBy(x => x.EndingGameDay

            if (validQuests.Count == 0)
            {
                return;
            }

            var nextAdventureFound = false;
            var adventureType = AdventureType.None;
            var level = -1;

            foreach (var validQuest in validQuests)
            {
                foreach (var questRequirement in validQuest.QuestRequirements)
                {
                    if (questRequirement is DungeonCompletionRequirementLogic { fullFilled: false } dungeonCompletionRequirementLogic)
                    {
                        adventureType = dungeonCompletionRequirementLogic.DungeonType;
                        level = dungeonCompletionRequirementLogic.LevelNumber;
                        nextAdventureFound = true;

                        break;
                    }

                    if (questRequirement is DungeonExplorationRequirementLogic { fullFilled: false } dungeonExplorationRequirementLogic)
                    {
                        adventureType = dungeonExplorationRequirementLogic.DungeonType;
                        level = dungeonExplorationRequirementLogic.LevelNumber;
                        nextAdventureFound = true;

                        break;
                    }

                    if (questRequirement is CustomizedDungeonThroughRequirementLogic { fullfilled: false } customizedDungeonThroughRequirementLogic)
                    {
                        adventureType = customizedDungeonThroughRequirementLogic.DungeonType;
                        level = customizedDungeonThroughRequirementLogic.Configuration.LevelNumber;
                        nextAdventureFound = true;

                        break;
                    }
                }

                if (nextAdventureFound)
                {
                    try
                    {
                        level = Math.Min(adventureType.GetMaxVisibleLevel(), level);
                        DragonCliffPlugin.Log.LogDebug($"[AutoQuest] New adventure found: {adventureType} ({level})");
                    }
                    catch
                    {
                        DragonCliffPlugin.Log.LogDebug($"[AutoQuest] New adventure found: {adventureType} (Special)");
                    }

                    break;
                }
            }

            if (!nextAdventureFound)
            {
                DragonCliffPlugin.Log.LogDebug("[AutoQuest] No valid adventure found.");

                return;
            }

            var worldMap = TownManager.Instance.Ui.WorldMap;

            if (adventureType is AdventureType.TwistedPalace or AdventureType.ShadowPath)
            {
                adventureType = AdventureType.Special;
            }

            GameWorld.instance.PlayerProfile.BattleTeams.ForEach(x => x.AutoUseTactic = true);
            worldMap.SelectBattleTeam(0);

            worldMap.SelectMap(adventureType);
            worldMap.SelectLevelFromChessLevelItem(adventureType, level);
            _currentState = AutoAdventureState.InBattle;
        }

        [HarmonyPatch(typeof(QuestMenuController), nameof(QuestMenuController.TryUpdate))]
        public class QuestMenuControllerTryUpdatePatch
        {
            public static void Prefix()
            {
                SearchAndStart();
            }
        }

        [HarmonyPatch(typeof(AutoAdventureController), "Update")]
        public class AutoAdventureControllerUpdatePatch
        {
            public static void Postfix(AutoAdventureController __instance, AutoAdventureState ____currentState)
            {
                if (!_isEnabled.Value)
                {
                    return;
                }

                if (__instance.AutoAdventure.IsOn && ____currentState == AutoAdventureState.Finished)
                {
                    __instance.StopAutoAdventure();
                    _currentState = AutoAdventureState.Finished;

                    return;
                }

                switch (_currentState)
                {
                    case AutoAdventureState.WaitToOpenChest:
                        BattleManager.instance.Spawner.AutoSelectChest();
                        _currentState = AutoAdventureState.OnRewardPanel;

                        break;

                    case AutoAdventureState.OnRewardPanel:
                        if (BattleManager.instance.RewardPanel.isActiveAndEnabled)
                        {
                            BattleManager.instance.CompeletionPanelConfirmButton();
                            _currentState = AutoAdventureState.Finished;
                        }

                        break;
                }
            }
        }

        [HarmonyPatch(typeof(AutoAdventureController), nameof(AutoAdventureController.WaitToOpenChest))]
        public static class AutoAdventureControllerWaitToOpenChestPatch
        {
            public static void Postfix()
            {
                _currentState = AutoAdventureState.WaitToOpenChest;
            }
        }

        [HarmonyPatch(typeof(AutoAdventureController), nameof(AutoAdventureController.AdventureFailed))]
        public static class AutoAdventureControllerAdventureFailedPatch
        {
            public static void Postfix(AutoAdventureController __instance)
            {
                if (_isEnabled.Value)
                {
                    DragonCliffPlugin.Log.LogDebug("[AutoQuest] Expedition failed. Stopping AutoQuest.");

                    _isEnabled.Value = false;
                }

                _currentState = AutoAdventureState.Finished;
            }
        }
    }
}
