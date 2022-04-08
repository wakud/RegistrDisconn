using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RegistrDisconnection.Models.Abonents;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace RegistrDisconnection.MyClasses
{
    /// <summary>
    /// Утиліти для роботи з БД ОСР
    /// </summary>
    public static class BillingUtils
    {
        public static IConfiguration Configuration;

        /// <summary>
        /// виклик програми ОСР і перегляд даних
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static string GetESLink(Person person)
        {
            string Base = "AppName Utility/UserGuid 2546BACD-5568-4C18-AF72-7AE1ED522077";
            string OrganizationPart = string.Format("OrganizationGuid {0}", person.Cok.OrganizationUnitGUID);
            string DbConfigPart = string.Format("DbConfigName {0}", person.Cok.DbConfigName);
            string CommandPart = "Command ShowAccountInfoById";
            string AccIdPart = string.Format("AccountId {0}", person.AccountId);
            string data = string.Format(
                "{0}/{1}/{2}/{3}/{4}",
                Base,
                OrganizationPart,
                DbConfigPart,
                CommandPart,
                AccIdPart
            );
            byte[] EncodedData = Encoding.UTF8.GetBytes(data);
            string s1 = Convert.ToBase64String(EncodedData);
            return "es:" + s1;
        }

        /// <summary>
        /// Завантаження боржників з БД ОСР
        /// </summary>
        /// <param name="cokCode"></param>
        /// <param name="environment"></param>
        /// <param name="SumaBorgu"></param>
        /// <param name="CntMonth"></param>
        /// <param name="borhMonth"></param>
        /// <returns></returns>
        public static DataTable GetUtilResults(string cokCode, IWebHostEnvironment environment,
            string SumaBorgu, int CntMonth, int borhMonth)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string path;
            string script;
            path = cokCode == "ORG"
                ? environment.WebRootPath + "\\Scripts\\назва скрипту.sql"
                : environment.WebRootPath + "\\Scripts\\назва скрипту.sql";
            script = "USE " + cokCode + "Organization" + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            string connectionString = Configuration.GetConnectionString("OSRConnection");
            DataTable results = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(script, conn);
            conn.Open();
            //добавляємо параметр вибірки сума боргу
            SqlParameter borg = new SqlParameter("@SummBorh", SumaBorgu);
            _ = command.Parameters.Add(borg);
            //добавляємо параметр неоплати к-ть місяців
            SqlParameter notPay = new SqlParameter("@CntMonth", CntMonth);
            _ = command.Parameters.Add(notPay);
            //добавляємо к-ть місяців боргу
            SqlParameter borgMonth = new SqlParameter("@CntMonthBorh", borhMonth);
            _ = command.Parameters.Add(borgMonth);
            command.CommandTimeout = 1200;
            SqlDataReader reader = command.ExecuteReader();
            results.Load(reader);
            reader.Close();
            return results;
        }

        /// <summary>
        /// Завантаження всіх населених пунктів
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="RegionId"></param>
        /// <returns></returns>
        public static DataTable GetCity(IWebHostEnvironment environment, int RegionId)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string path;
            path = environment.WebRootPath + "\\Scripts\\City.sql";
            string script;
            script = "USE Organization" + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            string connectionString = Configuration.GetConnectionString("OSRConnection");
            DataTable results = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(script, conn);
            conn.Open();
            _ = command.Parameters.AddWithValue("region", RegionId);
            command.CommandTimeout = 600;
            SqlDataReader reader = command.ExecuteReader();
            results.Load(reader);
            reader.Close();
            return results;
        }

        /// <summary>
        /// Завантаження населених пунктів по айді
        /// </summary>
        /// <param name="env"></param>
        /// <param name="CityId"></param>
        /// <returns></returns>
        public static DataTable GetCityById(IWebHostEnvironment env, int CityId)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string path;
            path = env.WebRootPath + "\\Scripts\\CityById.sql";
            string script;
            script = "USE Organization" + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            string connectionString = Configuration.GetConnectionString("OSRConnection");
            DataTable results = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(script, conn);
            conn.Open();
            _ = command.Parameters.AddWithValue("CityId", CityId);
            command.CommandTimeout = 1200;
            SqlDataReader reader = command.ExecuteReader();
            results.Load(reader);
            reader.Close();
            return results;
        }

        /// <summary>
        /// Поновлення боржників
        /// </summary>
        /// <param name="env"></param>
        /// <param name="people"></param>
        /// <param name="cokCode"></param>
        /// <returns></returns>
        public static DataTable UpdatePoper(IWebHostEnvironment env, List<Person> people, string cokCode)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string path;
            path = env.WebRootPath + "\\Scripts\\UpdatePoper.sql";
            string script;
            script = "USE " + cokCode + "Organization" + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            List<string> personIds = new List<string>();
            script += "'";
            foreach (Person p in people)
            {
                personIds.Add(p.AccountId);
            }
            script += string.Join("','", personIds);
            script += "')";
            string connectionString = Configuration.GetConnectionString("OSRConnection");
            DataTable results = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(script, conn);
            conn.Open();
            command.CommandTimeout = 1200;
            SqlDataReader reader = command.ExecuteReader();
            results.Load(reader);
            reader.Close();
            return results;
        }

        /// <summary>
        /// Поновлення відключених
        /// </summary>
        /// <param name="env"></param>
        /// <param name="people"></param>
        /// <param name="cokCode"></param>
        /// <returns></returns>
        public static DataTable UpdateVykl(IWebHostEnvironment env, List<ActualDataPerson> people, string cokCode)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string path;
            path = env.WebRootPath + "\\Scripts\\UpdateVykl.sql";
            string script;
            script = "USE " + cokCode + "Organization" + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            List<string> personIds = new List<string>();
            script += "'";
            foreach (ActualDataPerson p in people)
            {
                personIds.Add(p.Person.AccountId);
            }
            script += string.Join("','", personIds);
            script += "')";
            string connectionString = Configuration.GetConnectionString("OSRConnection");
            DataTable results = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(script, conn);
            _ = command.Parameters.AddWithValue("AccNumber", script);
            conn.Open();
            command.CommandTimeout = 1200;
            SqlDataReader reader = command.ExecuteReader();
            results.Load(reader);
            reader.Close();
            return results;
        }

        /// <summary>
        /// Завантаження одного (вибраного) абонента боржника
        /// </summary>
        /// <param name="env"></param>
        /// <param name="OsRah"></param>
        /// <param name="cokCode"></param>
        /// <returns></returns>
        public static DataTable AddAbons(IWebHostEnvironment env, string OsRah, string cokCode)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string path = cokCode == "ORG"
                ? env.WebRootPath + "\\Scripts\\скрипт SQL.sql"
                : env.WebRootPath + "\\Scripts\\скрипт SQL.sql";
            string script;
            script = "USE " + cokCode + "Organization" + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            _ = new List<string>();
            script += " WHERE a.AccountNumber = '" + OsRah.Trim() + "'";
            string connectionString = Configuration.GetConnectionString("OSRConnection");
            DataTable results = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(script, conn);
            conn.Open();
            command.CommandTimeout = 1200;
            SqlDataReader reader = command.ExecuteReader();
            results.Load(reader);
            reader.Close();
            return results;
        }

        /// <summary>
        /// Завантаження інформації для звіту 1
        /// </summary>
        /// <param name="env"></param>
        /// <param name="cokCode"></param>
        /// <param name="stanom_na"></param>
        /// <returns></returns>
        public static DataTable Zvit1(IWebHostEnvironment env, string cokCode, string stanom_na)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string path;
            string script;
            path = env.WebRootPath + "\\Scripts\\Zvit1.sql";
            script = "USE " + cokCode + "Organization" + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            string connectionString = Configuration.GetConnectionString("OSRConnection");
            DataTable results = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(script, conn);
            conn.Open();
            _ = command.Parameters.AddWithValue("stanom_na", stanom_na);
            command.CommandTimeout = 1200;
            SqlDataReader reader = command.ExecuteReader();
            results.Load(reader);
            reader.Close();
            return results;
        }

        /// <summary>
        /// Звіт по виданим попередженням
        /// </summary>
        /// <param name="env"></param>
        /// <param name="people"></param>
        /// <param name="cokCode"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public static DataTable Zvit4(IWebHostEnvironment env,
            List<ActualDataPerson> people,
            string cokCode, DateTime dateFrom, DateTime dateTo)
        {
            List<string> Inserts = new List<string>();
            foreach (ActualDataPerson p in people)
            {
                Inserts.Add(
                    string.Format(
                        "INSERT @table VALUES ('{0}') ", p.Person.AccountId
                    )
               );
            }
            string InsertScript = string.Join("\n", Inserts);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string path;
            string script;
            path = env.WebRootPath + "\\Scripts\\Zvit4.sql";
            script = "USE " + cokCode + "Organization" + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            script = script.Replace("$params$", InsertScript);
            string connectionString = Configuration.GetConnectionString("OSRConnection");
            DataTable results = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(script, conn);
            conn.Open();
            _ = command.Parameters.AddWithValue("dateFrom", dateFrom);
            _ = command.Parameters.AddWithValue("dateTo", dateTo);
            command.CommandTimeout = 1200;
            SqlDataReader reader = command.ExecuteReader();
            results.Load(reader);
            reader.Close();
            return results;
        }

        /// <summary>
        /// Завантаження вибраного абонента, якого відключили
        /// </summary>
        /// <param name="env"></param>
        /// <param name="OsRah"></param>
        /// <param name="cokCode"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static DataTable AddVykl(IWebHostEnvironment env, string OsRah, string cokCode, string period)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string path = cokCode == "ORG"
                ? env.WebRootPath + "\\Scripts\\Пошук відключених1.sql"
                : env.WebRootPath + "\\Scripts\\Пошук відключених2.sql";
            string script;
            script = "USE " + cokCode + "Organization" + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            _ = new List<string>();
            script += " WHERE [Повна адреса] IS NOT NULL";
            string connectionString = Configuration.GetConnectionString("OSRConnection");
            DataTable results = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(script, conn);
            conn.Open();
            _ = command.Parameters.AddWithValue("AccNumber", OsRah);
            _ = command.Parameters.AddWithValue("period", period);
            command.CommandTimeout = 1200;
            SqlDataReader reader = command.ExecuteReader();
            results.Load(reader);
            reader.Close();
            return results;
        }

        /// <summary>
        /// Завантаження всіх відключених абонентів з БД ОСР
        /// </summary>
        /// <param name="cokCode"></param>
        /// <param name="environment"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static DataTable GetUtilVykl(string cokCode, IWebHostEnvironment environment, string period)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string path = cokCode == "ORG"
                ? environment.WebRootPath + "\\Scripts\\Пошук відключених1.sql"
                : environment.WebRootPath + "\\Scripts\\Пошук відключених2.sql";
            string script;
            script = "USE " + cokCode + "Organization" + "\n";
            script += File.ReadAllText(path, Encoding.GetEncoding(1251));
            script += " WHERE [дата викл.] <> '' AND [сума боргу] IS NOT NULL " +
                "AND [Повна адреса] IS NOT NULL";
            string connectionString = Configuration.GetConnectionString("OSRConnection");
            DataTable results = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(script, conn);
            conn.Open();
            _ = command.Parameters.AddWithValue("period", period);
            command.CommandTimeout = 1200;
            SqlDataReader reader = command.ExecuteReader();
            results.Load(reader);
            reader.Close();
            return results;
        }
    }
}
