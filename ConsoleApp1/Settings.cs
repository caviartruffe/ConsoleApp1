using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace manage
{
    public class Settings
    {
        public static Settings Default { get; private set; } = new();
        public string RegFolder { get; internal set; } = $"C:\\tools";

        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUser{ get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string MessageTextNormal { get; set; } = string.Empty;
        public string MessageTextError { get; set; } = string.Empty;
        public List<string> OfficeExtentions { get; set; } = new List<string>();
        public List<string> AutoCadExtentions { get; set; } = new List<string>();
        public List<string> IcadNxExtentions { get; set; } = new List<string>();
    }
}
