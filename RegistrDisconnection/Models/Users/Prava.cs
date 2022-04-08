using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Users
{
    /// <summary>
    /// таблиця прав доступу до програми
    /// </summary>
    [Table("Prava")]
    public class Prava
    {
        [Key]
        public int Id { get; set; }

        [StringLength(25)]
        public string Name { get; set; }    //назва

        [StringLength(1)]
        public string Code { get; set; }    //код

        public List<User> Users { get; set; }   //список користувачів

        public Prava()
        {
            Users = new List<User>();
        }
    }
}
