namespace RegistrDisconnection.Models.Zvity
{
    /// <summary>
    /// Звіт по виданим попередженням
    /// </summary>
    public class Zvit1
    {
        public int Id { get; set; }
        public string? OsRah { get; set; }      //особовий
        public int? AccountId { get; set; }     //айді в БД ОСР
        public string? FullName { get; set; }   //ПІП абонента
        public string? FullAdres { get; set; }  //адреса абонента
        public decimal? Borg { get; set; }      //сума боргу
        public decimal? DebLoad { get; set; }   //завантажена сума боргу
        public int? Month { get; set; }         //місяць завантаження
        public string? DataPoper { get; set; }  //дата видачі попередження
        public int CountPoper { get; set; }     //номер попередження
        public string? DataVykl { get; set; }   //прогнозована дата відключення
        public string? DataVyklOsr { get; set; }    //дата коли відключив ОСР
    }
}
