using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Dictionaries
{
    /// <summary>
    /// напрямки ОСР
    /// </summary>
    [Table("DirectionDict")]
    public class DirectionDict
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string Name { get; set; }            //назва напрямку

        [ForeignKey("CokId")]
        public int? CokId { get; set; }             //код організації

        public int? ParentDirectionId { get; set; }  //дочірній напрямок

        public virtual Organization Cok { get; set; }

        public List<DirectionCityMap> DirectionCityMaps { get; set; }

        public DirectionDict()
        {
            DirectionCityMaps = new List<DirectionCityMap>();
        }
    }
}
