namespace RegistrDisconnection.Models.Zvity
{
    /// <summary>
    /// Звіт по відключеним абонентам
    /// </summary>
    public class ZvitVykl
    {
        public int Id { get; set; }
        public string? OsRah { get; set; }      //особовий
        public string? FullName { get; set; }   //ПІП
        public string? FullAdres { get; set; }  //адреса
        public string? DataVykl { get; set; }   //дата відключення
        public string? DataVkl { get; set; }    //дата підключення
        public decimal? DebPoch { get; set; }   //борг на початок періоду
        public decimal? DebKin { get; set; }    //борг на кінець періоду
        public decimal? KredPoch { get; set; }  //переплата на початок періоду
        public decimal? KredKin { get; set; }   //переплата на кінець періоду
        public decimal? Sumavykl { get; set; }  //сума за відключення
        public decimal? SumaVkl { get; set; }   //сума за підключення
        public decimal? Oplata { get; set; }    //оплата витрат на відкл. і підкл.
        public decimal? Borg { get; set; }      //сума боргу
        public decimal? BorgEE { get; set; }    //сума боргу за активну е/е
        public decimal? OplataEE { get; set; }  //оплата за активну е/е
    }
}
