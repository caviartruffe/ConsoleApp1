using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class DocumentInfo
    {
        // ドキュメントID
        public string Id { get; set; } = string.Empty;

        public List<FileInfo> Files { get; set; } = new List<FileInfo>();
    }

    public class FileInfo
    {
        public string FileName { get; set; } = string.Empty;

        public string FileSize { get; set; } = string.Empty;
    }
}
