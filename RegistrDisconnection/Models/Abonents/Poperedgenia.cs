using RegistrDisconnection.Models.Operations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Abonents
{
    /// <summary>
    /// Видані попередження абоненту
    /// </summary>
    [Table("poperedgenia")]
    public class Poperedgenia
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "date")]
        public DateTime LoadDay { get; set; }       //дата завантаження

        [Column(TypeName = "date")]
        public DateTime? Poper { get; set; }        //дата видачі попередження

        [Column(TypeName = "date")]
        public DateTime? StanNa { get; set; }       //попередження станом на

        [Column(TypeName = "date")]
        public DateTime? DateVykl { get; set; }       //дата відключення

        public bool? Dostavka { get; set; }         //помітка про отримання попередження

        public bool? VydanePoper { get; set; }         //помітка про друк попередження

        [Column(TypeName = "numeric")]
        public decimal Napramok { get; set; }       //напрямок

        public int? Period { get; set; }            //період

        public int? NumberPoper { get; set; }       //номер попередження

        //посилання на персон
        [ForeignKey("PersonId")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }

        [ForeignKey("PoperDrukGroupId")]
        public int? PoperDrukGroupId { get; set; }
        public virtual PoperDrukGroup PoperDruk { get; set; }
    }
}
