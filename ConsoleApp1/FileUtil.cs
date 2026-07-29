using ConsoleApp1;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace manage
{
    public class FileUtil
    {
        // log4net
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        public static List<string> ScanRegFolder()
        {
            _logger.Info("test2");
            var regFolders = new List<string>();
			try
			{
				var folders = Directory.GetDirectories(Settings.Default.RegFolder);
                foreach (var folder in folders)
                {
                    var num = GetRegNumber(folder);
                    if (!string.IsNullOrEmpty(num))
                    {
                        continue;
                    }
                }
            }
			catch (Exception)
			{
                throw;
            }
            return regFolders;
        }

        public static string GetRegNumber(string path)
        {
            // チェックポイントファイルのないフォルダは対象外
            if (File.Exists(Path.Combine(path, ".done")))
            {
                return Path.GetFileName(Path.GetFileName(path));
            }
            return string.Empty;
        }

        public static string GetRegFolderPath(string regNumber)
        {
            return Path.Combine(Settings.Default.RegFolder, regNumber);
        }

        public enum FolderState
        {
            Uploaded,
            Converted,
            Sent,
            None

        }

        /// <summary>
        /// フォルダ処理進行状態を取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FolderState GetFolderState(string path)
        {
            if (File.Exists(Path.Combine(path, ".send.done")))
            {
                return FolderState.Sent;
            }
            if (File.Exists(Path.Combine(path, ".convert.done")))
            {
                return FolderState.Converted;
            }
            if (File.Exists(Path.Combine(path, ".upload.done")))
            {
                return FolderState.Uploaded;
            }
            // チェックポイントファイルのないフォルダは対象外
            return FolderState.None;
        }

        public enum FileType
        {
            /// <summary>
            /// Microsoft Officeファイル
            /// </summary>
            OfficeFile,
            /// <summary>
            /// DWG/DXFファイル
            /// </summary>
            DxfFile,
            /// <summary>
            /// iCAD/MXファイル
            /// </summary>
            MxFile,
            /// <summary>
            /// その他
            /// </summary>
            Others
        }


        private static FileType GetFileType(string filePath)
        {
            // 2. 拡張子とファイルタイプのマッピングを定義
            // HashSet（ハッシュセット）を使うことで、拡張子の検索を高速に行えます
            // ※　設定ファイルクラスに
            var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif" };
            var documentExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".pdf", ".docx", ".xlsx", ".txt" };
            var audioExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".mp3", ".wav", ".flac", ".aac" };

            List<string> _list = new List<string>();
            HashSet<string> _test = new HashSet<string>(_list, StringComparer.OrdinalIgnoreCase);

            // 3. ファイルパスから拡張子を取得（例: ".docx"）
            // Path.GetExtension は大文字小文字を維持するため、判定時は注意が必要です
            string extension = Path.GetExtension(filePath);

            // 4. 拡張子のリストを元にファイルタイプを識別
            FileType fileType;
            if (imageExtensions.Contains(extension))
            {
                fileType = FileType.OfficeFile;
            }
            else if (documentExtensions.Contains(extension))
            {
                fileType = FileType.DxfFile;
            }
            else if (audioExtensions.Contains(extension))
            {
                fileType = FileType.MxFile;
            }
            else
            {
                fileType = FileType.Others;
            }
            return fileType;
        }
    }
}
