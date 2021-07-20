using Habittodo.Data;
using Habittodo.Model.Storage;
using System.Collections.Generic;
using System.Linq;
using Habittodo.Model.Todoist;
using System.Threading.Tasks;
using System.Collections;

namespace Habittodo.Service
{
    public class TodoistIntegrationService
    {
        private TodoistServiceClient todoistClient { get; set; }
        private TableStorageClient storageClient { get; set; }

        private string userId { get; set; }
        private string latestSyncToken { get; set; } = string.Empty;

        public TodoistIntegrationService(string todoistApiKey, 
            string storageConnectionString,
            string userId)
        {
            this.todoistClient = new TodoistServiceClient(todoistApiKey);
            this.storageClient = new TableStorageClient(storageConnectionString);
            this.userId = userId;
        }

        public async Task<Items> ReadItemChanges()
        {
            var itemChangesResponse = await todoistClient.GetItemChanges(ReadLatestSyncToken());
            latestSyncToken = itemChangesResponse.Sync_token;

            return new Items(itemChangesResponse.Items, storageClient, userId);
        }

        public async Task CommitRead()
        {
            await storageClient.InsertOrUpdate(new TodoistSync(userId, latestSyncToken));
        }

        private string ReadLatestSyncToken()
        {
            try
            {
                return storageClient.Query<TodoistSync>()
                    .Where(x => x.PartitionKey == userId)
                    .ToList()
                    .OrderByDescending(x => x.Timestamp)
                    .First().RowKey;
            }
            catch { }

            return string.Empty;
        }

        public class Items : IEnumerable<Item>
        {
            private List<Item> changedItems;
            private readonly TableStorageClient storageClient;
            private readonly string userId;

            internal Items(List<Item> changedItems, TableStorageClient storageClient, string userId)
            {
                this.changedItems = changedItems;
                this.storageClient = storageClient;
                this.userId = userId;
            }

            public IEnumerable<Item> WhereAdded()
            {
                return changedItems
                    .Where(x => !storageClient
                        .Exists<TodoHabitLink>(userId, x.Id) && x.Is_deleted == 0)
                    .OrderBy(x => x.Parent_Id);
            }

            public IEnumerable<Item> WhereUpdated()
            {
                return changedItems
                    .Where(x => storageClient
                        .Exists<TodoHabitLink>(userId, x.Id) && x.Is_deleted == 0)
                    .OrderBy(x => x.Parent_Id);
            }

            public IEnumerable<Item> WhereCompleted()
            {
                return changedItems
                    .Where(x => storageClient
                        .Exists<TodoHabitLink>(userId, x.Id) && x.Is_deleted == 0 && x.Date_completed != null)
                    .OrderByDescending(x => x.Parent_Id);
            }

            public IEnumerable<Item> WhereDeleted()
            {
                return changedItems
                    .Where(x => storageClient
                        .Exists<TodoHabitLink>(userId, x.Id) && x.Is_deleted == 1);
            }

            public IEnumerator<Item> GetEnumerator()
            {
                return changedItems.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
