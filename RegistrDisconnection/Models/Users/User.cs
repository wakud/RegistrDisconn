//using Audit.EntityFramework;
using RegistrDisconnection.Models.Dictionaries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Users
{
    /// <summary>
    /// таблиця короистувачів
    /// </summary>
    //[AuditDbContext]
    [Table("User")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string FullName { get; set; }        //ПІП користувача

        [Required]
        [StringLength(10)]
        public string Login { get; set; }           //Логін користувача

        [Required]
        public string Password { get; set; }        //Пароль користувача

        [ForeignKey("CokId")]
        public int? CokId { get; set; }

        public virtual Organization Cok { get; set; }   //організація

        [ForeignKey("PravaId")]
        public int PravaId { get; set; }

        public virtual Prava Prava { get; set; }    //права користувача
    }
}
