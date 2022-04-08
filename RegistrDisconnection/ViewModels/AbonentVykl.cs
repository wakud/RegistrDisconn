using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.MyClasses;
using System.Collections.Generic;

namespace RegistrDisconnection.ViewModels
{
    /// <summary>
    /// модель для перегляду відключених абонентів
    /// </summary>
    public class AbonentVykl
    {
        public IEnumerable<ActualDataPerson> MainContext { get; set; }      //список абонентів виключених з актуальними даними
        public int Period { get; set; }     //текучий період інтежер
        public string ZakrPeriod { get; set; }      //закритий період
        public string PeriodStr { get; set; }       //період прописом
        public PageViewModel PageViewModel { get; set; }    //інформація про пагінацію
    }
}
