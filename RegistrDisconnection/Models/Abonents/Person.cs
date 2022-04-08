using RegistrDisconnection.Models.Dictionaries;
using RegistrDisconnection.Models.Operations;
using RegistrDisconnection.MyClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Abonents
{
    /// <summary>
    /// Інформація по абоненту
    /// </summary>
    [Table("Person")]
    public class Person
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CokId")]
        public int CokId { get; set; }                      //код організації
        public virtual Organization Cok { get; set; }       //посилання на організацію

        [Required]
        [StringLength(10)]
        public string OsRah { get; set; }                   //особовий рахунок абонента

        [Required]
        [StringLength(10)]
        public string NewOsRah { get; set; }                   //новий особовий рахунок абонента

        [Required]
        [StringLength(10)]
        public string AccountId { get; set; }               //айді абонента в ОСР

        [Required]
        [StringLength(300)]
        public string FullName { get; set; }                //ПІП абонента

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }                //Прізвище абонента

        [Required]
        [StringLength(100)]
        public string FirsName { get; set; }                //Імя абонента

        [Required]
        [StringLength(100)]
        public string SecondName { get; set; }                //По батькові абонента

        [StringLength(300)]
        public string FullAddress { get; set; }             //адреса абонента

        [StringLength(10)]
        public string MobilePhoneNumber { get; set; }       //моб тел

        //public bool? Status { get; set; }

        [StringLength(10)]
        public string IdentKod { get; set; }                //ідент код

        [StringLength(12)]
        public string Passport { get; set; }                //паспортні дані

        public bool StatusAktyv { get; set; }               //активний абонент чи ні

        //посилання на адрес табл
        [ForeignKey("AddressId")]
        public int AddressId { get; set; }                  //айді адреси
        public virtual Address Address { get; set; }

        //посилання на сальдо
        public List<Saldo> Saldos { get; set; }
        //посилання на фінансові операції
        public List<Finance> Finances { get; set; }
        //посилання на лічильник
        public List<Lichylnyk> Lichylnyks { get; set; }
        //посилання на попередження
        public List<Poperedgenia> Poperedgenias { get; set; }
        //посилання на поновлення фінансів
        public List<UpdateFinance> UpdateFinances { get; set; }

        public Person()
        {
            Finances = new List<Finance>();
            Lichylnyks = new List<Lichylnyk>();
            Poperedgenias = new List<Poperedgenia>();
            UpdateFinances = new List<UpdateFinance>();
            Saldos = new List<Saldo>();
        }

        public string EsLink()
        {
            if (Cok == null)
            {
                Console.WriteLine(string.Format("Can not create link for {0}, because cok is null", OsRah));
                return "";
            }
            return BillingUtils.GetESLink(this);
        }

    }
}
