using HarmonyLib;
using UnityEngine;
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
                if (Current.Game.CurrentMap.GetComponent<MapComponent_QuestTracker>() == null)
                {
                    Current.Game.CurrentMap.components.Add(new MapComponent_QuestTracker(Current.Game.CurrentMap));
                }
            }
        }
    }

    public class MapComponent_QuestTracker : MapComponent
    {
        private bool initialized = false;
        private bool isDragging = false;
        private Vector2 dragStartPos = Vector2.zero;
        private Rect questTrackerRect = new Rect(UI.screenWidth - 200f, 20f, 180f, 30f);

        public MapComponent_QuestTracker(Map map) : base(map)
        {
        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();

            if (!initialized && Find.MainTabsRoot != null && Find.CurrentMap == map)
            {
                initialized = true;
            }
        }

        public override void MapComponentOnGUI()
        {
            base.MapComponentOnGUI();

            if (initialized)
            {
                HandleInput();
                DrawQuestTracker();
            }
        }

        private void HandleInput()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && questTrackerRect.Contains(e.mousePosition))
            {
                isDragging = true;
                dragStartPos = e.mousePosition - questTrackerRect.position;
                e.Use();
            }
            else if (isDragging && e.type == EventType.MouseUp && e.button == 0)
            {
                isDragging = false;
                e.Use();
            }

            if (isDragging && e.type == EventType.MouseDrag)
            {
                questTrackerRect.position = e.mousePosition - dragStartPos;
                e.Use();
            }
        }

        private void DrawQuestTracker()
        {
            Text.Anchor = TextAnchor.UpperRight;

            Texture2D backgroundTexture = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 0.5f));
            GUI.DrawTexture(questTrackerRect, backgroundTexture);

            GUI.color = Color.white;
            Widgets.Label(questTrackerRect, "Quest Tracker");

            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
