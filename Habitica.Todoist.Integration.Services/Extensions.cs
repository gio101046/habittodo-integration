using Habitica.Todoist.Integration.Model.Habitica;
using Habitica.Todoist.Integration.Model.Habitica.Enums;
using Habitica.Todoist.Integration.Model.Todoist;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Services.Extensions
{
    public static class Extensions
    {
        public static ChecklistItem ToHabiticaChecklistItem(this Item item, string habiticaId = null)
        {
            if (string.IsNullOrEmpty(item.Parent_Id))
                return null;

            var checklistItem = new ChecklistItem
            {
                Id = habiticaId,
                Text = item.Content
            };

            return checklistItem;
        }

        public static Task ToHabiticaTask(this Item item, string habiticaId = null)
        {
            if (!string.IsNullOrEmpty(item.Parent_Id)) 
                return null;

            var taskTypeStr = Enum.GetName(typeof(TaskType), TaskType.Todo).ToLower();
            var task = new Task
            {
                Id = habiticaId,
                Text = item.Content,
                Type = taskTypeStr,
                Date = item.Due?.ToJavaScriptDateStr(),
                Priority = GetHabiticaDifficulty(item.Priority)
            };

            return task;
        }

        private static string GetHabiticaDifficulty(int todoistPriority)
        {
            switch (todoistPriority)
            {
                case 1:
                    return "0.1";
                case 2:
                    return "1";
                case 3:
                    return "1.5";
                case 4:
                    return "2";
            }
            return null;
        }
    }
}
