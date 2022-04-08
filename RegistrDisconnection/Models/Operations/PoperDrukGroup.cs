using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Operations
{
    /// <summary>
    /// Формування і друк попереджень по напрямку (масове)
    /// </summary>
    [Table("PoperDrukGroup")]
    public class PoperDrukGroup
    {
        public int Id { get; set; }
        public string DirectionName { get; set; }       //назва напрямку
        public int CountAbon { get; set; }              //к-ть абонентів

        [DataType(DataType.Date)]
        public DateTime VydanePoper { get; set; }       //дата видачі попередження

        [DataType(DataType.Date)]
        public DateTime Stanomna { get; set; }          //дата попередження станом на

        [DataType(DataType.Date)]
        public DateTime Vykl { get; set; }              //прогнозована дата відключення

        [ForeignKey("DirectionDictId")]
        public int? DirectionDictId { get; set; }   //айді напрямку
        public virtual DirectionDict? DirectionDict { get; set; }   //посилання на напрямки

        public List<Poperedgenia> Poperedgenias { get; set; }   //список попереджень

        public PoperDrukGroup()
        {
            Poperedgenias = new List<Poperedgenia>();
        }
    }
}
