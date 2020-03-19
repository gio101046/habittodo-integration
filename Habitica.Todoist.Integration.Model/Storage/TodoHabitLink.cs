using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Model.Storage
{
    public class TodoHabitLink : TableEntity
    {
        public TodoHabitLink() { }

        public TodoHabitLink(string userId, string todoistId, string habiticaId)
        {
            PartitionKey = userId;
            RowKey = todoistId;
            HabiticaId = habiticaId;
        }

        public string HabiticaId { get; set; }

        public HabitTodoLink Reverse()
        {
            return new HabitTodoLink(PartitionKey, HabiticaId, RowKey);
        }
    }
}
