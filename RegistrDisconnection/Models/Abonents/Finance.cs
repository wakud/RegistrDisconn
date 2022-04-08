using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Abonents
{
    /// <summary>
    /// Фінансові операції по абоненту
    /// </summary>
    [Table("Finance")]
    public class Finance
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DebLoad { get; set; }       //борг на початок періоду

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DebPoch { get; set; }       //борг на початок періоду

        [Column(TypeName = "decimal(10,2)")]
        public decimal? KredPoch { get; set; }      //переплата на початок періоду

        [Column(TypeName = "decimal(10,2)")]
        public decimal? SumaVykl { get; set; }      //оплата за відключення

        [Column(TypeName = "decimal(10,2)")]
        public decimal? SumaVkl { get; set; }       //оплата за підключення

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Oplata { get; set; }        //оплата протягом періоду

        [Column(TypeName = "decimal(10,2)")]
        public decimal? DebKin { get; set; }        //борг на кінець періоду

        [Column(TypeName = "decimal(10,2)")]
        public decimal? KredKin { get; set; }       //переплата на кінець періоду

        public int? Period { get; set; }            //період

        public int? AktPer { get; set; }            //активний період

        public int? StatPer { get; set; }           //статичний період

        //посилання на персон
        [ForeignKey("PersonId")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }
    }
}
