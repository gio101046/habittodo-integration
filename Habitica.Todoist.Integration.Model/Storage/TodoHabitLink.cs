using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Model.Storage
{
    public class TodoHabitLink
    {
        public string TodoistId { get; set; }
        public string HabiticaId { get; set; }
    }
}
