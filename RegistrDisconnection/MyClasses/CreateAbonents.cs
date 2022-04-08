using RegistrDisconnection.Data;
using RegistrDisconnection.Models;
using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.Models.Dictionaries;
using RegistrDisconnection.Models.Operations;
using RegistrDisconnection.Models.Users;
using System;
using System.Linq;

namespace RegistrDisconnection.MyClasses
{
    /// <summary>
    /// Добавлення абонента в БД програми
    /// </summary>
    public class CreateAbonents
    {
        private readonly LoadUtility loadData;
        private readonly string cokCode;
        private readonly string SumaBorgu;
        private readonly int cokId;
        private readonly User currentUser;
        private readonly MainContext _context;

        private Address address;
        private Person person;
        private bool personExisted;
        private Finance finance;
        private Lichylnyk lichylnyk;
        private Vykl vykl;
        private Poperedgenia poper;
        private ActualDataPerson actualData;
        private readonly bool setActive;

        public CreateAbonents(string cokCode, int cokId, LoadUtility loadData, MainContext mainContext, User user, string SumaBorgu, bool setaktive)
        {
            this.cokCode = cokCode;
            this.cokId = cokId;
            this.loadData = loadData;
            _context = mainContext;
            currentUser = user;
            this.SumaBorgu = SumaBorgu;
            setActive = setaktive;
        }

        //наповнення адреси і проставлення напрямків
        public void CreateAddressWithDirection()
        {
            bool addressExisted = true;
            address = _context.Addresses.FirstOrDefault(
                a => a.CokId == cokId && a.UtilityAddressId == loadData.AdresId
             );
            if (address == null)
            {
                address = new Address();
                addressExisted = false;
            }

            address.PostalCode = loadData.PostalCode;
            address.CokId = (int)currentUser.CokId;
            address.District = loadData.District;
            address.Region = loadData.Region;
            address.CityType = loadData.CityType;
            address.CityTypeShort = loadData.CityTypeShot;
            address.UtilityAddressId = loadData.AdresId;
            address.CityId = loadData.CityCode;
            address.CityName = loadData.CityName;
            address.StreetTypeShortName = loadData.StreetTypeShortName;
            address.StreetType = loadData.StreetType;
            address.StreetName = loadData.StreetName;
            address.Building = loadData.Building;
            address.BuildingPart = loadData.BuildingPart;
            address.Apartment = loadData.Apartment.Trim();

            //Це для добавлення напрямків
            if (cokCode != "ORG")
            {
                DirectionCityMap dcm = _context.DirectionCityMaps.FirstOrDefault(dcm => dcm.UtilityCityId == address.CityId);
                address.DirectionDictId = dcm == null
                    ? _context.DirectionDicts.FirstOrDefault(
                        dd => dd.Cok.Code == cokCode && dd.Name == "Нульовий напрямок"
                    ).Id
                    : dcm.DirectionDictId;
            }
            else
            {
                DirectionDict dd = _context.DirectionDicts.FirstOrDefault(
                    dd => dd.Cok.Code == cokCode
                    && dd.Name.Trim().ToLower() == loadData.DirectionName.Trim().ToLower()
                );
                address.DirectionDictId = dd == null
                    ? _context.DirectionDicts.FirstOrDefault(
                        dd => dd.Cok.Code == cokCode && dd.Name == "Нульовий напрямок"
                    ).Id
                    : dd.ParentDirectionId != null ? (int)dd.ParentDirectionId : dd.Id;
            }

            //перевіряємо чи є вже такий запис, якщо немає то добавляємо новий, якщо є то апдейт
            if (!addressExisted)
            {
                _ = _context.Addresses.Add(address);        //добавляємо запис
            }
            _ = _context.SaveChanges();                 //зберігаємо в таблиці даний запис

        }

        //другою наповнюємо таблицю персон
        public void CreatePerson()
        {
            personExisted = true;
            person = _context.Persons.FirstOrDefault(
                p => p.AccountId == loadData.AccountId && p.Cok.Code == loadData.OrganizationCode
            );
            // null Якщо немає в базі
            if (person == null)
            {
                personExisted = false;
                person = new Person();
            }

            // Person Якщо є в базі
            person.Cok = _context.Coks.FirstOrDefault(c => c.Code == cokCode);
            person.OsRah = loadData.AccountNumber;
            person.NewOsRah = loadData.AccountNumberNew;
            person.AccountId = loadData.AccountId;
            person.FirsName = loadData.FirstName;
            person.LastName = loadData.LastName;
            person.SecondName = loadData.SecondName;
            person.FullName = loadData.FullName;
            person.FullAddress = loadData.FullAddress;
            person.MobilePhoneNumber = loadData.MobilePhoneNumber;
            person.IdentKod = loadData.IdentificationCode;
            person.Passport = loadData.Passport;
            person.AddressId = address.Id;

            if (loadData.DateDiscon != null && loadData.DateDiscon != "")
            {
                person.StatusAktyv = false;
            }
            else if (setActive)
            {
                person.StatusAktyv = true;
            }
            //перевіряємо чи є вже такий запис, якщо немає то добавляємо новий, якщо є то апдейт
            if (!personExisted)
            {
                _ = _context.Persons.Add(person);           //добавляємо запис
            }
            _ = _context.SaveChanges();                 //зберігаємо в таблиці добавлений запис
        }

        //третьою наповнюємо таблицю фінанси
        public void CreateFinance()
        {
            bool financeExisted = true;
            finance = _context.Finances.FirstOrDefault(f => f.PersonId == person.Id);
            // null Якщо немає в базі
            if (finance == null)
            {
                financeExisted = false;
                finance = new Finance();
            }

            finance.AktPer = Period.Per_now().Per_int;
            finance.DebPoch = loadData.RestSumm;
            finance.DebLoad = loadData.RestSumm;
            finance.KredPoch = 0;
            finance.Period = loadData.PeriodRestSumm;
            finance.PersonId = person.Id;

            if (!financeExisted)
            {
                _ = _context.Finances.Add(finance);           //добавляємо запис
            }
            _ = _context.SaveChanges();                 //зберігаємо в таблиці добавлений запис
        }

        //четвертою наповнюємо таблицю засобів обліку
        public void CreateLichylnyk()
        {
            bool lichylnykExisted = true;
            lichylnyk = _context.Lichylnyks
                .FirstOrDefault(l => l.Person.AccountId == loadData.AccountId
                    && l.Person.Cok.Code == loadData.OrganizationCode);
            // null Якщо немає в базі
            if (lichylnyk == null)
            {
                lichylnykExisted = false;
                lichylnyk = new Lichylnyk();
            }

            lichylnyk.Number = loadData.CounterNumber;
            lichylnyk.EIS = loadData.EIS;
            lichylnyk.Period = Period.Per_now().Per_int;
            lichylnyk.PersonId = person.Id;

            if (!lichylnykExisted)
            {
                _ = _context.Lichylnyks.Add(lichylnyk);           //добавляємо запис
            }
            _ = _context.SaveChanges();                 //зберігаємо в таблиці добавлений запис

        }

        //пятою наповнюємо таблицю відключення
        public void CreateVykl()
        {
            bool vyklExisted = true;
            vykl = _context.Vykls.FirstOrDefault(v => v.Lichylnyk.Number == loadData.CounterNumber
                                                    && v.Lichylnyk.Person.Cok.Code == loadData.OrganizationCode);

            // null Якщо немає в базі
            if (vykl == null)
            {
                vyklExisted = false;
                vykl = new Vykl();
            }

            if (loadData.DateDiscon != null && loadData.DateDiscon != "")
            {
                string iDate = loadData.DateDiscon.Trim();
                DateTime oDate = Convert.ToDateTime(iDate);
                vykl.DateVykl = oDate;
                vykl.Status = true;
                Period dateVyklPeriod = new Period(oDate);
                vykl.Period = dateVyklPeriod.Per_int;
            }
            else
            {
                vykl.Status = false;
                vykl.Period = Period.Per_now().Per_int;
            }
            vykl.LichylnykId = lichylnyk.Id;

            if (!vyklExisted)
            {
                _ = _context.Vykls.Add(vykl);           //добавляємо запис
            }
            _ = _context.SaveChanges();                 //зберігаємо в таблиці добавлений запис

        }

        //шостою наповнюємо таблицю попередження
        public void CreatePoper()
        {
            bool poperExisted = true;
            poper = _context.Poperedgenias.FirstOrDefault(p => p.Person.AccountId == loadData.AccountId
                    && p.Person.Cok.Code == loadData.OrganizationCode);
            // null Якщо немає в базі
            if (poper == null)
            {
                poperExisted = false;
                poper = new Poperedgenia();
            }
            poper.PersonId = person.Id;
            if ((poperExisted && !person.StatusAktyv) || !poperExisted)
            {
                poper.LoadDay = Period.Per_now().Per_date;
                poper.Period = Period.Per_now().Per_int;
                poper.VydanePoper = false;
            }
            if (!poperExisted)
            {
                poper.PoperDrukGroupId = 1;
                _ = _context.Poperedgenias.Add(poper);           //добавляємо запис
            }
            _ = _context.SaveChanges();                 //зберігаємо в таблиці добавлений запис

        }

        //Заповнення таблиці актуальних даних, згідно завантажених даних
        public ActualDataPerson CreateActualData()
        {
            bool actualExisted = true;
            if (personExisted)
            {
                actualData = _context.ActualDatas.FirstOrDefault(ad => ad.PersonId == person.Id);
                if (actualData == null)
                {
                    Console.WriteLine("DANGER! Person existed without actual data");
                    actualData = new ActualDataPerson();
                    actualExisted = false;
                }
            }
            else
            {
                actualData = new ActualDataPerson();
                actualExisted = false;
            }

            actualData.LoadSum = decimal.Parse(SumaBorgu);
            actualData.PersonId = person.Id;
            actualData.LichylnykId = lichylnyk.Id;
            actualData.VyklId = vykl.Id;
            actualData.PoperedgeniaId = poper.Id;
            actualData.FinanceId = finance.Id;

            if (!actualExisted)
            {
                _ = _context.ActualDatas.Add(actualData);
            }
            _ = _context.SaveChanges();
            return actualData;
        }

    }
}
