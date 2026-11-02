using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace HowToFish.SellMultiplier
{
    [BepInPlugin(Guid, "How to Fish - Sell Multiplier", "1.0.4")]
    public sealed class SellMultiplierPlugin : BaseUnityPlugin
    {
        public const string Guid = "local.howtofish.sellmultiplier";

        internal static ConfigEntry<int> SellMultiplier;
        internal static ConfigEntry<int> MaxPlayers;
        internal static BepInEx.Logging.ManualLogSource Log;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            SellMultiplier = Config.Bind(
                "Economy",
                "SellMultiplier",
                1000,
                "Multiplier applied when selling a fish. Set to 1 to disable.");

            MaxPlayers = Config.Bind(
                "Safety",
                "MaxPlayers",
                1,
                "Only apply the multiplier when no more than this many spawned Player objects are in the scene.");

            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(SellMultiplierPlugin).Assembly);
            Logger.LogInfo("How to Fish sell multiplier loaded. Multiplier=" + SellMultiplier.Value);
        }

        private void OnDestroy()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
            }
        }
    }

    internal static class SaleScope
    {
        [ThreadStatic]
        internal static bool Active;

        internal static void Begin(string source)
        {
            Active = false;

            int multiplier = SellMultiplierPlugin.SellMultiplier.Value;
            if (multiplier <= 1)
            {
                return;
            }

            try
            {
                int players = 0;
                Player[] candidates = UnityEngine.Object.FindObjectsOfType<Player>();
                for (int i = 0; i < candidates.Length; i++)
                {
                    Player player = candidates[i];
                    if (player != null && player.IsSpawned)
                    {
                        players++;
                    }
                }

                if (players > SellMultiplierPlugin.MaxPlayers.Value)
                {
                    SellMultiplierPlugin.Log.LogDebug(source + " skipped: spawnedPlayers=" + players + ", maxPlayers=" + SellMultiplierPlugin.MaxPlayers.Value);
                    return;
                }

                Active = true;
                SellMultiplierPlugin.Log.LogDebug(source + " active: spawnedPlayers=" + players + ", multiplier=" + multiplier);
            }
            catch (Exception exception)
            {
                Active = false;
                SellMultiplierPlugin.Log.LogWarning(source + " safety check failed: " + exception.Message);
            }
        }

        internal static void End()
        {
            Active = false;
        }

        internal static Exception Finalizer(Exception exception)
        {
            Active = false;
            return exception;
        }
    }

    [HarmonyPatch(typeof(MoneyManager), "SellItem")]
    internal static class MoneyManagerSellPatch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {

            SaleScope.Begin("MoneyManager.SellItem");
        }

        [HarmonyPostfix]
        private static void Postfix()
        {
            SaleScope.End();
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            return SaleScope.Finalizer(__exception);
        }
    }

    [HarmonyPatch(typeof(SellBox), "SellItem")]
    internal static class SellBoxPatch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {

            SaleScope.Begin("SellBox.SellItem");
        }

        [HarmonyPostfix]
        private static void Postfix()
        {
            SaleScope.End();
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            return SaleScope.Finalizer(__exception);
        }
    }

    [HarmonyPatch(typeof(NPC), "EatItem")]
    internal static class NpcEatPatch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {

            SaleScope.Begin("NPC.EatItem");
        }

        [HarmonyPostfix]
        private static void Postfix()
        {
            SaleScope.End();
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            return SaleScope.Finalizer(__exception);
        }
    }

    [HarmonyPatch(typeof(Item), "get_TotalWorth")]
    internal static class TotalWorthPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref int __result)
        {
            if (!SaleScope.Active)
            {
                return;
            }

            int original = __result;
            int multiplier = SellMultiplierPlugin.SellMultiplier.Value;
            long multiplied = (long)original * multiplier;
            int adjusted = multiplied > int.MaxValue
                ? int.MaxValue
                : multiplied < int.MinValue
                    ? int.MinValue
                    : (int)multiplied;

            __result = adjusted;
            SellMultiplierPlugin.Log.LogDebug("Sell worth adjusted: " + original + " -> " + adjusted);
        }
    }
}