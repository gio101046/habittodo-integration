using Habitica.Todoist.Integration.Data;
using Habitica.Todoist.Integration.Model.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Habitica.Todoist.Integration.Model.Todoist;
using System.Threading.Tasks;
using System.Collections;

namespace Habitica.Todoist.Integration.Services
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
            var response = await todoistClient.GetItemChanges(ReadLatestSyncToken());
            latestSyncToken = response.Sync_token;

            return new Items(response.Items, storageClient, userId);
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
            private List<Item> items { get; set; }
            private readonly TableStorageClient storageClient;
            private readonly string userId;

            internal Items(List<Item> items, TableStorageClient storageClient, string userId)
            {
                this.items = items;
                this.storageClient = storageClient;
                this.userId = userId;
            }

            public IEnumerable<Item> WhereAdded()
            {
                return items
                    .Where(x => !storageClient
                        .Exists<TodoHabitLink>(userId, x.Id) && x.Is_deleted == 0);
            }

            public IEnumerable<Item> WhereUpdated()
            {
                return items
                    .Where(x => storageClient
                        .Exists<TodoHabitLink>(userId, x.Id) && x.Is_deleted == 0);
            }

            public IEnumerable<Item> WhereCompleted()
            {
                return items
                    .Where(x => storageClient
                        .Exists<TodoHabitLink>(userId, x.Id) && x.Is_deleted == 0 && x.Date_completed != null);
            }

            public IEnumerable<Item> WhereDeleted()
            {
                return items
                    .Where(x => storageClient
                        .Exists<TodoHabitLink>(userId, x.Id) && x.Is_deleted == 1);
            }

            public IEnumerator<Item> GetEnumerator()
            {
                return items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
