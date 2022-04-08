using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.MyClasses;
using System.Collections.Generic;

namespace RegistrDisconnection.ViewModels
{
    public class ViewArhiv
    {
        public int Id { get; set; }
        public string OsRah { get; set; }                   //особовий рахунок абонента
        public string FullName { get; set; }                //ПІП абонента
        public string FullAddress { get; set; }             //адреса абонента.
        public string CokCode { get; set; }                 //Код ЦОКу
        public decimal? DebPoch { get; set; }       //борг на початок періоду
        public decimal? KredPoch { get; set; }      //переплата на початок періоду
        public decimal? SumaVykl { get; set; }      //оплата за відключення
        public decimal? SumaVkl { get; set; }       //оплата за підключення
        public decimal? Oplata { get; set; }        //оплата протягом періоду
        public decimal? DebKin { get; set; }        //борг на кінець періоду
        public decimal? KredKin { get; set; }       //переплата на кінець періоду
        public int? AktPeriod { get; set; }         //активний період
        public int? ArchPeriod { get; set; }       //період 

        public string PeriodStr { get; set; }       //період прописом
        public int Period { get; set; }     //текучий період інтежер
        public string SearchPeriod { get; set; }       //період для відображення у календарі вюхи
        public string ZakrPeriod { get; set; }      //закритий період
        public string Name { get; set; }            //назва або особовий для пошуку

        public Dictionary<int, SelectedGrouping> Selected { get; set; }

        public IEnumerable<Saldo> MainContexts { get; set; }        //список абонентів виключених з вибраним періодом
        public IEnumerable<ActualDataPerson> ActualContexts { get; set; }      //список абонентів виключених з актуальними даними
        public ViewArhiv PageViewModel { get; set; }    //інформація про пагінацію
    }
}
