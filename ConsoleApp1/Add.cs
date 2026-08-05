using System;
using System.Collections.Generic;
using System.Text;

namespace manage
{
    internal class Add
    {
        private static string _uploadFolderPath = string.Empty;
        private static string _errorFolderPath = string.Empty;

        /// <summary>
        /// 指定したフォルダとその中身を安全に削除します。
        /// </summary>
        /// <param name="folderPath"></param>
        public static void DeleteDocumentFolder(string folderPath)
        {
            //　アップロードフォルダ以下でなければ何もしない
            if (!folderPath.StartsWith(_uploadFolderPath))
            {
                return;
            }
            // フォルダが存在しない場合は何もしない
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            try
            {
                // 配下の全ファイル・フォルダの属性を通常に変更
                NormalizeAttributes(folderPath);

                // フォルダ配下を中身ごと削除
                Directory.Delete(folderPath, true);
            }
            catch (IOException ex)
            {
                // ファイルが他プロセスで使用中（ロック）の場合などのエラーハンドリング
                Console.WriteLine($"削除中にIOエラーが発生しました: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                // 権限不足エラーのハンドリング
                Console.WriteLine($"アクセス権限がありません: {ex.Message}");
            }
            catch (Exception ex)
            {
            }
            return;
        }

        /// <summary>
        /// フォルダ内のすべてのファイルとフォルダの属性を Normal に変更します。
        /// </summary>
        /// <param name="folderPath"></param>
        private static void NormalizeAttributes(string folderPath)
        {
            // 配下のファイルをすべて取得して属性を解除
            string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            // 配下のフォルダをすべて取得して属性を解除
            string[] dirs = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);
            foreach (string dir in dirs)
            {
                File.SetAttributes(dir, FileAttributes.Normal);
            }
        }

        /// <summary>
        /// 指定された移動元フォルダをエラーフォルダの配下に移動します。
        /// </summary>
        /// <param name="folderPath">移動するドキュメントフォルダパス</param>
        public static void MoveToErrorFolder(string folderPath)
        {
            //　移動元フォルダがアップロードフォルダ以下でなければ何もしない
            if (!folderPath.StartsWith(_uploadFolderPath))
            {
                return;
            }
            // フォルダが存在しない場合は何もしない
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"移動元フォルダが見つかりません: {folderPath}");
            }

            // 移動先のフォルダ名を生成
            string srcFolderName = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string dstFolderPath = Path.Combine(_errorFolderPath, srcFolderName);

            // 再帰的なコピー＆削除処理を実行（ドライブまたぎ対策）
            MoveDirectoryRecursive(folderPath, dstFolderPath, true);

            // 移動したドキュメントフォルダを削除
            if (Directory.Exists(folderPath))
            {
                DeleteDocumentFolder(folderPath);
            }
        }

        /// <summary>
        /// フォルダとその中身を再帰的に移動先へ転送します。
        /// </summary>
        private static void MoveDirectoryRecursive(string srcFolderPath, string dstFolderPath, bool overwrite = true)
        {
            try
            {
                // 移動先のフォルダが存在しない場合は作成
                Directory.CreateDirectory(dstFolderPath);

                // 配下のファイルを移動
                foreach (string file in Directory.GetFiles(srcFolderPath))
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(dstFolderPath, fileName);

                    // 移動先に同名ファイルがある場合の処理
                    if (File.Exists(destFile))
                    {
                        if (overwrite)
                        {
                            // 読み取り専用属性を解除して上書き削除・移動できるようにする
                            File.SetAttributes(destFile, FileAttributes.Normal);
                            File.Delete(destFile);
                        }
                        else
                        {
                            // 上書きしない場合はスキップ（または例外を投げる）
                            continue;
                        }
                    }

                    // ファイルの属性を通常に戻してから移動（読み取り専用対策）
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Move(file, destFile);
                }

                // 配下のフォルダを再帰的に処理
                foreach (string dir in Directory.GetDirectories(srcFolderPath))
                {
                    string dirName = Path.GetFileName(dir);
                    string destDir = Path.Combine(dstFolderPath, dirName);

                    MoveDirectoryRecursive(dir, destDir, overwrite);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
