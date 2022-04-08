using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistrDisconnection.Models.Operations
{
    /// <summary>
    /// таблиця оновлень фінансів
    /// </summary>
    [Table("UpdateGroup")]
    public class UpdateGroup
    {
        [Key]
        public int Id { get; set; }

        public DateTime DateUpdate { get; set; }    //дата коли робили апдейт

        public string UserUpd { get; set; }         //хто робив апдейт інфи по абонентам

        public bool RiznSum { get; set; }       //різниця між сумами так або ні

        public bool IsLoad { get; set; } = false;       //перевірка на завантаження

        public List<UpdateFinance> UpdateFinances { get; set; }

        public UpdateGroup()
        {
            UpdateFinances = new List<UpdateFinance>();
        }
    }
}
