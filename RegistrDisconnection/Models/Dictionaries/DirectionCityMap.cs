using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Dictionaries
{
    /// <summary>
    /// карта напрямків
    /// </summary>
    [Table("DirectionCityMap")]
    public class DirectionCityMap
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DirectionDictId { get; set; }                //код напрямку
        public virtual DirectionDict Direction { get; set; }

        [Required]
        public int UtilityCityId { get; set; }                  //код нас пункту в ОСР

        public string Name { get; set; }                        //назва нас пункту

    }
}
