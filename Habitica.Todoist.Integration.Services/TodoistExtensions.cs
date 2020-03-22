using Habitica.Todoist.Integration.Data;
using Habitica.Todoist.Integration.Model.Storage;
using Habitica.Todoist.Integration.Model.Todoist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Habitica.Todoist.Integration.Services
{
    public static class TodoistExtensions
    {
        public static IEnumerable<Item> WhereAdded(this List<Item> items, TableStorageClient storageClient, string userId)
        {
            return items
                .Where(x => !storageClient
                    .Exists<TodoHabitLink>(userId, x.Id) && x.Is_deleted == 0);
        }

        public static IEnumerable<Item> WhereUpdated(this List<Item> items, TableStorageClient storageClient, string userId)
        {
            return items
                .Where(x => storageClient
                    .Exists<TodoHabitLink>(userId, x.Id) && x.Is_deleted == 0 && x.Date_completed == null);
        }

        public static IEnumerable<Item> WhereCompleted(this List<Item> items, TableStorageClient storageClient, string userId)
        {
            return items
                .Where(x => storageClient
                    .Exists<TodoHabitLink>(userId, x.Id) && x.Is_deleted == 0 && x.Date_completed != null);
        }

        public static IEnumerable<Item> WhereDeleted(this List<Item> items, TableStorageClient storageClient, string userId)
        {
            return items
                .Where(x => storageClient
                    .Exists<TodoHabitLink>(userId, x.Id) && x.Is_deleted == 1);
        }
    }
}
