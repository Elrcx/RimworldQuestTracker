using HarmonyLib;
using Verse;

namespace RimworldQuestTracker
{
    [StaticConstructorOnStartup]
    public static class QuestTrackerMod
    {
        static QuestTrackerMod()
        {
            Harmony harmony = new Harmony("com.questtracker.rimworld.mod");
            harmony.PatchAll();

            // Lets register MapComponent to listen for map events.
            LongEventHandler.ExecuteWhenFinished(InitializeMapComponent);
        }

        private static void InitializeMapComponent()
        {
            // Check if we are in a game with an active map (colony view), this should prevent NullReference exceptions.
            if (Current.Game != null && Current.Game.CurrentMap != null)
            {
                // Register the MapComponent if it's not already registered.
                if (Current.Game.CurrentMap.GetComponent<QuestTrackerMap>() == null)
                {
                    Current.Game.CurrentMap.components.Add(new QuestTrackerMap(Current.Game.CurrentMap));
                }
            }
        }
    }
}
