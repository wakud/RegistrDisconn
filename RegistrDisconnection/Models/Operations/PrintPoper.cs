namespace RegistrDisconnection.Models.Operations
{
    /// <summary>
    /// друк попереджень
    /// </summary>
    public class PrintPoper
    {
        public int? Id { get; set; }
        public string? OrganizationCode { get; set; }       //код організації
        public string? OrganizationName { get; set; }       //назва організації
        public string? OrganizationNameDoc { get; set; }    //назва організації для документів (в родовому)
        public string? RozRah { get; set; }     //р/р організації
        public string? Tel { get; set; }    //телефон організації
        public string? Edrpou { get; set; }     //ЕДРПОУ організації
        public string? Nach { get; set; }   //ПІП начальника
        public string? Vykonavets { get; set; } //хто виписував попередження
        public string? OrgAdres { get; set; }   //адреса організації
        public string? OrgIndex { get; set; }   //індекс поштовий організації
        public string? OsRah { get; set; }      //особовий абонента в ОСР
        public string? NewOsRah { get; set; }   //особовий в програмі
        public string? FullName { get; set; }   //ПІП абонента
        public string? LastName { get; set; }   //фамілія абонента
        public string? FirstName { get; set; }  //ім'я
        public string? SecondName { get; set; } //по батькові
        public string? FullAddress { get; set; }    //адреса
        public int? PostalCode { get; set; }    //індекс
        public string? District { get; set; }   //район
        public string? Region { get; set; }     //область
        public string? CityType { get; set; }   //тип нас. пункту
        public string? CityTypeShot { get; set; }   //скорочено тип нас. пункту (м., с., смт.)
        public string? CityName { get; set; }       //назва нас. пункту
        public string? DirectionName { get; set; }  //назва напрямку
        public string? StreetTypeShortName { get; set; }    //скорочено тип вулиці (вул., пр-т, б-р і т.д.)
        public string? StreetType { get; set; }     //тип вулиці
        public string? StreetName { get; set; }     //назва вулиці
        public string? Building { get; set; }       //будинок
        public string? BuildingPart { get; set; }   //літера будинку
        public string? Apartment { get; set; }      //квартира
        public decimal? RestSumm { get; set; }      //сума боргу
        public int? PeriodRestSumm { get; set; }    //період боргу
        public string? CounterNumber { get; set; }  //обліковий засіб
        public string? EIS { get; set; }        //ЕІС
        public string? DateVykl { get; set; }   //дата відключення
        public string? StanomNaDate { get; set; }   //дата формування
        public string? PoperDate { get; set; }      //дата попередження
        public int? PoperNum { get; set; }      //номер попередження
    }
}
