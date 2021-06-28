using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habittodo.Model
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
