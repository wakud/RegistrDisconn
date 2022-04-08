using RegistrDisconnection.Models;
using RegistrDisconnection.Models.Dictionaries;
using RegistrDisconnection.Models.Operations;
using RegistrDisconnection.Models.Users;
using System;
using System.Linq;

namespace RegistrDisconnection.Data
{
    /// <summary>
    /// Заповнюємо БД початковими даними
    /// </summary>
    public class DbInitialization
    {
        public static void Initial(MainContext context)
        {
            //Добавимо початкові дані в БД користувачів
            if (!context.Rights.Any())
            {
                Prava admin = new Prava { Name = "Адміністратор", Code = "0" };
                Prava user = new Prava { Name = "Користувач", Code = "1" };
                Prava buh = new Prava { Name = "Бухгалтер", Code = "2" };

                context.Rights.AddRange(admin, user, buh);
                _ = context.SaveChanges();
            }

            //Добавимо початкові дані в БД організації
            if (!context.Coks.Any())
            {
                Organization misto = new Organization
                {
                    Name = "Назва організації",
                    Code = "ORG",
                    Nach = "ПІП начальника",
                    Buh = "ПІП бухгалтера",
                    Address = "адреса організації",
                    Rajon = "територіальний район",
                    City = "місто",
                    Index = "індекс",
                    Oblast = "область",
                    RegionId = 1,   //код району
                    EDRPOU = "едрпоу",
                    MFO = "мфо",
                    RozRah = "UA , у банку ПАТ \"БАНК УКРАЇНИ\"",
                    IPN = "іпн",
                    NameREM = "ОСР",
                    OrganizationUnitGUID = "guid організації в БД",
                    DbConfigName = "назва БД",
                    CurrPeriod = Period.Per_now().Per_int,  //активний період
                    IsLoads = false     //чи було проведено завантаження в цьому місяці
                };

                context.Coks.AddRange(misto);
                _ = context.SaveChanges();
            }

            //Добавимо початкові дані в БД користувачів
            if (!context.Users.Any())
            {
                Prava admin = context.Rights.FirstOrDefault(a => a.Code == "0");
                Prava user = context.Rights.FirstOrDefault(a => a.Code == "1");
                Prava buh = context.Rights.FirstOrDefault(a => a.Code == "2");

                User adm = new User { FullName = "admin", Login = "admin", Password = "Qwerty123", Prava = admin };
                User buhal = new User { FullName = "GolBuh", Login = "buh", Password = "Qwerty0147", Prava = buh };

                context.Users.AddRange(adm, buhal);
                _ = context.SaveChanges();
            }

            //Добавимо початкові дані в таблицю напрямків
            if (!context.DirectionDicts.Any())
            {
                Organization selo = context.Coks.FirstOrDefault(o => o.Code == "ORG");
                if (selo != null)
                {
                    DirectionDict directionDict = new DirectionDict { Cok = selo, Name = "Нульовий напрямок" };
                    _ = context.DirectionDicts.Add(directionDict);
                    _ = context.SaveChanges();
                    DirectionDict directionDict1 = new DirectionDict { Cok = selo, Name = "Напрямок 1" };
                    _ = context.DirectionDicts.Add(directionDict1);
                    _ = context.SaveChanges();
                    DirectionDict directionDict2 = new DirectionDict { Cok = selo, Name = "Напрямок 2" };
                    _ = context.DirectionDicts.Add(directionDict2);
                    _ = context.SaveChanges();
                    DirectionDict directionDict3 = new DirectionDict { Cok = selo, Name = "Напрямок 3" };
                    _ = context.DirectionDicts.Add(directionDict3);
                    _ = context.SaveChanges();
                    DirectionDict directionDict4 = new DirectionDict { Cok = selo, Name = "Напрямок 4" };
                    _ = context.DirectionDicts.Add(directionDict4);
                    _ = context.SaveChanges();
                    DirectionDict directionDict5 = new DirectionDict { Cok = selo, Name = "Напрямок 5" };
                    _ = context.DirectionDicts.Add(directionDict5);
                    _ = context.SaveChanges();
                    DirectionDict directionDict6 = new DirectionDict { Cok = selo, Name = "Напрямок 6" };
                    _ = context.DirectionDicts.Add(directionDict6);
                    _ = context.SaveChanges();
                    DirectionDict directionDict7 = new DirectionDict { Cok = selo, Name = "Напрямок 7" };
                    _ = context.DirectionDicts.Add(directionDict7);
                    _ = context.SaveChanges();
                }
            }

            //Добавимо вихідні і святкові дні
            if (!context.VyhAndSviats.Any())
            {
                VyhAndSviat newyear = new VyhAndSviat { Date = new DateTime(2020, 01, 01), Year = null, Month = 1, Day = 1, Name = "Новий рік" };
                VyhAndSviat Christmas = new VyhAndSviat { Date = new DateTime(2020, 01, 07), Year = null, Month = 1, Day = 7, Name = "Різдво" };
                VyhAndSviat WomenDay = new VyhAndSviat { Date = new DateTime(2020, 03, 08), Year = null, Month = 3, Day = 8, Name = "8 березня" };
                VyhAndSviat laborDay = new VyhAndSviat { Date = new DateTime(2020, 05, 01), Year = null, Month = 5, Day = 1, Name = "День праці" };
                VyhAndSviat VictoryDay = new VyhAndSviat { Date = new DateTime(2020, 05, 09), Year = null, Month = 5, Day = 9, Name = "День перемоги" };
                VyhAndSviat Trinity = new VyhAndSviat { Date = new DateTime(2020, 06, 07), Year = null, Month = 6, Day = 7, Name = "Трійця" };
                VyhAndSviat Constitution = new VyhAndSviat { Date = new DateTime(2020, 06, 28), Year = null, Month = 6, Day = 28, Name = "День Конcтитуції України" };
                VyhAndSviat IndependenceDay = new VyhAndSviat { Date = new DateTime(2020, 08, 24), Year = null, Month = 8, Day = 24, Name = "День незaлежності України" };
                VyhAndSviat DayofDefenders = new VyhAndSviat { Date = new DateTime(2020, 10, 14), Year = null, Month = 10, Day = 14, Name = "День захисників України" };
                VyhAndSviat Christmas1 = new VyhAndSviat { Date = new DateTime(2020, 12, 25), Year = null, Month = 12, Day = 25, Name = "Різдво Христове" };
                VyhAndSviat lastDayOfYear = new VyhAndSviat { Date = new DateTime(2020, 12, 31), Year = null, Month = 12, Day = 31, Name = "Перед новим роком" };
                context.VyhAndSviats.AddRange(newyear, Christmas, WomenDay, laborDay, VictoryDay, Trinity, Constitution, IndependenceDay, DayofDefenders, Christmas1, lastDayOfYear);
                _ = context.SaveChanges();
            }

            //добавимо групу попереджень для завантаження
            if (!context.DrukGroups.Any())
            {
                PoperDrukGroup poperDruk = new PoperDrukGroup
                {
                    DirectionName = "Це для заповнення(не видаляти)",
                    CountAbon = 0,
                    Stanomna = DateTime.Now,
                    VydanePoper = DateTime.Now,
                    Vykl = DateTime.Now
                };
                _ = context.DrukGroups.Add(poperDruk);
                _ = context.SaveChanges();
            }
        }
    }
}

