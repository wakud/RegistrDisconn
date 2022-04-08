using RegistrDisconnection.Models;
using RegistrDisconnection.Models.Operations;
using System.Data;

namespace RegistrDisconnection.MyClasses
{
    public static class LoadDataCreator
    {
        public static LoadUtility CreateLoadData(DataRow row, string cokCode)
        {
            //створюємо новий екземпляр класа load згідно списку LoadUtility
            LoadUtility loadUtility = new LoadUtility
            {
                //то потрібно для табл person
                OrganizationCode = "TR" + row["Код ЦОК"].ToString().Trim(),
                AccountNumber = row["особовий"].ToString().Trim(),
                AccountNumberNew = row["новий особовий"].ToString().Trim(),
                AccountId = row["accId"].ToString().Trim(),
                FullName = row["ПІП"].ToString().Trim(),
                LastName = row["Прізвище"].ToString().Trim(),
                FirstName = row["Ім'я"].ToString().Trim(),
                SecondName = row["По батькові"].ToString().Trim(),
                FullAddress = row["Повна адреса"].ToString().Trim(),
                MobilePhoneNumber = row["Моб.тел"].ToString().Trim().Length <= 10
                    ? row["Моб.тел"].ToString().Trim()
                    : row["Моб.тел"].ToString().Trim().Substring(0, 10),
                IdentificationCode = row["Ідент.код"].ToString().Trim().Length <= 10
                    ? row["Ідент.код"].ToString().Trim()
                    : row["Ідент.код"].ToString().Trim().Substring(0, 10),
                Passport = row["Паспорт"].ToString().Trim().Length <= 12
                    ? row["Паспорт"].ToString().Trim()
                    : row["Паспорт"].ToString().Trim().Substring(0, 12),
                //то потрібно для табл adress
                AdresId = int.Parse(row["UtilityAddressId"].ToString().Trim()),
                PostalCode = row["Індекс"] != null && row["Індекс"].ToString().Trim() != ""
                    ? int.Parse(row["Індекс"].ToString().Trim())
                    : cokCode == "TR40" ? 46000 : 99999,
                District = row["область"].ToString().Trim(),
                Region = row["район"].ToString().Trim(),
                CityType = row["тип пункту"].ToString().Trim(),
                CityTypeShot = row["тип н.п."].ToString().Trim(),
                CityName = row["Нас.пункт"].ToString().Trim(),
                StreetTypeShortName = row["тип в"].ToString().Trim(),
                StreetType = row["тип вул"].ToString().Trim(),
                StreetName = row["вулиця"].ToString().Trim(),
                Building = row["будинок"].ToString().Trim().Length <= 4
                    ? row["будинок"].ToString().Trim()
                    : row["будинок"].ToString().Trim().Substring(4),
                BuildingPart = row["корпус"].ToString().Trim().Length <= 2
                    ? row["корпус"].ToString().Trim()
                    : row["корпус"].ToString().Trim().Substring(2),
                Apartment = row["квартира"].ToString().Trim().Length <= 5
                    ? row["квартира"].ToString().Trim()
                    : row["квартира"].ToString().Trim().Substring(5),
                //то потрібно для табл фінанси
                RestSumm = decimal.Parse(row["сума боргу"].ToString().Trim()),
                PeriodRestSumm = Period.Per_now().Per_int,
                //то потрібно для табл лічильник
                CounterNumber = row["№ лічильника"].ToString().Trim(),
                EIS = row["ЕІС"].ToString().Trim(),
                //то для таблиць вкл і викл
                DateDiscon = row["дата викл."].ToString().Trim(),
                //DatePay = row["ост. дата опл."].ToString().Trim(),
                //Oplata = decimal.Parse(row["сума оплати"].ToString().Trim()),
                //DeliverDate = row["дата попер."].ToString().Trim(),
            };

            if (cokCode == "TR40")
            {
                loadUtility.DirectionName = row["Напрямок"].ToString().Trim();
            }
            else
            {
                loadUtility.CityCode = int.Parse(row["Код нас.пункту"].ToString().Trim());
            }

            return loadUtility;
        }
    }
}
