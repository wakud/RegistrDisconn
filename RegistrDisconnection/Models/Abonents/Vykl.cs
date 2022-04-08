using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Abonents
{
    /// <summary>
    /// відключення абонента
    /// </summary>
    [Table("vykl")]
    public class Vykl
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "DateTime")]
        public DateTime? DateVykl { get; set; }     //дата відключення

        [Column(TypeName = "DateTime")]
        public DateTime? DateVkl { get; set; }      //дата підключення

        public int? Period { get; set; }            //період

        public bool? Status { get; set; }           //статус відкл чи вкл

        //посилання на обліковий засіб
        [ForeignKey("LichylnykId")]
        public int LichylnykId { get; set; }
        public virtual Lichylnyk Lichylnyk { get; set; }

        public List<Saldo> Saldos { set; get; }

        public Vykl()
        {
            Saldos = new List<Saldo>();
        }

    }
}
