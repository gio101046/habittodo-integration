using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Model.Storage
{
    public class TodoistSync : TableEntity
    {
        public TodoistSync() { }

        public TodoistSync(string userId, string syncToken)
        {
            PartitionKey = userId;
            RowKey = syncToken;
        }
    }
}
