using RegistrDisconnection.Models.Abonents;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Operations
{
    /// <summary>
    /// оновлення фінансів абонента
    /// </summary>
    [Table("UpdateFinance")]
    public class UpdateFinance
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PersonId")]
        public int? PersonId { get; set; }
        public virtual Person? Person { get; set; }     //посилання на абонента

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrevFinanceSum { get; set; }    //попередні фінанси (сума боргу)

        public int? PrevFinanceId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? NextFinanceSum { get; set; }    //наступні фінанси (теперішній борг)

        public int? NextFinanceId { get; set; }

        [ForeignKey("UpdateGroupId")]
        public int? UpdateGroupId { get; set; }
        public virtual UpdateGroup UpdateGroup { get; set; }    //посилання на поновлення
    }
}
