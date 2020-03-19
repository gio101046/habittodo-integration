using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Model.Storage
{
    public class TodoChange
    {
        public string Id { get; set; }
        public TodoApp Application { get; set; }
        public TodoAction Action { get; set; }
        public bool Applied { get; set; }
        public string JsonTodo { get; set; }
    }
}
