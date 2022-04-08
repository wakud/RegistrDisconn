
namespace RegistrDisconnection.ViewModels
{
    public enum SortState
    {
        OsRahAsc,           //по особовому по зростанню
        OsRahDesc,          //по особовому по спаданню
        PIPAsc,             //по ПІП по зростанню
        PIPDesc,            //по ПІП по спаданню
        AdresaAsc,          //по адресі
        AdresaDesc,
        DebetPochAsc,       //по боргу на початок
        DebetPochDesc,
        KredytPochAsc,      //по переплаті на початок
        KredytPochDesc,
        SumaVyklAsc,        //сума виключення
        SumaVyklDesc,
        SumaVklAsc,         //сума включення
        SumaVklDesc,
        OplataAsc,          //оплата
        OplataDesc,
        DebetKinAsc,        //по боргу на кінець
        DebetKinDesc,
        KredytKinAsc,       //по переплаті на кінець
        KredytKinDesc
    }
}
