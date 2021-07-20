using Habittodo.Model.Storage.Enums;
using Microsoft.Azure.Cosmos.Table;

namespace Habittodo.Model.Storage
{
    /* TODO: Rework structure */
    public class TodoChange : TableEntity
    {
        public TodoChange() { }

        public TodoChange(string userId, string todoId)
        {
            PartitionKey = userId;
            RowKey = todoId;
        }

        public TodoApp Application { get; set; }
        public TodoAction Action { get; set; }
        public bool Applied { get; set; }
        public string JsonTodo { get; set; }
    }
}
