using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Abonents
{
    /// <summary>
    /// Обілкові засоби абоента
    /// </summary>
    [Table("Lichylnyk")]
    public class Lichylnyk
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Number { get; set; }          //номер облікового засобу

        [StringLength(24)]
        public string? EIS { get; set; }            //ЕІС облікового засобу

        [StringLength(15)]
        public string? Pokazy { get; set; }         //показник облікового засобу

        public int? Period { get; set; }            //період в який внеслися дані

        //посилання на відключку
        public List<Vykl> Vykls { get; set; }
        public Lichylnyk()
        {
            Vykls = new List<Vykl>();
        }

        //посилання на персон
        [ForeignKey("PersonId")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }
    }
}
