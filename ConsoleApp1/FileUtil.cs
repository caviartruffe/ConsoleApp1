using ConsoleApp1;
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
        public static List<string> ScanRegFolder()
        {
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
    }
}
