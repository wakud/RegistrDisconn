using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Abonents
{
    /// <summary>
    /// Актуальні дані по абоненту
    /// </summary>
    [Table("ActualDataPerson")]
    public class ActualDataPerson
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? LoadSum { get; set; }       //від якої суми завантажено

        //посилання на персон
        [ForeignKey("PersonId")]
        public int? PersonId { get; set; }
        public virtual Person? Person { get; set; }

        //посилання на фінанси
        [ForeignKey("FinanceId")]
        public int? FinanceId { get; set; }
        public virtual Finance? Finance { get; set; }

        //посилання на лічильник
        [ForeignKey("LichylnykId")]
        public int? LichylnykId { get; set; }
        public virtual Lichylnyk? Lichylnyk { get; set; }

        //посилання на попередження
        [ForeignKey("PoperedgeniaId")]
        public int? PoperedgeniaId { get; set; }
        public virtual Poperedgenia? Poperedgenia { get; set; }

        //посилання на відключку
        [ForeignKey("VyklId")]
        public int? VyklId { get; set; }
        public virtual Vykl? Vykl { get; set; }

        //посилання на сальдо
        [ForeignKey("SaldoId")]
        public int? SaldoId { get; set; }
        public virtual Saldo? Saldo { get; set; }
    }
}
