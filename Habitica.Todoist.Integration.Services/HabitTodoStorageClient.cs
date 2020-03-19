using Habitica.Todoist.Integration.Model.Storage;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Services
{
    public class HabitTodoStorageClient
    {
        private CloudStorageAccount storageAccount { get; set; }

        public HabitTodoStorageClient(string connectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(connectionString);
        }

        //public TodoLink CreateTodoLink(TodoLink todoLink)
        //{
        //    return null;
        //}
    }
}
