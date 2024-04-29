using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections;
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

            // Draw quest information if visible.
            if (isQuestInfoVisible)
            {
                QuestManager questManager = Find.QuestManager;
                if (questManager != null)
                {
                    for (int i = questManager.QuestsListForReading.Count - 1; i >= 0; i--)
                    {
                        Quest quest = questManager.QuestsListForReading[i];

                        if (!quest.EverAccepted && quest.State != QuestState.Ongoing) continue;
                        QuestPart_Delay delayPart = GetMainDelayPart(quest);

                        string questName = $"► <b>{quest.name}</b>";
                        DrawLabel(questName, rowIndentation, rowHeight, ref yOffset);

                        CheckSpecialRequirements(quest, rowIndentation, rowHeight, ref yOffset);

                        string expiryInfo = $"<i>{string.Format(delayPart.expiryInfoPart, GetRemainingTime(quest, delayPart))}.</i>";
                        DrawLabel(expiryInfo, rowIndentation, rowHeight, ref yOffset);
                    }
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void CheckSpecialRequirements(Quest quest, float rowIndentation, float rowHeight, ref float yOffset)
        {
            foreach (QuestPart part in quest.PartsListForReading)
            {
                if (part is QuestPart_InitiateTradeRequest tradeRequest)
                {
                    int thingCountInStorage = Find.CurrentMap.resourceCounter.GetCount(tradeRequest.requestedThingDef);

                    string labelText = $"Trade x{tradeRequest.requestedCount} of {tradeRequest.requestedThingDef.LabelCap} ({thingCountInStorage}/{tradeRequest.requestedCount})";
                    DrawLabel(labelText, rowIndentation, rowHeight, ref yOffset);
                }
            }
        }

        private void DrawLabel(string text, float rowIndentation, float rowHeight, ref float yOffset)
        {
            GUI.skin.label.wordWrap = true;
            Color originalColor = GUI.contentColor;
            GUI.contentColor = Color.white;

            float labelWidth = questTrackerRect.width - rowIndentation;
            Vector2 labelSize = GUI.skin.label.CalcSize(new GUIContent(text));
            int lines = Mathf.CeilToInt(labelSize.x / labelWidth);

            Rect questDetailsRect = new Rect(questTrackerRect.x + rowIndentation, yOffset, labelWidth, labelSize.y * lines);
            GUI.Label(questDetailsRect, text);
            yOffset += rowHeight * lines;

            GUI.contentColor = originalColor;
            GUI.skin.label.wordWrap = false;
        }

        private string GetRemainingTime(Quest quest, QuestPart_Delay delayPart)
        {
            if (quest.State != QuestState.Ongoing) return "Completed";

            int remainingTicks = Mathf.Max(0, delayPart.delayTicks - quest.TicksSinceAccepted);
            int remainingDays = Mathf.FloorToInt((float)remainingTicks / GenDate.TicksPerDay);
            float remainingDaysFloat = (float)Math.Round((float)remainingTicks / GenDate.TicksPerDay, 1);
            int remainingHours = Mathf.RoundToInt((float)remainingTicks % GenDate.TicksPerDay / GenDate.TicksPerHour);

            if (remainingDays >= 10)
            {
                return $"{remainingDays} Days";
            }
            else if (remainingDays > 1 && remainingDays < 10)
            {
                return $"{remainingDaysFloat} Days";
            }
            return $"{remainingHours} Hours";
        }

        private QuestPart_Delay GetMainDelayPart(Quest quest)
        {
            foreach (QuestPart part in quest.PartsListForReading)
            {
                if (part is QuestPart_Delay delayPart && delayPart.expiryInfoPart != null)
                {
                    return delayPart;
                }
            }

            return null;
        }
    }
}
