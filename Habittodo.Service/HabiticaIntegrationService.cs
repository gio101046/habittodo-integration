using Habittodo.Data;
using ChecklistItem = Habittodo.Model.Habitica.ChecklistItem;
using Habittodo.Model.Habitica.Enums;
using Habittodo.Model.Storage;
using Habittodo.Model.Todoist;
using Habittodo.Service.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Habittodo.Service
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

            var link = new TodoHabitLink(userId, item.Id, checklistItem.Id, item.Parent_Id);
            await storageClient.InsertOrUpdate(link);
            await storageClient.InsertOrUpdate(link.Reverse(habiticaTaskId));
        }

        public async Task Update(IEnumerable<Item> items)
        {
            foreach (var item in items.OrderBy(x => x.Parent_Id))
            {
                if (!item.IsChild)
                    await UpdateTask(item);
                else
                    await UpdateChecklistItem(item);
            }
        }

        public async Task UpdateTask(Item item)
        {
            if (item.IsChild)
                throw new ArgumentException("Item passed as arguement has a valid Parent_Id");

            var habiticaId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, item.Id)).HabiticaId;
            await habiticaClient.UpdateTask(item.ToHabiticaTask(habiticaId)); 
        }

        public async Task UpdateChecklistItem(Item item)
        {
            if (!item.IsChild)
                throw new ArgumentException("Item passed as arguement does not have a valid Parent_Id");

            var habiticaTaskId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, item.Parent_Id)).HabiticaId;
            var habiticaChecklistId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, item.Id)).HabiticaId;
            await habiticaClient.UpdateChecklistItem(item.ToHabiticaChecklistItem(habiticaChecklistId), habiticaTaskId);
        }

        public async Task Complete(IEnumerable<Item> items)
        {
            foreach (var item in items.OrderByDescending(x => x.Parent_Id))
            {
                if (!item.IsChild)
                    await CompleteTask(item.Id);
                else
                    await CompleteChecklistItem(item.Id, item.Parent_Id);
            }
        }

        public async Task CompleteTask(string todoistId)
        {
            var habiticaId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, todoistId)).HabiticaId;
            await habiticaClient.ScoreTask(habiticaId, ScoreAction.Up);
        }

        public async Task CompleteChecklistItem(string todoistId, string todoistParentId)
        {
            var habiticaTaskId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, todoistParentId)).HabiticaId;
            var habiticaChecklistId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, todoistId)).HabiticaId;
            await habiticaClient.ScoreChecklistItem(habiticaTaskId, habiticaChecklistId);
        }

        public async Task Delete(IEnumerable<Item> items)
        {
            List<(Item Item, TodoHabitLink Link)> itemsAndLinks = new List<(Item, TodoHabitLink)>();
            foreach (var item in items)
                itemsAndLinks.Add((item, await storageClient.RetrieveRecord<TodoHabitLink>(userId, item.Id)));

            foreach (var itemAndLink in itemsAndLinks.OrderByDescending(x => x.Link.TodoistParentId))
            {
                var item = itemAndLink.Item;
                var link = itemAndLink.Link;
                if (string.IsNullOrEmpty(link.TodoistParentId))
                    await DeleteTask(item.Id);
                else
                    await DeleteChecklistItem(item.Id, link.TodoistParentId);
            }
        }

        public async Task DeleteTask(string todoistId)
        {
            var habiticaId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, todoistId)).HabiticaId;
            await habiticaClient.DeleteTask(habiticaId);
        }

        public async Task DeleteChecklistItem(string todoistId, string todoistParentId)
        {
            var habiticaTaskId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, todoistParentId)).HabiticaId;
            var habiticaChecklistId = (await storageClient.RetrieveRecord<TodoHabitLink>(userId, todoistId)).HabiticaId;
            await habiticaClient.DeleteChecklistItem(habiticaTaskId, habiticaChecklistId);
        }
    }
}
