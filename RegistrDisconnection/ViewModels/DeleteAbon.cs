using Microsoft.AspNetCore.Mvc.Rendering;
using RegistrDisconnection.Models.Abonents;
using System.Collections.Generic;

namespace RegistrDisconnection.ViewModels
{
    /// <summary>
    /// масове видалення абонентів
    /// </summary>
    public class DeleteAbon
    {
        public IEnumerable<Person> People { get; set; }

        public SelectList Cok { get; set; }
    }
}
