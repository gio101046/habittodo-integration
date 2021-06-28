using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habittodo.Model
{
    public class TodoHabitLink : TableEntity
    {
        public TodoHabitLink() { }

        public TodoHabitLink(string userId, string todoistId, string habiticaId, string todoistParentId = null)
        {
            PartitionKey = userId;
            RowKey = todoistId;
            TodoistParentId = todoistParentId;
            HabiticaId = habiticaId;
        }

        public string TodoistParentId { get; set; }
        public string HabiticaId { get; set; }

        public HabitTodoLink Reverse(string habiticaParentId = null)
        {
            return new HabitTodoLink(PartitionKey, HabiticaId, RowKey, habiticaParentId);
        }
    }
}
