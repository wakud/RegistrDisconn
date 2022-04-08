using Microsoft.AspNetCore.Mvc.Rendering;
using RegistrDisconnection.Models.Abonents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistrDisconnection.ViewModels
{
    /// <summary>
    /// видалення напрямків
    /// </summary>
    public class DellNapr
    {
        public IEnumerable<ActualDataPerson> People { get; set; }
        public SelectList Directions { get; set; }
        public int? DirectId { get; set; }
        public int CountAbon { get; set; }
        [DataType(DataType.Date)]
        public DateTime VydanePoper { get; set; }
        public DateTime Stanomna { get; set; }
        public DateTime Vykl { get; set; }
    }
}
