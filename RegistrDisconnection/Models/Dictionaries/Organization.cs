using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.Models.Users;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Dictionaries
{
    /// <summary>
    /// організація
    /// </summary>
    [Table("Organization")]
    public class Organization
    {
        [Key]
        public int Id { get; set; }

        [StringLength(4)]
        public string Code { get; set; }

        [StringLength(50)]
        public string Name { get; set; }    //назва 

        [StringLength(50)]
        public string Nach { get; set; }        //начальник

        [StringLength(50)]
        public string Buh { get; set; }         //бухгалтер

        [StringLength(5)]
        public string Index { get; set; }       //поштовий індекс

        [Required]
        public int RegionId { get; set; }      //id району для витягнення нас.пунктів по скрипту

        [StringLength(50)]
        public string Oblast { get; set; }      //область

        [StringLength(100)]
        public string Rajon { get; set; }       //район

        [StringLength(100)]
        public string City { get; set; }        //населений пункт

        [StringLength(100)]
        public string Street { get; set; }      //вулиця

        [StringLength(100)]
        public string Address { get; set; }     //повна адреса

        [StringLength(10)]
        public string Tel { get; set; }         //телефон

        [StringLength(8)]
        public string EDRPOU { get; set; }         //ЄДРПОУ

        [StringLength(12)]
        public string IPN { get; set; }         //ІПН

        [StringLength(8)]
        public string MFO { get; set; }         //МФО

        [StringLength(100)]
        public string RozRah { get; set; }         //р/р

        [StringLength(100)]
        public string NameREM { get; set; }         //назва ОСР

        [StringLength(36)]
        public string OrganizationUnitGUID { get; set; }    //Guid організації в БД ОСР

        [StringLength(10)]
        public string DbConfigName { get; set; }            //назва конфігурації бд

        public int? NumLastPoper { get; set; }       //номер останнього поперелдення

        public int CurrPeriod { get; set; }     //закритий період в організації

        public List<User> Users { get; set; }   //список користувачів

        public List<Person> Persons { get; set; }   //список абонентів

        public Organization()
        {
            Users = new List<User>();
            Persons = new List<Person>();
        }

        public bool IsLoads { get; set; }                   //чи в даний момент триває завантаження
    }
}
