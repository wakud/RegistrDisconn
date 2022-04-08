using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Dictionaries
{
    /// <summary>
    /// опції напрямку
    /// </summary>
    [Table("Options")]
    public class Options
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Value { get; set; }

        [StringLength(150)]
        public string Description { get; set; }
    }
}
