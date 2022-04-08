using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Operations
{
    /// <summary>
    /// завантаження абонентів з БД ОСР
    /// </summary>
    [Table("LoadUtility")]
    public class LoadUtility
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(4)]
        public string OrganizationCode { get; set; }    //код організації

        [Required]
        [StringLength(10)]
        public string AccountNumber { get; set; }       //особовий абонента

        [Required]
        [StringLength(10)]
        public string AccountNumberNew { get; set; }    //новий особовий

        [Required]
        [StringLength(10)]
        public string AccountId { get; set; }           //айді абонента в ОСР

        [Required]
        [StringLength(300)]
        public string FullName { get; set; }        //ПІП

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }        //Фамілія

        [Required]
        [StringLength(70)]
        public string FirstName { get; set; }       //ім'я

        [Required]
        [StringLength(70)]
        public string SecondName { get; set; }      //по батькові

        [Required]
        [StringLength(300)]
        public string FullAddress { get; set; }     //адреса

        [StringLength(10)]
        public string? IdentificationCode { get; set; } //ідент. код

        [StringLength(12)]
        public string? Passport { get; set; }   //серія і номер паспорту

        [StringLength(10)]
        public string MobilePhoneNumber { get; set; }   //моб. телефон

        [Required]
        public int AdresId { get; set; }    //айді адреси абонента в БД ОСР

        [Required]
        public int? PostalCode { get; set; }    //поштовий індекс

        [Required]
        [StringLength(50)]
        public string District { get; set; }    //район

        [Required]
        [StringLength(50)]
        public string Region { get; set; }      //область

        [Required]
        [StringLength(40)]
        public string CityType { get; set; }    //тип нас. пункту

        [Required]
        [StringLength(10)]
        public string CityTypeShot { get; set; }    //коротка тип нас. пункту (с., м., смт.)

        [Required]
        public int CityCode { get; set; }       //код нас. пункту

        [Required]
        [StringLength(50)]
        public string CityName { get; set; }        //назва населеного пункту

        [StringLength(50)]
        public string? DirectionName { get; set; }      //назва напрямку

        [Required]
        [StringLength(10)]
        public string StreetTypeShortName { get; set; }     //короткий тип вулиці (в., пр-т, б-р і т.д.)

        [Required]
        [StringLength(20)]
        public string StreetType { get; set; }      //тип вулиці (вул., бульвар і т.д.)

        [Required]
        [StringLength(50)]
        public string StreetName { get; set; }      //назва вулиці

        [StringLength(4)]
        public string Building { get; set; }        //номер будинку

        [StringLength(2)]
        public string? BuildingPart { get; set; }   //буква будинку

        [StringLength(5)]
        public string? Apartment { get; set; }      //квартира

        [Required]
        [Column(TypeName = "money")]
        public decimal? RestSumm { get; set; }      //сума боргу

        [Required]
        public int PeriodRestSumm { get; set; }     //період винекнення боргу

        [Required]
        [StringLength(20)]
        public string CounterNumber { get; set; }   //обліковий засіб номер

        [StringLength(24)]
        public string EIS { get; set; }             //код ЕІС

        public string DateDiscon { get; set; }      //дата відключення
        public string DatePay { get; set; }         //дата оплати
        public decimal Oplata { get; set; }         //проведена оплата
        public string DeliverDate { get; set; }     //дата доставки
        public bool Disconnection { get; set; }     //чи відключений абонент
    }
}
