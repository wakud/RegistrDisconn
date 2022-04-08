using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.Models.Dictionaries;
using RegistrDisconnection.Models.Operations;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Data;
using System.Globalization;
using System.Linq;
using System.IO;

namespace RegistrDisconnection.MyClasses
{
    /// <summary>
    /// клас утиліт для програми
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// це для екселя наступна буква колонки
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public static char GetNextLetterExcel(char letter)
        {
            char nextChar = letter == 'z'
                ? 'a'
                : letter == 'Z'
                ? 'A'
                : (char)(letter + 1);
            return nextChar;
        }

        /// <summary>
        /// вивід інформації табличкою в вюшці
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table>";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            }

            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                }

                html += "</tr>";
            }
            html += "</table>";
            return html;
        }

        /// <summary>
        /// Утиліта для визначення дати відключення
        /// </summary>
        /// <param name="Holidays"></param>
        /// <param name="dt"></param>
        /// <param name="dates"></param>
        /// <returns></returns>
        public static DateTime GetFreePoperDay(DbSet<VyhAndSviat> Holidays, DateTime dt, IQueryable<PoperDrukGroup> dates)
        {
            Console.OutputEncoding = Encoding.GetEncoding(1251);
            DateTime current = dt; // К-сть днів
            int currentDay = 1;
            int endDay = 19;    //було 16
            //Вираховуємо 16 робочих днів + 3 календарні дні, для дати відключення
            while (currentDay < endDay)
            {
                if (currentDay >= 7 && currentDay <= 9)
                {
                    currentDay += 2;
                    current = current.AddDays(2);
                }

                VyhAndSviat existedHoliday = Holidays.FirstOrDefault(
                     eh => (eh.Year == null || current.Year == eh.Year) &&
                             (eh.Month == null || current.Month == eh.Month) &&
                             (eh.Day == null || current.Day == eh.Day)
                 );

                VyhAndSviat exHoliday = Holidays.FirstOrDefault(h => h.Id == 1);
                bool isNotSaturday = current.DayOfWeek != DayOfWeek.Saturday;
                bool isNotSunday = current.DayOfWeek != DayOfWeek.Sunday;

                if (existedHoliday == null && isNotSaturday && isNotSunday)
                {
                    currentDay += 1;
                }
                current = current.AddDays(1);   //якщо все добре добавляємо день
            }
            current = current.AddDays(1);

            //Перевіряємо чи дата відключення не припадає на пятницю, суботу, неділю і на святковий день
            while (true)
            {
                VyhAndSviat existedHoliday = Holidays.FirstOrDefault(
                     eh => (eh.Year == null || current.Year == eh.Year) &&
                             (eh.Month == null || current.Month == eh.Month) &&
                             (eh.Day == null || current.Day == eh.Day)
                 );
                //Перевірка дати на співпадання дат виданих попереджень
                foreach (PoperDrukGroup item in dates)
                {
                    if (current == item.Vykl)
                    {
                        current = current.AddDays(1);
                    }
                }

                VyhAndSviat exHoliday = Holidays.FirstOrDefault(h => h.Id == 1);
                bool isNotFriday = current.DayOfWeek != DayOfWeek.Friday;
                bool isNotSaturday = current.DayOfWeek != DayOfWeek.Saturday;
                bool isNotSunday = current.DayOfWeek != DayOfWeek.Sunday;

                if (existedHoliday == null && isNotFriday && isNotSaturday && isNotSunday)
                {
                    return current;
                }

                current = current.AddDays(1);
            }
        }

        /// <summary>
        /// Утиліта для формування списку людей для попередження
        /// </summary>
        /// <param name="item"></param>
        /// <param name="vykonavets"></param>
        /// <returns></returns>
        public static PrintPoper CreatePrintPoperFromActual(ActualDataPerson item, string vykonavets)
        {
            CultureInfo ukUa = new CultureInfo("uk-UA");
            string NameDoc;
            if (item.Person.Cok.Code == "TR40" || item.Person.Cok.Code == "TR39")
            {
                NameDoc = item.Person.Cok.Name.Remove(12) + "ого ЦОК";
            }
            else
            {
                NameDoc = item.Person.Cok.Name.Trim();
                NameDoc = NameDoc.Remove(NameDoc.Length - 6) + "ого ЦОК";
            }
            return new PrintPoper
            {
                //абонент табл персон
                OsRah = item.Person.OsRah,
                NewOsRah = item.Person.NewOsRah,
                FullName = item.Person.FullName,
                FirstName = item.Person.FirsName,
                LastName = item.Person.LastName,
                SecondName = item.Person.SecondName,
                //адреса абонента
                PostalCode = item.Person.Address.PostalCode,
                FullAddress = item.Person.FullAddress,
                District = item.Person.Address.District,
                Region = item.Person.Address.Region,
                CityName = item.Person.Address.CityName,
                CityType = item.Person.Address.CityType,
                CityTypeShot = item.Person.Address.CityType,
                StreetName = item.Person.Address.StreetName,
                StreetType = item.Person.Address.StreetType,
                StreetTypeShortName = item.Person.Address.StreetTypeShortName,
                Building = item.Person.Address.Building,
                BuildingPart = item.Person.Address.BuildingPart,
                Apartment = item.Person.Address.Apartment,
                //лічильник
                CounterNumber = item.Lichylnyk.Number,
                EIS = item.Lichylnyk.EIS,
                //фінанси
                RestSumm = item.Finance.DebPoch,
                PeriodRestSumm = (int)item.Finance.Period,
                //попередження
                PoperDate = ((DateTime)item.Poperedgenia.Poper).ToString("D", ukUa),
                StanomNaDate = ((DateTime)item.Poperedgenia.StanNa).ToString("d", ukUa),
                DirectionName = item.Person.Address.Direction.Name,
                PoperNum = item.Poperedgenia.Id,
                //відключення
                DateVykl = ((DateTime)item.Poperedgenia.DateVykl).ToString("D", ukUa),
                //ін-ція з таблиці цок
                OrganizationCode = item.Person.Cok.Code,
                OrganizationName = item.Person.Cok.Name,
                OrganizationNameDoc = NameDoc,
                Tel = item.Person.Cok.Tel,
                Nach = item.Person.Cok.Nach,
                Vykonavets = vykonavets,
                OrgAdres = item.Person.Cok.Address,
                OrgIndex = item.Person.Cok.Index,
                RozRah = item.Person.Cok.RozRah.Substring(0, 30),
                Edrpou = item.Person.Cok.EDRPOU
            };
        }

        /// <summary>
        /// Утиліта для розрахунку сальда
        /// </summary>
        /// <param name="saldo"></param>
        public static void CalculateSaldo(Saldo saldo)
        {
            //Без активної е/е
            decimal? narah = saldo.DebPoch + saldo.SumaVkl + saldo.SumaVykl;
            decimal? oplat = saldo.KredPoch + saldo.Oplata;
            decimal? saldoEnd = narah - oplat;

            saldo.DebKin = 0;
            saldo.KredKin = 0;

            if (saldoEnd > 0)
            {
                saldo.DebKin = saldoEnd;
            }
            else
            {
                saldo.KredKin = saldoEnd * -1;
            }
        }

        /// <summary>
        /// Утиліта для кодування паролів
        /// </summary>
        /// <param name="clearText"></param>
        /// <returns></returns>
        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "abc123";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                clearText = Convert.ToBase64String(ms.ToArray());
            }
            return clearText;
        }

        /// <summary>
        /// Утиліта для розкодування паролів
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "abc123";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using MemoryStream ms = new MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
            return cipherText;
        }
    }
}
