using Habitica.Todoist.Integration.Data;
using ChecklistItem = Habitica.Todoist.Integration.Model.Habitica.ChecklistItem;
using Habitica.Todoist.Integration.Model.Habitica.Enums;
using Habitica.Todoist.Integration.Model.Storage;
using Habitica.Todoist.Integration.Model.Todoist;
using Habitica.Todoist.Integration.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Habitica.Todoist.Integration.Services
{
    public class HabiticaIntegrationService
    {
        private HabiticaServiceClient habiticaClient { get; set; }
        private TableStorageClient storageClient { get; set; }

        private string userId { get; set; }

        public HabiticaIntegrationService(string habiticaUserId, 
            string habiticaApiKey, 
            string storageConnectionString,
            string userId)
        {
            this.habiticaClient = new HabiticaServiceClient(habiticaUserId, habiticaApiKey);
            this.storageClient = new TableStorageClient(storageConnectionString);
            this.userId = userId;
        }

        public async Task Add(IEnumerable<Item> items)
        {
            foreach (var item in items.OrderBy(x => x.Parent_Id))
            {
                if (!item.IsChild)
                    await AddTask(item);
                else
                    await AddChecklistItem(item);
            }
        }

        public async Task AddTask(Item item)
        {
            if (item.IsChild)
                throw new ArgumentException("Item passed as arguement has a valid Parent_Id");

            var task = (await habiticaClient.CreateTask(item.ToHabiticaTask())).Data;
            var link = new TodoHabitLink(userId, item.Id, task.Id);

            await storageClient.InsertOrUpdate(link);
            await storageClient.InsertOrUpdate(link.Reverse());
        }

        public async Task AddChecklistItem(Item item)
        {
            if (!item.IsChild)
                throw new ArgumentException("Item passed as arguement does not have a valid Parent_Id");

            var habiticaTaskId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, item.Parent_Id)).HabiticaId;
            var checklistItem = (await habiticaClient.CreateChecklistItem(item.ToHabiticaChecklistItem(), habiticaTaskId)).Data.Checklist
                .First(x => x.Text == item.Content);

            var link = new TodoHabitLink(userId, item.Id, checklistItem.Id);
            await storageClient.InsertOrUpdate(link);
            await storageClient.InsertOrUpdate(link.Reverse());
        }

        public async Task UpdateTasks(IEnumerable<Item> items)
        {
            foreach (var item in items)
                await UpdateTask(item);
        }

        public async Task UpdateTask(Item item)
        {
            var habiticaId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, item.Id)).HabiticaId;
            await habiticaClient.UpdateTask(item.ToHabiticaTask(habiticaId)); 
        }

        public async Task CompleteTasks(IEnumerable<Item> items)
        {
            foreach (var item in items)
                await CompleteTask(item);
        }

        public async Task CompleteTask(Item item)
        {
            await CompleteTask(item.Id);
        }

        public async Task CompleteTask(string todoistId)
        {
            var habiticaId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, todoistId)).HabiticaId;
            await habiticaClient.ScoreTask(habiticaId, ScoreAction.Up);
        }

        public async Task DeleteTasks(IEnumerable<Item> items)
        {
            foreach (var item in items)
                await DeleteTask(item);
        }

        public async Task DeleteTask(Item item)
        {
            await DeleteTask(item.Id);
        }

        public async Task DeleteTask(string todoistId)
        {
            var habiticaId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, todoistId)).HabiticaId;
            await habiticaClient.DeleteTask(habiticaId);
        }
    }
}
