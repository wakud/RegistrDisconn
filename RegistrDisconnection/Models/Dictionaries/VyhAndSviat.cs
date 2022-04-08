using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Dictionaries
{
    /// <summary>
    /// таблиця вихідних і святкових днів
    /// </summary>
    [Table("VyhAndSviat")]
    public class VyhAndSviat
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Date { get; set; }       //дата

        public int? Year { get; set; }      //рік
        public int? Month { get; set; }     //місяць
        public int? Day { get; set; }       //день

        public string Name { get; set; }    //назва свята

        private bool MatchDt(DateTime dt)
        {
            return
                (Year == null || dt.Year == Year) &&
                (Month == null || dt.Month == Month) &&
                (Day == null || dt.Day == Day)
            ;
        }

        private DateTime GetForCurrent(DateTime? dt = null)
        {
            DateTime currentDt = DateTime.Today;
            if (dt != null)
            {
                currentDt = (DateTime)dt;
            }
            return new DateTime(
                year: Year == null ? currentDt.Year : (int)Year,
                month: Month == null ? currentDt.Month : (int)Month,
                day: Day == null ? currentDt.Day : (int)Day
            );
        }

    }
}
