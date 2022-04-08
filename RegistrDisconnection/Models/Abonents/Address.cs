using RegistrDisconnection.Models.Dictionaries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Abonents
{
    /// <summary>
    /// адреса абонента
    /// </summary>
    [Table("Address")]
    public class Address
    {
        [Key]
        public int Id { get; set; }

        public int? PostalCode { get; set; }     //індекс

        [Required]
        public int CokId { get; set; }          //айді організації

        [Required]
        [StringLength(50)]
        public string District { get; set; }    //область

        [Required]
        [StringLength(50)]
        public string Region { get; set; }      //район

        [Required]
        [StringLength(40)]
        public string CityType { get; set; }    //тип населеного пункту

        [StringLength(15)]
        public string? CityTypeShort { get; set; }    //тип населеного пункту

        [Required]
        public int UtilityAddressId { get; set; }  //айді адреси з БД ОСР

        [Required]
        public int CityId { get; set; } // айді нп з БД ОСР

        [Required]
        [StringLength(50)]
        public string CityName { get; set; }    //назва нас пункту

        [Required]
        [StringLength(10)]
        public string StreetTypeShortName { get; set; }     //коротка назва типу вул

        [Required]
        [StringLength(20)]
        public string StreetType { get; set; }      //тип вул

        [Required]
        [StringLength(50)]
        public string StreetName { get; set; }      //назва вул

        [StringLength(9)]
        public string Building { get; set; }        //будинок

        [StringLength(10)]
        public string BuildingPart { get; set; }    //буква будинку

        [StringLength(5)]
        public string Apartment { get; set; }       //квартира

        //посилання на напрямок
        [ForeignKey("DirectionDictId")]
        public int DirectionDictId { get; set; }    //напрямок
        public virtual DirectionDict Direction { get; set; }
    }
}
