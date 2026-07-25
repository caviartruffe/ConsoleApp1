using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Settings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUser{ get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string MessageTextNormal { get; set; } = string.Empty;
        public string MessageTextError { get; set; } = string.Empty;
        public static Settings Default { get; private set; } = new();
    }
}
