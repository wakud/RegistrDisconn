using System;

namespace RegistrDisconnection.Models
{
    /// <summary>
    /// Клас для формування періоду в числове, стрічкове, дата формати
    /// </summary>
    public class Period
    {
        public int Per_int { get; set; }        //період в числовому
        public string Per_str { get; set; }     //період  в символьному
        public DateTime Per_date { get; set; }  //період як дата

        //період у форматі числовому (012021)
        public Period(int per_int)
        {
            Per_int = per_int;
            Per_str = per_int.ToString();
            int rik = int.Parse(per_int.ToString().Trim().Substring(0, 4));
            int mis = int.Parse(per_int.ToString().Trim().Substring(4, 2));
            Per_date = new DateTime(rik, mis, 1);
        }

        //добавлення нуля в до місяців менше 10 (01, 02, 03 ... , 09)
        public string True_month(int mis)
        {
            return mis < 10 ? "0" + mis.ToString() : mis.ToString();
        }

        //період у форматі дата
        public Period(DateTime per_date)
        {
            Per_date = per_date;
            int rik = per_date.Year;
            int mis = per_date.Month;
            Per_str = rik.ToString() + True_month(mis);
            Per_int = int.Parse(Per_str);
        }

        //період у форматі текстовому
        public Period(string per_str)
        {
            Per_int = int.Parse(per_str);
            Per_str = per_str;
            int rik = int.Parse(Per_int.ToString().Trim().Substring(0, 4));
            int mis = int.Parse(Per_int.ToString().Trim().Substring(4, 2));
            Per_date = new DateTime(rik, mis, 1);
        }

        //отримання останнього дня попереднього місяця
        public static Period WithOffset(DateTime? start = null, int offset = -1)
        {
            Period startPeriod = start == null ? Per_now() : new Period((DateTime)start);
            DateTime PrevPer = startPeriod.Per_date.AddMonths(offset);
            return new Period(PrevPer);
        }

        //визначення періоду зараз
        public static Period Per_now()
        {
            DateTime from_date = DateTime.Now;
            return new Period(from_date);
        }
    }
}
