using Microsoft.AspNetCore.Mvc.Rendering;
using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.Models.Dictionaries;
using System.Collections.Generic;

namespace RegistrDisconnection.ViewModels
{
    /// <summary>
    /// вивід напрямків
    /// </summary>
    public class FilterNapr
    {
        public IEnumerable<ActualDataPerson> People { get; set; }
        public SelectList Directions { get; set; }
        public IEnumerable<DirectionDict> DirectionList { get; set; }
        public string Name { get; set; }
        public int? DirectId { get; set; }
    }
}
