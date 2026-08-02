using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace manage
{
    public class InfoUploadFile
    {
        public enum FileTypes
        {
            OfficeFile,
            AutoCadFile,
            IcadMxFile,
            Others
        }

        public FileTypes FileType { get; set; } = FileTypes.Others;
        public bool ConvertError { get; set; } = false;
        public string Message { get; set; } = string.Empty;


        public string FileName { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public string PdfFileName { get; set; } = string.Empty;

        public InfoUploadFile()
        {
            FileType = GetFileType(FileName);
            if (FileType != FileTypes.Others)
            {
                PdfFileName += ".pdf";
            }
        }

        public static HashSet<string> OfficeExtensions
        {
            get
            {
                if (_officeExtensions == null)
                    _officeExtensions = new HashSet<string>(Settings.Default.OfficeExtentions, StringComparer.OrdinalIgnoreCase);
                return _officeExtensions;
            }
        }
        public static HashSet<string> AutoCadExtensions
        {
            get
            {
                if (_autoCadExtensions == null)
                    _autoCadExtensions = new HashSet<string>(Settings.Default.OfficeExtentions, StringComparer.OrdinalIgnoreCase);
                return _autoCadExtensions;
            }
        }
        public static HashSet<string> IcadMxExtensions
        {
            get
            {
                if (_icadMxExtensions == null)
                    _icadMxExtensions = new HashSet<string>(Settings.Default.OfficeExtentions, StringComparer.OrdinalIgnoreCase);
                return _icadMxExtensions;
            }
        }

        private static HashSet<string>? _officeExtensions = null;
        private static HashSet<string>? _autoCadExtensions = null;
        private static HashSet<string>? _icadMxExtensions = null;

        public static FileTypes GetFileType(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            FileTypes fileType;
            if (OfficeExtensions.Contains(extension))
            {
                fileType = FileTypes.OfficeFile;
            }
            else if (AutoCadExtensions.Contains(extension))
            {
                fileType = FileTypes.AutoCadFile;
            }
            else if (IcadMxExtensions.Contains(extension))
            {
                fileType = FileTypes.IcadMxFile;
            }
            else
            {
                fileType = FileTypes.Others;
            }
            return fileType;
        }

    }
}
