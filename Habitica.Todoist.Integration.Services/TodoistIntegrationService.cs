using Habitica.Todoist.Integration.Data;
using Habitica.Todoist.Integration.Model.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Habitica.Todoist.Integration.Model.Todoist;
using System.Threading.Tasks;

namespace Habitica.Todoist.Integration.Services
{
    public class TodoistIntegrationService
    {
        private TodoistServiceClient todoistClient { get; set; }
        private TableStorageClient storageClient { get; set; }

        private string userId { get; set; }
        private string latestSyncToken { get; set; } = string.Empty;

        public TodoistIntegrationService(TodoistServiceClient todoistClient, 
            TableStorageClient storageClient,
            string userId)
        {
            this.todoistClient = todoistClient;
            this.storageClient = storageClient;
            this.userId = userId;
        }

        public async Task<List<Item>> ReadItemChanges()
        {
            var response = await todoistClient.GetItemChanges(ReadLatestSyncToken());
            return response.Items;
        }

        public async Task CommitRead()
        {
            await storageClient.InsertOrUpdate(new TodoistSync(userId, latestSyncToken));
        }

        private string ReadLatestSyncToken()
        {
            try
            {
                latestSyncToken = storageClient.Query<TodoistSync>()
                    .Where(x => x.PartitionKey == userId)
                    .ToList()
                    .OrderByDescending(x => x.Timestamp)
                    .First().RowKey;
            }
            catch { }

            return latestSyncToken;
        }
    }
}
