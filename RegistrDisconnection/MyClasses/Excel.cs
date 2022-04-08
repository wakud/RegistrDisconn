using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using RegistrDisconnection.Data;
using RegistrDisconnection.Models.Abonents;
using RegistrDisconnection.Models.Operations;
using RegistrDisconnection.Models.Users;
using RegistrDisconnection.Models.Zvity;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace RegistrDisconnection.MyClasses
{
    public class Excel
    {
        private readonly XLWorkbook wb;
        private IXLWorksheet ws;
        private readonly MainContext _context;

        public Excel(MainContext context)
        {
            _context = context;
            wb = new XLWorkbook();
        }

        private DateTime CreateStanomNa(string stanom_na)
        {
            return DateTime.Parse(stanom_na);
        }

        private string CreateNameDoc(User currentUser)
        {
            string NameDoc;
            if (currentUser.Login == "buh")
            {
                NameDoc = "ТОВ \"Тернопільелектропостач\"";
            }
            else
            {
                if (currentUser.Cok.Code == "TR40" || currentUser.Cok.Code == "TR39")
                {
                    NameDoc = currentUser.Cok.Name.Remove(12) + "ому ЦОКу";
                }
                else
                {
                    NameDoc = currentUser.Cok.Name.Trim();
                    NameDoc = NameDoc.Remove(NameDoc.Length - 6) + "ому ЦОКу";
                }
            }

            return NameDoc;
        }

        private void SetDefaultSettings()
        {
            ws.Name = "Звіт";
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;  //ставимо альбомну сторінку
            ws.PageSetup.AdjustTo(80);
            ws.PageSetup.Margins.Left = 0.6;
            ws.PageSetup.Margins.Right = 0.4;
            ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
            ws.PageSetup.VerticalDpi = 600;
            ws.PageSetup.HorizontalDpi = 600;
        }

        private void SetZvitHeader(string value)
        {
            ws.Cell("A1").Value = value;
            //ws.Cell("A1").Style
        }

        public void CreateZvit1(List<Zvit1> zvits, User user, string stanom_na)
        {
            string NameDoc = CreateNameDoc(user);
            DateTime DtStanom = CreateStanomNa(stanom_na);
            ws = wb.Worksheets.Add();
            ExcelStyling styler = new ExcelStyling(ws);  // Створюємо інстанс стиліста 
            SetDefaultSettings(); // Встановлюємо базові налаштування для Сторінки
            //шапка документу
            SetZvitHeader(
                "Інформація щодо заборгованості за спожиту електроенергію населенням " +
                "(від 1187 грн та/або до 1187 грн з терміном виникнення 3 місяці і більше)"
            );
            styler.CenterAndMerge("A1", "H1"); // Центруємо перший параметр комірку, і з'єднуємо її з другим параметром
            ws.Cell("A2").Value = "станом на " + DtStanom.ToString("dd MMMM yyyy") + " р. по " + NameDoc;
            styler.CenterAndMerge("A2", "H2");
            //робимо шапку таблиці
            ws.Cell("A3").Value = "№ п/п";
            ws.Cell("B3").Value = "Особовий рахунок";
            ws.Cell("B3").Style.Alignment.WrapText = true;
            ws.Cell("C3").Value = "ПІП споживача";
            ws.Cell("D3").Value = "Адреса";
            ws.Cell("E3").Value = "Сума заборгованості";
            ws.Cell("E3").Style.Alignment.WrapText = true;
            ws.Cell("F3").Value = "Тривалість місяців виникнення";
            ws.Cell("F3").Style.Alignment.WrapText = true;
            ws.Cell("G3").Value = "Вжиті заходи до боржників";
            styler.CenterAndMergeStreamWithOneOffset(new List<string>() {
                "A3", "B3", "C3", "D3", "E3", "F3"
            }); // Потоково центруємо клітинки і з'єднуємо їх із наступними під ними( А3 з'єднує з A4 і т.д.)
            styler.CenterAndMerge("G3", "H3");
            ws.Cell("G4").Value = "попереджено";
            ws.Cell("G4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("H4").Value = "відключено";
            ws.Cell("H4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //виставляємо ширину стовпчиків
            ws.Column(1).Width = 7;
            ws.Column(2).Width = 10;
            ws.Column(3).Width = 30;
            ws.Column(4).Width = 50;
            ws.Column(5).Width = 15;
            ws.Column(6).Width = 11;
            ws.Column(7).Width = 16;
            ws.Column(8).Width = 16;

            //встановлюємо початок запису даних
            int currRow = 5;        //рядок 5
            int currCell = 1;       //стовпчик 1
            int VsogoPoper = 0;     //для підрахунку к-ті попереджень
            int VsogoVykl = 0;      //для підрахунку к-ті відключень
            foreach (Zvit1 coll in zvits)
            {
                ws.Cell(currRow, currCell).Value = coll.Id;
                ws.Cell(currRow, currCell + 1).Value = coll.OsRah;      //виводимо особовий рахунок
                ws.Cell(currRow, currCell + 2).Value = coll.FullName;   //виводимо ПІП
                ws.Cell(currRow, currCell + 3).Value = coll.FullAdres;  //виводимо адресу
                ws.Cell(currRow, currCell + 4).Value = coll.Borg;       //виводимо борг
                ws.Cell(currRow, currCell + 5).Value = coll.Month;      //виводимо к-ть місяців боргу
                ws.Cell(currRow, currCell + 6).Value = coll.DataPoper;  //виводимо коли видане попередження
                ws.Cell(currRow, currCell + 7).Value = coll.DataVykl;   //виводимо коли відключили
                styler.CenterRowCellStramRange(currRow, currCell, 7, new int[2] { 2, 3 }); // Потоком центруємо і робимо переніс тексту 
                if (!string.IsNullOrEmpty(coll.DataPoper))
                {
                    VsogoPoper = ++VsogoPoper;
                }

                if (!string.IsNullOrEmpty(coll.DataVykl))
                {
                    VsogoVykl = ++VsogoVykl;
                }

                currRow++;     //збільшеємо рядок на 1
                currCell = 1;   //переводимо на перший стовбець
            }
            ws.Cell(currRow, currCell).Value = "Всього:";
            string Rng = "E5:E" + (currRow - 1);
            ws.Cell(currRow, currCell + 4).FormulaA1 = "=SUM(" + Rng + ")";
            ws.Cell(currRow, currCell + 6).Value = VsogoPoper;
            ws.Cell(currRow, currCell + 7).Value = VsogoVykl;
            styler.SetStreamBold(currRow, currCell, new int[] { 0, 4, 6, 7 });
            styler.CenterRowCell(currRow, currCell + 4);
            styler.CenterRowCell(currRow, currCell + 6, wrap: false);
            styler.CenterRowCell(currRow, currCell + 7, wrap: false);
            styler.SetBorder("A3:H" + currRow, left: false);

            //Ви водимо підвал документу
            ws.Cell(currRow + 2, currCell + 2).Value = "Начальник ___________________ ЦОК";
            ws.Cell(currRow + 2, currCell + 4).Value = user.Cok.Nach;
        }

        public void CreateZvit2(List<Zvit1> zvits, User user, string stanom_na, DateTime per)
        {
            DateTime from = per;
            DateTime to = DateTime.Parse(stanom_na);

            ws = wb.Worksheets.Add();
            ExcelStyling styler = new ExcelStyling(ws);  // Створюємо інстанс стиліста 
            SetDefaultSettings(); // Встановлюємо базові налаштування для Сторінки
            //робимо шапку документу
            SetZvitHeader(
                "Інформація щодо відключень та виданих попереджень по населенню ТОВ \"Тернопільелектропостач\"" +
                " за " + per.ToString("Y")
            );
            styler.CenterAndMerge("A1", "I1"); // Центруємо перший параметр комірку, і з'єднуємо її з другим параметром
            ws.Cell("A2").Value = "(Універсальна послуга)";
            styler.CenterAndMerge("A2", "I2");
            //Шапка таблиці
            ws.Cell("A3").Value = "Групи споживачів";
            ws.Cell("A3").Style.Alignment.WrapText = true;
            ws.Cell("B3").Value = "Кількість боржників всього";
            ws.Cell("B3").Style.Alignment.WrapText = true;
            ws.Cell("C3").Value = "В тому числі";
            ws.Cell("C3").Style.Alignment.WrapText = true;
            styler.CenterAndMerge("C3", "D3");
            ws.Cell("E3").Value = "Сума заборгованості всього";
            ws.Cell("E3").Style.Alignment.WrapText = true;
            ws.Cell("F3").Value = "В тому числі";
            ws.Cell("F3").Style.Alignment.WrapText = true;
            styler.CenterAndMerge("F3", "G3");
            ws.Cell("H3").Value = "Кількість виданих попереджень на відключення в звітньому місяці";
            ws.Cell("H3").Style.Alignment.WrapText = true;
            ws.Cell("I3").Value = "Кількість відключень в звітньому місяці";
            ws.Cell("I3").Style.Alignment.WrapText = true;
            styler.CenterAndMergeStreamWithOneOffset(new List<string>() {
                "A3", "B3", "E3", "H3", "I3"
            });
            ws.Cell("C4").Value = "кількість боржників 1 місяць";
            ws.Cell("C4").Style.Alignment.WrapText = true;
            ws.Cell("C4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("D4").Value = "кількість боржників більше 1 місяця";
            ws.Cell("D4").Style.Alignment.WrapText = true;
            ws.Cell("D4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("F4").Value = "сума заборгованості 1 місяць";
            ws.Cell("F4").Style.Alignment.WrapText = true;
            ws.Cell("F4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("G4").Value = "сума заборгованості більше 1 місяця";
            ws.Cell("G4").Style.Alignment.WrapText = true;
            ws.Cell("G4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("A5").Value = "1";
            ws.Cell("A5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("B5").Value = "2=(3+4)";
            ws.Cell("B5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("C5").Value = "3";
            ws.Cell("C5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("D5").Value = "4";
            ws.Cell("D5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("E5").Value = "5=(6+7)";
            ws.Cell("E5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("F5").Value = "6";
            ws.Cell("F5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("G5").Value = "7";
            ws.Cell("G5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("H5").Value = "8";
            ws.Cell("H5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("I5").Value = "9";
            ws.Cell("I5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            //Виставляємо ширину стовпчиків
            ws.Column(1).Width = 10.57;
            ws.Column(2).Width = 12;
            ws.Column(3).Width = 26;
            ws.Column(4).Width = 15;
            ws.Column(5).Width = 15;
            ws.Column(6).Width = 15;
            ws.Column(7).Width = 16;
            ws.Column(8).Width = 17;
            ws.Column(9).Width = 15;

            //Заповнюємо таблицю
            ws.Cell("A6").Value = "Населення";
            ws.Cell("A6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int countAbon = 0;
            int countAbon1 = 0;
            decimal borg = 0;
            decimal borg1 = 0;
            int poper = 0;
            int vykl = 0;

            foreach (Zvit1 coll in zvits)
            {
                if (coll.Month <= 1)
                {
                    countAbon++;
                    borg = (decimal)(borg + coll.Borg);
                }
                else
                {
                    countAbon1++;
                    borg1 = (decimal)(borg1 + coll.Borg);
                }
                if (!string.IsNullOrEmpty(coll.DataVykl) && DateTime.Parse(coll.DataVykl) >= from && DateTime.Parse(coll.DataVykl) <= to)
                {
                    vykl++;
                }
            }

            poper = _context.Poperedgenias
                .Where(
                        p => (bool)p.VydanePoper &&
                        p.Poper >= from &&
                        p.Poper <= to &&
                        p.Person.Cok.Code == user.Cok.Code
            ).Count();

            ws.Cell("C6").Value = countAbon;
            ws.Cell("C6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("D6").Value = countAbon1;
            ws.Cell("D6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("B6").Value = countAbon + countAbon1;
            ws.Cell("B6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("F6").Value = borg;
            ws.Cell("F6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("G6").Value = borg1;
            ws.Cell("G6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("E6").Value = borg + borg1;
            ws.Cell("E6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("H6").Value = poper;
            ws.Cell("H6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("I6").Value = vykl;
            ws.Cell("I6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            styler.SetBorder("A3:I6", left: false);
            //Ви водимо підвал документу
            ws.Cell("C9").Value = "Начальник ___________________ ЦОК";
            ws.Cell("G9").Value = user.Cok.Nach;
        }

        //Звіт "Перелік споживачів, по яких видано вимогу на вимкнення"
        public void CreateZvit3(User user, string stanom_na, DateTime per)
        {
            DateTime from = per;
            DateTime to = DateTime.Parse(stanom_na);
            DateTime DtStanom = CreateStanomNa(stanom_na);

            ws = wb.Worksheets.Add();
            ExcelStyling styler = new ExcelStyling(ws);  // Створюємо інстанс стиліста 
            SetDefaultSettings(); // Встановлюємо базові налаштування для Сторінки

            //робимо шапку документу
            SetZvitHeader(
                "Перелік споживачів, по яких видано вимогу на вимкнення (" + user.Cok.Name + ")"
            );
            styler.CenterAndMerge("A1", "G1");
            ws.Cell("A2").Value = " за " + per.ToString("Y");
            styler.CenterAndMerge("A2", "G2");

            //Шапка таблиці
            ws.Cell("A3").Value = "№ п/п";
            ws.Cell("B3").Value = "Особовий рахунок";
            ws.Cell("B3").Style.Alignment.WrapText = true;
            ws.Cell("C3").Value = "Прізвище, ім'я, по-батькові";
            ws.Cell("C3").Style.Alignment.WrapText = true;
            ws.Cell("D3").Value = "Подано на вимкнення";
            styler.CenterAndMerge("D3", "F3");
            ws.Cell("G3").Value = "Заборгованість за спожиту е.е на " + DtStanom.ToString("dd.MM.yyyy") + " р.";
            ws.Cell("G3").Style.Alignment.WrapText = true;
            ws.Cell("D4").Value = "Дата відключення";
            ws.Cell("D4").Style.Alignment.WrapText = true;
            ws.Cell("D4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("D4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("E4").Value = "Заборгованість за спожиту е.е на дату відключення";
            ws.Cell("E4").Style.Alignment.WrapText = true;
            ws.Cell("E4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("E4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("F4").Value = "Відключено (+/-)";
            ws.Cell("F4").Style.Alignment.WrapText = true;
            ws.Cell("F4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("F4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            styler.CenterAndMergeStreamWithOneOffset(new List<string>() {
                "A3", "B3", "C3", "G3"
            });

            //виставляємо ширину стовпчиків
            ws.Column(1).Width = 7;
            ws.Column(2).Width = 16;
            ws.Column(3).Width = 60;
            ws.Column(4).Width = 12;
            ws.Column(5).Width = 20;
            ws.Column(6).Width = 16;
            ws.Column(7).Width = 20;

            //встановлюємо початок запису даних
            int currRow = 5;        //рядок 5
            int currCell = 1;       //стовпчик 1
            int i = 1;
            DateTime date = new DateTime().Date;

            List<ActualDataPerson> people = _context.ActualDatas
                .Include(ad => ad.Person)
                .ThenInclude(p => p.UpdateFinances)
                .ThenInclude(uf => uf.UpdateGroup)
                .Include(ad => ad.Finance)
                .Include(ad => ad.Poperedgenia)
                .Include(ad => ad.Lichylnyk)
                .ThenInclude(l => l.Vykls)
                .Where(ad => ad.Person.Cok.Code == user.Cok.Code)
                .ToList();

            char sign = '-';

            foreach (ActualDataPerson ad in people)
            {
                bool canHandle = true;
                if ((bool)ad.Vykl.Status && (bool)ad.Poperedgenia.VydanePoper)
                {
                    date = (DateTime)ad.Vykl.DateVykl;
                    sign = '+';
                }
                else if ((bool)ad.Poperedgenia.VydanePoper)
                {
                    date = (DateTime)ad.Poperedgenia.DateVykl;
                    sign = '-';

                }
                else
                {
                    canHandle = false;
                }
                if (canHandle)
                {
                    if (date >= from && date <= to)
                    {
                        ws.Cell(currRow, currCell).Value = i++;
                        ws.Cell(currRow, currCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(currRow, currCell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell(currRow, currCell + 1).Value = ad.Person.OsRah;      //виводимо особовий рахунок
                        ws.Cell(currRow, currCell + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(currRow, currCell + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell(currRow, currCell + 2).Value = ad.Person.FullName;   //виводимо ПІП
                        ws.Cell(currRow, currCell + 3).Value = date.Date;           //виводимо дату відключення
                        ws.Cell(currRow, currCell + 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(currRow, currCell + 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        ws.Cell(currRow, currCell + 5).Value = sign;                //виводимо +/-
                        ws.Cell(currRow, currCell + 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(currRow, currCell + 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        UpdateFinance updateBeforeDisc = ad.Person.UpdateFinances
                            .Where(uf => uf.UpdateGroup.DateUpdate <= date)
                            .OrderByDescending(uf => uf.UpdateGroup.DateUpdate)
                            .FirstOrDefault();

                        bool updateBeforeDiscExists = false;
                        if (updateBeforeDisc != null)
                        {
                            updateBeforeDiscExists = true;
                            ws.Cell(currRow, currCell + 4).Value = updateBeforeDisc.NextFinanceSum;
                        }
                        ws.Cell(currRow, currCell + 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(currRow, currCell + 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        UpdateFinance updateRepDate = ad.Person.UpdateFinances
                            .Where(uf => uf.UpdateGroup.DateUpdate >= from && uf.UpdateGroup.DateUpdate <= to)
                            .OrderByDescending(uf => uf.UpdateGroup.DateUpdate)
                            .FirstOrDefault();

                        if (updateRepDate != null)
                        {
                            if (!updateBeforeDiscExists)
                            {
                                ws.Cell(currRow, currCell + 4).Value = updateRepDate.PrevFinanceSum;
                            }
                            ws.Cell(currRow, currCell + 6).Value = updateRepDate.NextFinanceSum;
                        }
                        else
                        {
                            if (!updateBeforeDiscExists)
                            {
                                ws.Cell(currRow, currCell + 4).Value = ad.Finance.DebPoch;
                            }
                            ws.Cell(currRow, currCell + 6).Value = ad.Finance.DebPoch;
                        }
                        ws.Cell(currRow, currCell + 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(currRow, currCell + 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        currRow++;     //збільшеємо рядок на 1
                        currCell = 1;   //переводимо на перший стовбець
                    }
                }
            }

            ws.Cell(currRow, currCell).Value = "Всього:";
            styler.SetBorder("A3:G" + currRow, left: false);

            //Ви водимо підвал документу
            ws.Cell(currRow + 2, currCell + 2).Value = "Начальник ___________________ ЦОК";
            ws.Cell(currRow + 2, currCell + 4).Value = user.Cok.Nach;
        }

        //Звіт кому видали попередження
        public void CreateZvit4(User user, DateTime from, List<Zvit1> people)
        {
            ws = wb.Worksheets.Add();
            ExcelStyling styler = new ExcelStyling(ws);  // Створюємо інстанс стиліста 
            SetDefaultSettings(); // Встановлюємо базові налаштування для Сторінки

            //робимо шапку документу
            SetZvitHeader(
                "Перелік споживачів, по яких видано попередження (" + user.Cok.Name + ")"
            );
            styler.CenterAndMerge("A1", "H1");
            ws.Cell("A2").Value = " за " + from.ToString("Y");
            styler.CenterAndMerge("A2", "H2");

            //Шапка таблиці
            ws.Cell("A3").Value = "№ п/п";
            ws.Cell("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("A3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell("B3").Value = "Особовий рахунок";
            ws.Cell("B3").Style.Alignment.WrapText = true;
            ws.Cell("B3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("B3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell("C3").Value = "Прізвище, ім'я, по-батькові";
            ws.Cell("C3").Style.Alignment.WrapText = true;
            ws.Cell("C3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("C3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell("D3").Value = "Заборгованість за спожиту е.е на дату попередження";
            ws.Cell("D3").Style.Alignment.WrapText = true;
            ws.Cell("D3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("D3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell("E3").Value = "Поточна заборгованість за спожиту е.е";
            ws.Cell("E3").Style.Alignment.WrapText = true;
            ws.Cell("E3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("E3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell("F3").Value = "Видано попередження";
            ws.Cell("F3").Style.Alignment.WrapText = true;
            ws.Cell("F3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("F3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell("G3").Value = "Прогнозована дата відключення";
            ws.Cell("G3").Style.Alignment.WrapText = true;
            ws.Cell("G3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("G3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell("H3").Value = "Вимкнення ОСР";
            ws.Cell("G3").Style.Alignment.WrapText = true;
            ws.Cell("G3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("G3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            //виставляємо ширину стовпчиків
            ws.Column(1).Width = 7;
            ws.Column(2).Width = 16;
            ws.Column(3).Width = 45;
            ws.Column(4).Width = 20;
            ws.Column(5).Width = 20;
            ws.Column(6).Width = 15;
            ws.Column(7).Width = 15;
            ws.Column(8).Width = 17;

            //встановлюємо початок запису даних
            int currRow = 4;        //рядок 5
            int currCell = 1;       //стовпчик 1
            int i = 1;

            foreach (Zvit1 ad in people)
            {
                ws.Cell(currRow, currCell).Value = i++; //№ п/п
                ws.Cell(currRow, currCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currRow, currCell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currRow, currCell + 1).Value = ad.OsRah;      //виводимо особовий рахунок
                ws.Cell(currRow, currCell + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currRow, currCell + 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currRow, currCell + 2).Value = ad.FullName;   //виводимо ПІП
                ws.Cell(currRow, currCell + 3).Value = ad.DebLoad;   //виводимо завантажений борг
                ws.Cell(currRow, currCell + 4).Value = ad.Borg;   //виводимо поточний борг
                ws.Cell(currRow, currCell + 5).Value = ad.DataPoper;   //виводимо дату попередження
                ws.Cell(currRow, currCell + 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currRow, currCell + 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currRow, currCell + 6).Value = ad.DataVykl;   //виводимо дату відключення прогноз
                ws.Cell(currRow, currCell + 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currRow, currCell + 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(currRow, currCell + 7).Value = ad.DataVyklOsr;   //виводимо дату відключення ОСР
                ws.Cell(currRow, currCell + 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(currRow, currCell + 7).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                currRow++;     //збільшеємо рядок на 1
                currCell = 1;   //переводимо на перший стовбець
            }

            ws.Cell(currRow, currCell).Value = "Всього:";
            styler.SetBorder("A3:H" + currRow, left: false);
            //виводимо підсумки
            string RngLoad = "D5:D" + (currRow - 1);
            ws.Cell(currRow, currCell + 3).FormulaA1 = "=SUM(" + RngLoad + ")";
            string Rng = "E4:E" + (currRow - 1);
            ws.Cell(currRow, currCell + 4).FormulaA1 = "=SUM(" + Rng + ")";

            //Ви водимо підвал документу
            ws.Cell(currRow + 2, currCell + 2).Value = "Виконавець ___________________ ";
            ws.Cell(currRow + 2, currCell + 4).Value = user.FullName;
        }

        public void CreateZvitVykl(List<ZvitVykl> zvitVykls, User user, string stanom_na)
        {
            string NameDoc = CreateNameDoc(user);
            DateTime DtStanom = CreateStanomNa(stanom_na);
            ws = wb.Worksheets.Add();
            ExcelStyling styler = new ExcelStyling(ws);  // Створюємо інстанс стиліста 
            SetDefaultSettings(); // Встановлюємо базові налаштування для Сторінки
            //шапка документу
            SetZvitHeader(
                            //"ТОВ \"Тернопільелектропостач\" " + NameDoc +
                            "Припинення та відновлення електропостачання "
                         );
            styler.CenterAndMerge("A1", "M1"); // Центруємо перший параметр комірку, і з'єднуємо її з другим параметром
            styler.SetStreamBold(1, 1, 1);
            ws.Cell("A2").Value = "станом на " + DtStanom.ToString("dd MMMM yyyy") + " р. по " + NameDoc;
            styler.CenterAndMerge("A2", "M2");
            styler.SetStreamBold(2, 1, 1);
            //робимо шапку таблиці
            ws.Cell("A3").Value = "№ п/п";                      //1
            ws.Cell("A3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("A3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("B3").Value = "Особовий рахунок";           //2
            ws.Cell("B3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("B3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("B3").Style.Alignment.WrapText = true;
            ws.Cell("C3").Value = "ПІП споживача";              //3
            ws.Cell("C3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("C3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("D3").Value = "Адреса";                     //4
            ws.Cell("D3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("D3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("E3").Value = "Дата відключення";           //5
            ws.Cell("E3").Style.Alignment.WrapText = true;
            ws.Cell("E3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("E3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("F3").Value = "Дебет на початок";           //6
            ws.Cell("F3").Style.Alignment.WrapText = true;
            ws.Cell("F3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("F3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("G3").Value = "Кредит на початок";          //7
            ws.Cell("G3").Style.Alignment.WrapText = true;
            ws.Cell("G3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("G3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("H3").Value = "Сума відключення";           //8
            ws.Cell("H3").Style.Alignment.WrapText = true;
            ws.Cell("H3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("H3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("I3").Value = "Сума підключення";           //9
            ws.Cell("I3").Style.Alignment.WrapText = true;
            ws.Cell("I3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("I3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("J3").Value = "Оплата";                     //11
            ws.Cell("J3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("J3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("K3").Value = "Дата підключення";           //10
            ws.Cell("K3").Style.Alignment.WrapText = true;
            ws.Cell("K3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("K3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("L3").Value = "Дебет на кінець";            //12
            ws.Cell("L3").Style.Alignment.WrapText = true;
            ws.Cell("L3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("L3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("M3").Value = "Кредит на кінець";           //13
            ws.Cell("M3").Style.Alignment.WrapText = true;
            ws.Cell("M3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("M3").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Column(1).Width = 6;
            ws.Column(2).Width = 9.29;
            ws.Column(3).Width = 30;
            ws.Column(4).Width = 33.5;
            ws.Column(5).Width = 9.14;
            ws.Column(6).Width = 11;
            ws.Column(7).Width = 8;
            ws.Column(8).Width = 9.43;
            ws.Column(9).Width = 9.57;
            ws.Column(10).Width = 6.29;
            ws.Column(11).Width = 9.29;
            ws.Column(12).Width = 11;
            ws.Column(13).Width = 7.43;
            styler.SetStreamBold(3, 1, 13);
            //встановлюємо початок запису даних
            int currRow = 4;        //рядок 4
            int currCell = 1;       //стовпчик 1
            int i = 1;
            //int VsogoPoper = 0;     //для підрахунку к-ті попереджень
            //int VsogoVykl = 0;      //для підрахунку к-ті відключень
            foreach (ZvitVykl coll in zvitVykls)
            {
                ws.Cell(currRow, currCell).Value = i;
                ws.Cell(currRow, currCell + 1).Value = coll.OsRah;      //виводимо особовий рахунок
                ws.Cell(currRow, currCell + 2).Value = coll.FullName;   //виводимо ПІП
                ws.Cell(currRow, currCell + 3).Value = coll.FullAdres;  //виводимо адресу
                ws.Cell(currRow, currCell + 4).Value = coll.DataVykl;   //виводимо коли відключили
                ws.Cell(currRow, currCell + 5).Value = coll.DebPoch;    //виводимо Дт поч
                ws.Cell(currRow, currCell + 6).Value = coll.KredPoch;   //виводимо Кт поч
                ws.Cell(currRow, currCell + 7).Value = coll.Sumavykl;   // виводимо сума за відключення
                ws.Cell(currRow, currCell + 8).Value = coll.SumaVkl;    //виводимо суму за підключення
                ws.Cell(currRow, currCell + 9).Value = coll.Oplata;     //виводимо оплату
                ws.Cell(currRow, currCell + 10).Value = coll.DataVkl;   //виводимо дату підключення
                ws.Cell(currRow, currCell + 11).Value = coll.DebKin;    //Дт кінець
                ws.Cell(currRow, currCell + 12).Value = coll.KredKin;   //Кт кінець
                styler.CenterRowCellStramRange(currRow, currCell, 12, new int[2] { 2, 3 }); // Потоком центруємо і робимо переніс тексту 

                currRow++;     //збільшуємо рядок на 1
                i++;
                currCell = 1;   //переводимо на перший стовбець
            }
            ws.Cell(currRow, currCell + 1).Value = "Всього:";
            styler.CenterRowCell(currRow, currCell);
            string Rng_Deb_Poch = "F4:F" + (currRow - 1);
            ws.Cell(currRow, currCell + 5).FormulaA1 = "=SUM(" + Rng_Deb_Poch + ")";
            string Rng_Kred_Poch = "G4:G" + (currRow - 1);
            ws.Cell(currRow, currCell + 6).FormulaA1 = "=SUM(" + Rng_Kred_Poch + ")";
            string Rng_SumaVykl = "H4:H" + (currRow - 1);
            ws.Cell(currRow, currCell + 7).FormulaA1 = "=SUM(" + Rng_SumaVykl + ")";
            string Rng_SumaVkl = "I4:I" + (currRow - 1);
            ws.Cell(currRow, currCell + 8).FormulaA1 = "=SUM(" + Rng_SumaVkl + ")";
            string Rng_Oplata = "J4:J" + (currRow - 1);
            ws.Cell(currRow, currCell + 9).FormulaA1 = "=SUM(" + Rng_Oplata + ")";
            string Rng_DebKin = "L4:L" + (currRow - 1);
            ws.Cell(currRow, currCell + 11).FormulaA1 = "=SUM(" + Rng_DebKin + ")";
            string Rng_KredKin = "M4:M" + (currRow - 1);
            ws.Cell(currRow, currCell + 12).FormulaA1 = "=SUM(" + Rng_KredKin + ")";
            styler.SetStreamBold(currRow, currCell, 13);

            styler.SetBorder("A3:M" + currRow, left: false);

            //Ви водимо підвал документу
            ws.Cell(currRow + 2, currCell + 2).Value = "Начальник ЦОК _______________  " + user.Cok.Nach;
            ws.Cell(currRow + 4, currCell + 2).Value = "Бухгалтер ЦОК _______________  " + user.Cok.Buh;
        }

        public void CreateZvitReestr(List<ZvitVykl> zvitVykls, User user, string stanom_na)
        {
            string NameDoc = CreateNameDoc(user);
            DateTime DtStanom = CreateStanomNa(stanom_na);
            ws = wb.Worksheets.Add();
            ExcelStyling styler = new ExcelStyling(ws);  // Створюємо інстанс стиліста 
            SetDefaultSettings(); // Встановлюємо базові налаштування для Сторінки
            //шапка документу
            SetZvitHeader(
                            //"ТОВ \"Тернопільелектропостач\" " + NameDoc +
                            "Реєстр припинення та відновлення електропостачання "
                         );
            styler.CenterAndMerge("A1", "L1"); // Центруємо перший параметр комірку, і з'єднуємо її з другим параметром
            styler.SetStreamBold(1, 1, 1);
            ws.Cell("A2").Value = "станом на " + DtStanom.ToString("dd MMMM yyyy") + " р. по " + NameDoc;
            styler.CenterAndMerge("A2", "L2");
            styler.SetStreamBold(2, 1, 1);
            //робимо шапку таблиці
            ws.Cell("A3").Value = "№ з/п";                                          //1
            styler.CenterAndMerge("A3", "A4");

            ws.Cell("B3").Value = "Споживачі";                                      //2
            styler.CenterAndMerge("B3", "B4");
            ws.Cell("B3").Style.Alignment.WrapText = true;

            ws.Cell("C3").Value = "№ договору";                                     //3
            styler.CenterAndMerge("C3", "C4");
            ws.Cell("C3").Style.Alignment.WrapText = true;

            ws.Cell("D3").Value = "Дата відключення";                               //4
            styler.CenterAndMerge("D3", "D4");
            ws.Cell("D3").Style.Alignment.WrapText = true;

            ws.Cell("E3").Value = "Заборгованість за активну е/e, грн.";            //5
            styler.CenterAndMerge("E3", "E4");
            ws.Cell("E3").Style.Alignment.WrapText = true;

            ws.Cell("F3").Value = "Оплачено заборгованість за активну е/е, грн.";   //6
            styler.CenterAndMerge("F3", "F4");
            ws.Cell("F3").Style.Alignment.WrapText = true;

            ws.Cell("G3").Value = "Сальдо на_________";                             //
            styler.CenterAndMerge("G3", "H3");
            ws.Cell("H3").Style.Alignment.WrapText = true;

            ws.Cell("G4").Value = "Дебет";                                          //7
            ws.Cell("G4").Style.Alignment.WrapText = true;
            ws.Cell("G4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("G4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell("H4").Value = "Кредит";                                         //8
            ws.Cell("H4").Style.Alignment.WrapText = true;
            ws.Cell("H4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("H4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell("I3").Value = "";
            styler.CenterAndMerge("I3", "J3");
            ws.Cell("J3").Style.Alignment.WrapText = true;

            ws.Cell("I4").Value = "Нараховано, грн.";                               //9
            ws.Cell("I4").Style.Alignment.WrapText = true;
            ws.Cell("I4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("I4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell("J4").Value = "Оплачено, грн.";                                 //10
            ws.Cell("J4").Style.Alignment.WrapText = true;
            ws.Cell("J4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("J4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


            ws.Cell("K3").Value = "Сальдо на __________";                           //
            styler.CenterAndMerge("K3", "L3");
            ws.Cell("L3").Style.Alignment.WrapText = true;

            ws.Cell("K4").Value = "Дебет";                                          //11
            ws.Cell("K4").Style.Alignment.WrapText = true;
            ws.Cell("K4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("K4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell("L4").Value = "Кредит";                                         //12
            ws.Cell("L4").Style.Alignment.WrapText = true;
            ws.Cell("L4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("L4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Column(1).Width = 6;
            ws.Column(2).Width = 35;
            ws.Column(3).Width = 10;
            ws.Column(4).Width = 10;
            ws.Column(5).Width = 15;
            ws.Column(6).Width = 15;
            ws.Column(7).Width = 10;
            ws.Column(8).Width = 10;
            ws.Column(9).Width = 11;
            ws.Column(10).Width = 11;
            ws.Column(11).Width = 10;
            ws.Column(12).Width = 10;
            //styler.SetStreamBold(3, 1, 13);
            //встановлюємо початок запису даних
            int currRow = 5;        //рядок 5
            int currCell = 1;       //стовпчик 1
            int i = 1;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            foreach (ZvitVykl coll in zvitVykls)
            {
                ws.Cell(currRow, currCell).Value = i;                   // № з/п
                ws.Cell(currRow, currCell + 1).Value = coll.FullName;   //виводимо ПІП
                ws.Cell(currRow, currCell + 2).Value = coll.OsRah;      //виводимо особовий рахунок
                ws.Cell(currRow, currCell + 3).Value = coll.DataVykl;    //дата відключення
                ws.Cell(currRow, currCell + 4).Value = coll.BorgEE;    //виводимо заборгованість за активну е/е
                ws.Cell(currRow, currCell + 5).Value = coll.OplataEE;   //виводимо оплату за активну е/е
                ws.Cell(currRow, currCell + 6).Value = coll.DebPoch;    //виводимо Дт поч
                ws.Cell(currRow, currCell + 7).Value = coll.KredPoch;   //виводимо Кт поч
                ws.Cell(currRow, currCell + 8).Value = coll.Sumavykl + coll.SumaVkl;   // виводимо сума за відключення
                ws.Cell(currRow, currCell + 9).Value = coll.Oplata;     //виводимо оплату
                ws.Cell(currRow, currCell + 10).Value = coll.DebKin;    //Дт кінець
                ws.Cell(currRow, currCell + 11).Value = coll.KredKin;   //Кт кінець
                styler.CenterRowCellStramRange(currRow, currCell, 10, new int[8] { 3, 4, 5, 6, 7, 8, 9, 10 }); // Потоком центруємо і робимо переніс тексту 

                currRow++;     //збільшуємо рядок на 1
                i++;
                currCell = 1;   //переводимо на перший стовбець
            }
            ws.Cell(currRow, currCell + 1).Value = "Всього:";
            styler.CenterRowCell(currRow, currCell);
            string Rng_Borg_EE = "E5:E" + (currRow - 1);
            ws.Cell(currRow, currCell + 4).FormulaA1 = "=SUM(" + Rng_Borg_EE + ")";
            string Rng_Oplata_EE = "F5:F" + (currRow - 1);
            ws.Cell(currRow, currCell + 5).FormulaA1 = "=SUM(" + Rng_Oplata_EE + ")";
            string Rng_Deb_Poch = "G5:G" + (currRow - 1);
            ws.Cell(currRow, currCell + 6).FormulaA1 = "=SUM(" + Rng_Deb_Poch + ")";
            string Rng_Kred_Poch = "H5:H" + (currRow - 1);
            ws.Cell(currRow, currCell + 7).FormulaA1 = "=SUM(" + Rng_Kred_Poch + ")";
            string Rng_Narah = "I5:I" + (currRow - 1);
            ws.Cell(currRow, currCell + 8).FormulaA1 = "=SUM(" + Rng_Narah + ")";
            string Rng_Oplata = "J5:J" + (currRow - 1);
            ws.Cell(currRow, currCell + 9).FormulaA1 = "=SUM(" + Rng_Oplata + ")";
            string Rng_DebKin = "K5:K" + (currRow - 1);
            ws.Cell(currRow, currCell + 10).FormulaA1 = "=SUM(" + Rng_DebKin + ")";
            string Rng_KredKin = "L5:L" + (currRow - 1);
            ws.Cell(currRow, currCell + 11).FormulaA1 = "=SUM(" + Rng_KredKin + ")";

            styler.SetStreamBold(currRow, currCell, 13);

            styler.SetBorder("A3:L" + currRow, left: false);

            //Ви водимо підвал документу
            ws.Cell(currRow + 2, currCell + 1).Value = "Начальник ЦОК _______________  ";
            ws.Cell(currRow + 4, currCell + 1).Value = "Бухгалтер ЦОК _______________  ";
        }

        public void CreateZvitVyklAll(Dictionary<int, SelectedGrouping> Selected, User user, string stanom_na)
        {
            string NameDoc = CreateNameDoc(user);
            DateTime DtStanom = CreateStanomNa(stanom_na);
            ws = wb.Worksheets.Add();
            ExcelStyling styler = new ExcelStyling(ws);  // Створюємо інстанс стиліста 
            SetDefaultSettings(); // Встановлюємо базові налаштування для Сторінки
            //шапка документу
            SetZvitHeader(NameDoc);
            styler.CenterAndMerge("A1", "H1"); // Центруємо перший параметр комірку, і з'єднуємо її з другим параметром
            styler.SetStreamBold(1, 1, 1);
            ws.Cell("A2").Value = "Припинення та відновлення електропостачання ";
            styler.CenterAndMerge("A2", "H2");
            styler.SetStreamBold(2, 1, 1);
            ws.Cell("A3").Value = "станом на " + DtStanom.ToString("dd MMMM yyyy") + " року";
            styler.CenterAndMerge("A3", "H3");
            styler.SetStreamBold(3, 1, 1);
            //робимо шапку таблиці
            ws.Cell("A5").Value = "№ п/п";                      //1
            ws.Cell("A5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("A5").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            styler.CenterAndMerge("A5", "A6");
            ws.Cell("B5").Value = "ЦОК";                        //2
            ws.Cell("B5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("B5").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("B5").Style.Alignment.WrapText = true;
            styler.CenterAndMerge("B5", "B6");
            ws.Cell("C5").Value = "Сальдо на початок періоду";  //3
            ws.Cell("C5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("C5").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            styler.CenterAndMerge("C5", "D5");
            ws.Cell("E5").Value = "Обороти за період";          //4
            ws.Cell("E5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("E5").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            styler.CenterAndMerge("E5", "F5");
            ws.Cell("G5").Value = "Сальдо на кінець періоду";  //5
            ws.Cell("G5").Style.Alignment.WrapText = true;
            ws.Cell("G5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("G5").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            styler.CenterAndMerge("G5", "H5");
            ws.Cell("C6").Value = "ДТ";
            ws.Cell("C6").Style.Alignment.WrapText = true;
            ws.Cell("C6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("C6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("D6").Value = "КТ";
            ws.Cell("D6").Style.Alignment.WrapText = true;
            ws.Cell("D6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("D6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("E6").Value = "Нараховано";
            ws.Cell("E6").Style.Alignment.WrapText = true;
            ws.Cell("E6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("E6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("F6").Value = "Оплачено";
            ws.Cell("F6").Style.Alignment.WrapText = true;
            ws.Cell("F6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("F6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("G6").Value = "ДТ";
            ws.Cell("G6").Style.Alignment.WrapText = true;
            ws.Cell("G6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("G6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Cell("H6").Value = "КТ";
            ws.Cell("H6").Style.Alignment.WrapText = true;
            ws.Cell("H6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("H6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Column(1).Width = 6;
            ws.Column(2).Width = 30;
            ws.Column(3).Width = 15;
            ws.Column(4).Width = 15;
            ws.Column(5).Width = 15;
            ws.Column(6).Width = 15;
            ws.Column(7).Width = 15;
            ws.Column(8).Width = 15;

            //встановлюємо початок запису даних
            int currRow = 7;        //рядок 5
            int currCell = 1;       //стовпчик 1
            int i = 1;
            foreach (var coll in Selected)
            {
                ws.Cell(currRow, currCell).Value = i;
                ws.Cell(currRow, currCell + 1).Value = coll.Value.Cok;          //виводимо назву ЦОКу
                ws.Cell(currRow, currCell + 2).Value = coll.Value.DebStart;     //виводимо ДТ на поч періоду
                ws.Cell(currRow, currCell + 3).Value = coll.Value.CredStart;    //виводимо КТ на поч періоду
                ws.Cell(currRow, currCell + 4).Value = coll.Value.Narah;        //ДТ нараховано
                ws.Cell(currRow, currCell + 5).Value = coll.Value.Opl;          //КТ оплачено
                ws.Cell(currRow, currCell + 6).Value = coll.Value.DebEnd;       //ДТ на кінець періоду
                ws.Cell(currRow, currCell + 7).Value = coll.Value.CredEnd;      //КТ на кінець періоду

                currRow++;     //збільшуємо рядок на 1
                i++;
                currCell = 1;   //переводимо на перший стовбець
            }
            ws.Cell(currRow, currCell + 1).Value = "Всього:";
            styler.CenterRowCell(currRow, currCell);
            string Rng_Deb_Poch = "C7:C" + (currRow - 1);
            ws.Cell(currRow, currCell + 2).FormulaA1 = "=SUM(" + Rng_Deb_Poch + ")";    //сумуємо ДТ на поч періоду
            string Rng_Kred_Poch = "D7:D" + (currRow - 1);
            ws.Cell(currRow, currCell + 3).FormulaA1 = "=SUM(" + Rng_Kred_Poch + ")";   //сумуємо КТ на поч періоду
            string Rng_Narah = "E7:E" + (currRow - 1);
            ws.Cell(currRow, currCell + 4).FormulaA1 = "=SUM(" + Rng_Narah + ")";   //сумуємо ДТ нараховано
            string Rng_Opl = "F7:F" + (currRow - 1);
            ws.Cell(currRow, currCell + 5).FormulaA1 = "=SUM(" + Rng_Opl + ")";   //сумуємо ДТ оплачено
            string Rng_DebKin = "G7:G" + (currRow - 1);
            ws.Cell(currRow, currCell + 6).FormulaA1 = "=SUM(" + Rng_DebKin + ")";//ДТ на кінець періоду
            string Rng_KredKin = "H7:H" + (currRow - 1);
            ws.Cell(currRow, currCell + 7).FormulaA1 = "=SUM(" + Rng_KredKin + ")";//КТ на кінець періоду
            styler.SetBorder("A5:H" + currRow, left: false);        //робимо рамку таблиці

            //Ви водимо підвал документу
            ws.Cell(currRow + 2, currCell + 1).Value = "Начальник ТЕП _______________  (Мандзин І.П.)";
            ws.Cell(currRow + 4, currCell + 1).Value = "Бухгалтер ТЕП _______________  (Чокан І.А.)";
        }

        public byte[] CreateFile()
        {
            using MemoryStream stream = new MemoryStream();
            wb.SaveAs(stream);
            byte[] content = stream.ToArray();
            return content;
        }
    }
}
