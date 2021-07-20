using Microsoft.Azure.Cosmos.Table;

namespace Habittodo.Model.Storage
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
