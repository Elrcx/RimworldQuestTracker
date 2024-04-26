using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimworldQuestTracker
{
    internal class QuestTrackerMap : MapComponent
    {
        private bool initialized = false;
        private bool isDragging = false;
        private bool isQuestInfoVisible = true;
        private Vector2 dragStartPos = Vector2.zero;
        private Rect questTrackerRect = new Rect(UI.screenWidth - 200f, 20f, 180f, 30f);
        private float lastClickTime = 0f;
        private float doubleClickTime = 0.3f;

        int amount = UnityEngine.Random.Range(2, 6);  // for testing.

        public QuestTrackerMap(Map map) : base(map)
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
                if (Time.realtimeSinceStartup - lastClickTime < doubleClickTime)
                {
                    // Double-click detected, toggle visibility of quest info.
                    isQuestInfoVisible = !isQuestInfoVisible;
                }
                else
                {
                    // Single-click detected, initiate dragging.
                    isDragging = true;
                    dragStartPos = e.mousePosition - questTrackerRect.position;
                }
                lastClickTime = Time.realtimeSinceStartup;
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
            Text.Anchor = TextAnchor.UpperLeft;

            // Define row sizes.
            float rowHeight = 20f;
            float rowIndentation = 20f;

            // Calculate vertical position.
            float yOffset = questTrackerRect.y + 5f;

            // Draw draggable handle.
            Rect labelRect = new Rect(questTrackerRect.x, yOffset, questTrackerRect.width, rowHeight);
            string handleText = isQuestInfoVisible ? "▼ Tracked quests:" : "► Tracked quests:";
            GUI.Label(labelRect, handleText);
            yOffset += rowHeight;


            // Draw quest information.
            if (isQuestInfoVisible)
            {
                for (int i = 0; i < amount; i++)
                {
                    Rect questRect = new Rect(questTrackerRect.x, yOffset, questTrackerRect.width, rowHeight);
                    GUI.Label(questRect, "► <b>Quest " + (i + 1) + "</b>");
                    yOffset += rowHeight;

                    Rect questDetailsRect = new Rect(questTrackerRect.x + rowIndentation, yOffset, questTrackerRect.width - rowIndentation, rowHeight);
                    GUI.Label(questDetailsRect, "<i>details</i>");
                    yOffset += rowHeight;
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }

    }
}
