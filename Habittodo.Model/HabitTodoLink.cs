using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habittodo.Model
{
    public class HabitTodoLink : TableEntity
    {
        public HabitTodoLink() { }

        public HabitTodoLink(string userId, string habiticaId, string todoistId, string habiticaParentId = null)
        {
            PartitionKey = userId;
            RowKey = habiticaId;
            HabiticaParentId = habiticaParentId;
            TodoistId = todoistId;
        }

        public string HabiticaParentId { get; set; }
        public string TodoistId { get; set; }

        public TodoHabitLink Reverse()
        {
            return new TodoHabitLink(PartitionKey, TodoistId, RowKey);
        }
    }
}
