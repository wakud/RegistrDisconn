
namespace RegistrDisconnection.ViewModels
{
    /// <summary>
    /// для друку актів і рахкнку
    /// </summary>
    public class RahEndAkt
    {
        public string Data { get; set; }        //дата документу
        public string OsRah { get; set; }       //особовий
        public string FullName { get; set; }    //ПІП абонента
        public string FullAddress { get; set; } //адреса абонента
        public string CinaBezPdv { get; set; }  //ціна баз ПДВ
        public string SumaBezPdv { get; set; }  //сума без ПДВ
        public string PDV { get; set; }         //ПДВ
        public string Vsogo { get; set; }       //всього
        public string SumaStr { get; set; }     //сума прописом
        public string Vykon { get; set; }       //хто виписав
    }
}
