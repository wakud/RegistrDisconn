using RegistrDisconnection.Models.Operations;
using System.Collections.Generic;

namespace RegistrDisconnection.ViewModels
{
    public class PrintPoperPostal
    {
        public List<PrintPoper> PrintPopers { get; set; }
        public string SumText { get; set; }
        public decimal SumDecimal { get; set; }
        public string Postal { get; set; }
        public string Price { set; get; }
        public string Nach { get; set; }
        public string Buh { get; set; }
    }
}
