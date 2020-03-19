using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Model.Storage
{
    public class HabitTodoLink : TableEntity
    {
        public HabitTodoLink() { }

        public HabitTodoLink(string userId, string habiticaId, string todoistId)
        {
            PartitionKey = userId;
            RowKey = habiticaId;
            TodoistId = todoistId;
        }

        public string TodoistId { get; set; }

        public TodoHabitLink Reverse()
        {
            return new TodoHabitLink(PartitionKey, TodoistId, RowKey);
        }
    }
}
