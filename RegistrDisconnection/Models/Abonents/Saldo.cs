using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Abonents
{
    /// <summary>
    /// Сальдовка по абоненту
    /// </summary>
    [Table("Saldo")]
    public class Saldo
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DebPoch { get; set; }       //борг на початок періоду

        [Column(TypeName = "decimal(10,2)")]
        public decimal? KredPoch { get; set; }      //переплата на початок періоду

        [Column(TypeName = "decimal(10,2)")]
        public decimal? SumaVykl { get; set; }      //оплата за відключення

        [Column(TypeName = "decimal(10,2)")]
        public decimal? SumaVkl { get; set; }       //оплата за підключення

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Oplata { get; set; }        //оплата протягом періоду[Column(TypeName = "decimal(10,2)")]

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OplataZaEE { get; set; }        //оплата протягом періоду за активну е/е

        [Column(TypeName = "decimal(10,2)")]
        public decimal? BorgZaEE { get; set; }        //борг за активну е/е

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DebKin { get; set; }        //Дт на кінець періоду

        [Column(TypeName = "decimal(10,2)")]
        public decimal? KredKin { get; set; }       //переплата на кінець періоду

        public int? StartPeriod { get; set; }       //період коли абонент попав в програму

        public bool ZakrPeriod { get; set; }        //останній закритий період

        public int? AktPeriod { get; set; }         //активний період

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Recount { get; set; }       //перерахунки за активну е/е

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Narah { get; set; }         //нарахована сума в звітньому періоді за активну е/е

        //посилання на персон
        [ForeignKey("PersonId")]
        public int? PersonId { get; set; }
        public virtual Person Person { get; set; }  //посилання на персон

        //посилання на персон
        [ForeignKey("VyklId")]
        public int? VyklId { get; set; }
        public virtual Vykl Vykl { get; set; }  //посилання на персон


    }
}
