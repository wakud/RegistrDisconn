using System;

namespace RegistrDisconnection.MyClasses
{
    /// <summary>
    /// клас групування даних по організації для звітів
    /// </summary>
    public class SelectedGrouping
    {
        public int CokId { get; set; }          //айді організації
        public string Cok { get; set; }         //назва організації
        public decimal? DebStart { get; set; }  //борг на початок періоду
        public decimal? CredStart { get; set; } //переплата на початок періоду
        public decimal? Narah { get; set; }     //нараховано
        public decimal? Opl { get; set; }       //оплачено
        public decimal? DebEnd { get; set; }    //заборгованість на кінець періоду
        public decimal? CredEnd { get; set; }   //переплата на кінець періоду
    }
}
