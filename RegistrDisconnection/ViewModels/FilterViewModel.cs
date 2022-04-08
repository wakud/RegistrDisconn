using RegistrDisconnection.Models.Abonents;

namespace RegistrDisconnection.ViewModels
{
    /// <summary>
    /// вивід абонентів згідно напрямку
    /// </summary>
    public class FilterViewModel
    {
        public ActualDataPerson DataPerson { get; set; }
        public int? Direction { get; set; }
    }
}
