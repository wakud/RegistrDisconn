using Microsoft.AspNetCore.Mvc.Rendering;
using RegistrDisconnection.Models.Abonents;
using System.Collections.Generic;

namespace RegistrDisconnection.ViewModels
{
    public class DeleteAbon
    {
        public IEnumerable<Person> People { get; set; }
        //public IEnumerable<ActualDataPerson> People { get; set; }

        public SelectList Cok { get; set; }
    }
}
