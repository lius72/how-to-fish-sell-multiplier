using System;
using BepInEx;
using BepInEx.Configuration;
using FishNet.Object;
using HarmonyLib;
using UnityEngine;

namespace HowToFish.SellMultiplier.Multiplayer
{
    [BepInPlugin(Guid, "How to Fish - Multiplayer Sell Multiplier", "1.0.0")]
    public sealed class MultiplayerSellMultiplierPlugin : BaseUnityPlugin
    {
        public const string Guid = "local.howtofish.sellmultiplier.multiplayer";

        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<int> SellMultiplier;
        internal static ConfigEntry<int> MinPlayers;
        internal static ConfigEntry<int> MaxPlayers;
        internal static BepInEx.Logging.ManualLogSource Log;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            Enabled = Config.Bind("General", "Enabled", true, "Enable the multiplayer sell multiplier on the host/server.");
            SellMultiplier = Config.Bind("Economy", "SellMultiplier", 1000, "Multiplier applied when selling a fish on the host/server.");
            MinPlayers = Config.Bind("Multiplayer", "MinPlayers", 2, "Do not apply unless this many spawned Player objects exist.");
            MaxPlayers = Config.Bind("Multiplayer", "MaxPlayers", 4, "Do not apply when more than this many spawned Player objects exist.");

            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(MultiplayerSellMultiplierPlugin).Assembly);
            Logger.LogInfo("How to Fish multiplayer sell multiplier loaded. Multiplier=" + SellMultiplier.Value);
        }

        private void OnDestroy()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
            }
        }
    }

    internal static class MultiplayerSaleScope
    {
        [ThreadStatic]
        internal static bool Active;

        internal static void Begin(string source, NetworkBehaviour behaviour)
        {
            Active = false;

            if (!MultiplayerSellMultiplierPlugin.Enabled.Value)
            {
                return;
            }

            if (behaviour == null || !behaviour.IsServerInitialized)
            {
                MultiplayerSellMultiplierPlugin.Log.LogDebug(source + " skipped: not running on host/server");
                return;
            }

            int multiplier = MultiplayerSellMultiplierPlugin.SellMultiplier.Value;
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

                if (players < MultiplayerSellMultiplierPlugin.MinPlayers.Value)
                {
                    MultiplayerSellMultiplierPlugin.Log.LogDebug(source + " skipped: spawnedPlayers=" + players + " < minPlayers=" + MultiplayerSellMultiplierPlugin.MinPlayers.Value);
                    return;
                }

                if (players > MultiplayerSellMultiplierPlugin.MaxPlayers.Value)
                {
                    MultiplayerSellMultiplierPlugin.Log.LogDebug(source + " skipped: spawnedPlayers=" + players + " > maxPlayers=" + MultiplayerSellMultiplierPlugin.MaxPlayers.Value);
                    return;
                }

                Active = true;
                MultiplayerSellMultiplierPlugin.Log.LogDebug(source + " active: spawnedPlayers=" + players + ", multiplier=" + multiplier);
            }
            catch (Exception exception)
            {
                Active = false;
                MultiplayerSellMultiplierPlugin.Log.LogWarning(source + " safety check failed: " + exception.Message);
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
        private static void Prefix(MoneyManager __instance)
        {
            MultiplayerSaleScope.Begin("MoneyManager.SellItem", __instance);
        }

        [HarmonyPostfix]
        private static void Postfix()
        {
            MultiplayerSaleScope.End();
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            return MultiplayerSaleScope.Finalizer(__exception);
        }
    }

    [HarmonyPatch(typeof(SellBox), "SellItem")]
    internal static class SellBoxPatch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            MultiplayerSaleScope.Begin("SellBox.SellItem", MoneyManager.Instance);
        }

        [HarmonyPostfix]
        private static void Postfix()
        {
            MultiplayerSaleScope.End();
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            return MultiplayerSaleScope.Finalizer(__exception);
        }
    }

    [HarmonyPatch(typeof(NPC), "EatItem")]
    internal static class NpcEatPatch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            MultiplayerSaleScope.Begin("NPC.EatItem", MoneyManager.Instance);
        }

        [HarmonyPostfix]
        private static void Postfix()
        {
            MultiplayerSaleScope.End();
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            return MultiplayerSaleScope.Finalizer(__exception);
        }
    }

    [HarmonyPatch(typeof(Item), "get_TotalWorth")]
    internal static class TotalWorthPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref int __result)
        {
            if (!MultiplayerSaleScope.Active)
            {
                return;
            }

            int original = __result;
            int multiplier = MultiplayerSellMultiplierPlugin.SellMultiplier.Value;
            long multiplied = (long)original * multiplier;
            int adjusted = multiplied > int.MaxValue
                ? int.MaxValue
                : multiplied < int.MinValue
                    ? int.MinValue
                    : (int)multiplied;

            __result = adjusted;
            MultiplayerSellMultiplierPlugin.Log.LogDebug("Sell worth adjusted: " + original + " -> " + adjusted);
        }
    }
}