
namespace RegistrDisconnection.Models.Operations
{
    /// <summary>
    /// таблиця оновлень відключень
    /// </summary>
    public class UpdateVykl
    {
        public int Id { get; set; }
        public string OrganizationCode { get; set; }    //код цоку
        public string AccountNumber { get; set; }       //особовий
        public decimal? RestSumm { get; set; }          //сума боргу
        public bool Disconnection { get; set; }         //статус виключення
        public string DateDiscon { get; set; }          //дата виключення
    }
}
