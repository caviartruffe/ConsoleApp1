using Microsoft.VisualBasic.FileIO;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace manage
{
    public class SftpAccess
    {
        public enum SftpFunction
        {
            Numbering = 4002,
            Relation = 4004,
            Registration = 4005
        }

        public static string GetSftpBaseName(SftpFunction func, int no)
        {
            var basename = $"XXXX_B-{func}_{no}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
            return basename;
        }

        public SftpClient GetSftpClient()
        {
            // --- 設定情報 ---
            string host = "://server.com";
            string username = "your_username";
            string password = "your_password";

            var connectionInfo = new ConnectionInfo(host, username, new PasswordAuthenticationMethod(username, password));
            return new SftpClient(connectionInfo);
        }

        public bool Connect()
        {
            var keyFile = new PrivateKeyFile(@"C:\path\to\id_rsa", "passphrase_if_needed");
            var connectionInfo = new ConnectionInfo("://example.com", 22, "username",
                new PrivateKeyAuthenticationMethod("username", keyFile));

            //using (var client = new SftpClient("://example.com", 22, "username", "password"))
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                if (client.IsConnected)
                {
                    client.Disconnect();
                }
            }

            return true;
        }

        public void DeleteRemoteFilesInFolder(string remoteFolderPath)
        {
            using (var client = GetSftpClient())
            {
                try
                {
                    Console.WriteLine("SFTPサーバーに接続中...");
                    client.Connect();

                    // 指定フォルダが存在するか確認
                    if (!client.Exists(remoteFolderPath))
                    {
                        Console.WriteLine($"エラー: 指定されたフォルダが存在しません: {remoteFolderPath}");
                        return;
                    }

                    Console.WriteLine($"{remoteFolderPath} 内のファイルを検索中...");

                    // フォルダ内の項目をすべて取得し個々にファイルを削除
                    var files = client.ListDirectory(remoteFolderPath);
                    foreach (var file in files)
                    {
                        // ディレクトリは対象外
                        if (file.IsDirectory)
                            continue;

                        // ファイルの削除を実行
                        Console.WriteLine($"削除中: {file.Name} ...");
                        client.DeleteFile(file.FullName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"エラーが発生しました: {ex.Message}");
                }
                finally
                {
                    if (client.IsConnected)
                    {
                        client.Disconnect();
                        Console.WriteLine("切断しました。");
                    }
                }
            }
        }

        public void DeleteRemoteFiles(List<string> files)
        {
            if (files == null || files.Count == 0)
            {
                Console.WriteLine("削除対象のファイルが指定されていません。");
                return;
            }

            // 3. クライアントの生成と自動切断（using）
            using (var client = GetSftpClient())
            {
                try
                {
                    Console.WriteLine("SFTPサーバーに接続中...");
                    client.Connect();

                    // 4. ファイルのループ処理
                    foreach (string remoteFilePath in files)
                    {
                        // フォルダ名とファイル名を安全に結合
                        //string remoteFilePath = Path.Combine(remoteFolderPath, fileName).Replace("\\", "/");

                        try
                        {
                            // ファイルの存在確認をしてから削除
                            if (client.Exists(remoteFilePath))
                            {
                                client.DeleteFile(remoteFilePath);
                            }
                            else
                            {
                                Console.WriteLine($"[スキップ] ファイルが存在しません: {remoteFilePath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            // 1つのファイルでエラーが起きても、次のファイルの処理を続ける
                            Console.WriteLine($"[削除失敗] {remoteFilePath} の処理中にエラー: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 接続自体に失敗した場合などのエラーハンドリング
                    Console.WriteLine($"SFTP通信全体でエラーが発生しました: {ex.Message}");
                }
                finally
                {
                    if (client.IsConnected)
                    {
                        client.Disconnect();    // 明示的に切断
                        Console.WriteLine("SFTPサーバーから切断しました。");
                    }
                }
            }
        }
        public void DownloadRemoteFiles(List<string> files, string localFolderPath)
        {
            // 1. 引数の事前チェック
            if (files == null || files.Count == 0)
            {
                Console.WriteLine("ダウンロード対象のファイルが指定されていません。");
                return;
            }

            // 2. 保存先ローカルフォルダがなければ自動作成
            if (!Directory.Exists(localFolderPath))
            {
                // なければエラー
                return;
            }

            using (var client = GetSftpClient())
            {
                try
                {
                    Console.WriteLine("SFTPサーバーに接続中...");
                    client.Connect();

                    // 4. ファイルのループ処理
                    foreach (string remoteFilePath in files)
                    {
                        // リモート側とローカル側のフルパスをそれぞれ安全に結合
                        var fileName = Path.GetFileName(remoteFilePath);
                        string localFilePath = Path.Combine(localFolderPath, fileName);

                        try
                        {
                            // サーバー側にファイルが存在するか確認
                            if (client.Exists(remoteFilePath))
                            {
                                Console.WriteLine($"ダウンロード中: {fileName} ...");

                                // ローカルにファイルを生成してダウンロード（既存ファイルがある場合は上書き）
                                using (var fileStream = File.Create(localFilePath))
                                {
                                    client.DownloadFile(remoteFilePath, fileStream);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[エラー] サーバー上にファイルが存在しません: {fileName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            // 1つのファイルでエラーが起きても、次のファイルの処理を続ける
                            Console.WriteLine($"[失敗] {fileName} のダウンロード中にエラー: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SFTP通信全体でエラーが発生しました: {ex.Message}");
                }
                finally
                {
                    if (client.IsConnected)
                    {
                        client.Disconnect(); // 明示的に切断
                        Console.WriteLine("SFTPサーバーから切断しました。");
                    }
                }
            }
        }
        public bool ExistsRemoteFile(string remoteFilePath)
        {
            using (var client = GetSftpClient())
            {
                try
                {
                    Console.WriteLine("SFTPサーバーに接続中...");
                    client.Connect();

                    // ファイルの存在チェック
                    if (client.Exists(remoteFilePath))
                    {
                        Console.WriteLine($"【存在します】: {remoteFilePath}");

                        // (応用) 存在する場合だけ、ファイル情報を取得する例
                        var fileAttributes = client.GetAttributes(remoteFilePath);
                        Console.WriteLine($"サイズ: {fileAttributes.Size} バイト");
                        Console.WriteLine($"最終更新日時: {fileAttributes.LastWriteTime}");
                    }
                    else
                    {
                        Console.WriteLine($"【存在しません】: {remoteFilePath}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"エラーが発生しました: {ex.Message}");
                }
                finally
                {
                    if (client.IsConnected)
                    {
                        client.Disconnect();
                        Console.WriteLine("切断しました。");
                    }
                }

                return true;
            }
        }

        public void UploadLocalFiles(List<string> files, string remoteFolderPath)
        {
            if (files.Count == 0)
            {
                Console.WriteLine("アップロードするファイルがありません。");
                return;
            }

            // 接続情報の作成

            using (var client = GetSftpClient())
            {
                try
                {
                    Console.WriteLine("SFTPサーバーに接続中...");
                    client.Connect();

                    // リモートの指定フォルダが存在しない場合は作成
                    if (!client.Exists(remoteFolderPath))
                    {
                        // エラー
                        Console.WriteLine($"リモートフォルダを作成します: {remoteFolderPath}");
                        return; 
                    }

                    foreach (string localFilePath in files)
                    {
                        // リモート側のフルパスを生成
                        string fileName = Path.GetFileName(localFilePath);
                        string remoteFilePath = Path.Combine(remoteFolderPath, fileName).Replace("\\", "/");

                        Console.WriteLine($"アップロード中: {fileName} ...");

                        // ファイルを読み込んでアップロード実行
                        using (var fileStream = File.OpenRead(localFilePath))
                        {
                            client.UploadFile(fileStream, remoteFilePath, true); // trueで同名ファイルは上書き
                        }
                    }

                    Console.WriteLine("すべてのファイルのアップロードが完了しました。");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"エラーが発生しました: {ex.Message}");
                }
                finally
                {
                    if (client.IsConnected)
                    {
                        client.Disconnect();
                        Console.WriteLine("切断しました。");
                    }
                }
            }
        }

        public static void CreateSftpControlFile()
        {
            var lines = new List<string>
        {
            string.Join("\t", new[] { "ID", "名前", "年齢" }),
            string.Join("\t", new[] { "1", "Taro", "25" }),
            string.Join("\t", new[] { "2", "Hanako", "30" })
        };

            // ファイルパスを指定して出力（UTF-8）
            string filePath = "output.tsv";
            File.WriteAllLines(filePath, lines);
        }


        // これは使用しない
        public static void GetTsvValue(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            using (var parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters("\t");
                parser.HasFieldsEnclosedInQuotes = true; // ダブルクォートの囲みを有効にする

                // 1. ヘッダ行を空読み
                if (!parser.EndOfData)
                {
                    parser.ReadFields();
                }
                else
                {
                    //error
                }

                // 3. データ行目を読み込み
                if (!parser.EndOfData)
                {
                    string[]? fields = parser.ReadFields();

                    if (fields != null)
                    {
                        // 5カラム目（インデックス4）が存在するか確認
                        if (fields != null && fields.Length >= 5)
                        {
                            string value = fields[4]; // ダブルクォートは自動除去されます
                            Console.WriteLine($"取得した値: {value}");
                        }
                    }
                    else
                    {
                        //error
                    }
                }
                else
                {
                    // error
                }
            }
        }

        public class SftpResult
        {
            public string ReturnCode { get; set; } = string.Empty;
            public SftpResultContent Results { get; set; } = new();
        }

        public class SftpResultContent
        {
            public string Error { get; set; } = string.Empty;
        }

        public static bool IsSftpResultSuccess(string filePath)
        {
            // ファイルが存在しない場合のチェック
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"エラー: ファイルが見つかりません ({filePath})");
                return false;
            }

            // 大文字小文字を区別しないオプションを設定
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                // デシリアライズ
                string jsonString = File.ReadAllText(filePath);
                SftpResult? response = JsonSerializer.Deserialize<SftpResult>(jsonString, options);
                if (response == null)
                    return false;

                if (response.ReturnCode == "00")
                    return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ファイル読み込みまたは解析中にエラーが発生しました: {ex.Message}");
            }
            return false;
        }
    }
}
