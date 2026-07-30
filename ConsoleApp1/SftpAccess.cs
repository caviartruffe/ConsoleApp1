using ConsoleApp1;
using Microsoft.VisualBasic.FileIO;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace manage
{
    public class SftpAccess
    {
        public bool CreateControlFile(DocumentInfo info)
        {
            return false;
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

        public void func()
        {
            var host = "your-sftp-server.com";
            var username = "your_username";
            var password = "your_password";

            // 1. ローカルの対象フォルダと、リモートの保存先を指定
            string localFolder = @"C:\local\upload_files";
            string remoteBaseDir = "/remote/uploads/daily_report";

            // ローカルにフォルダがあるか確認
            if (!Directory.Exists(localFolder))
            {
                Console.WriteLine("指定されたローカルフォルダが存在しません。");
                return;
            }

            // アップロード対象のファイル一覧を取得
            string[] localFiles = Directory.GetFiles(localFolder);

            using (var client = new SftpClient(host, username, password))
            {
                try
                {
                    client.Connect();
                    Console.WriteLine("SFTPサーバに接続しました。");

                    // 【機能1】アップロード先フォルダの自動作成
                    // 階層が深い場合（例: /a/b/c）を考慮して作成する関数を呼び出す
                    CreateRemoteDirectoryIfNeeded(client, remoteBaseDir);

                    // 【機能2】複数ファイルをループ処理で一括アップロード
                    foreach (var localPath in localFiles)
                    {
                        string fileName = Path.GetFileName(localPath);
                        // リモート側のフルパスを生成 Linuxの区切り文字「/」にする
                        string remotePath = $"{remoteBaseDir.TrimEnd('/')}/{fileName}";

                        Console.WriteLine($"\n[開始] {fileName} をアップロード中...");

                        using (var localStream = File.OpenRead(localPath))
                        {
                            // アップロード実行（上書き許可）
                            client.UploadFile(localStream, remotePath, canOverride: true);
                        }

                        // 【機能3】アップロードが成功したかをExistsで確認
                        if (client.Exists(remotePath))
                        {
                            // 確認完了としてファイルサイズを取得し、整合性をチェックするとより安全です
                            var sftpFile = client.Get(remotePath);
                            Console.WriteLine($"[成功] {fileName} のアップロードを確認しました。(サイズ: {sftpFile.Attributes.Size} バイト)");
                        }
                        else
                        {
                            Console.WriteLine($"[失敗] {fileName} はサーバ上に確認できませんでした。");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"システムエラー: {ex.Message}");
                }
                finally
                {
                    client.Disconnect();
                    Console.WriteLine("\nSFTP接続を切断しました。");
                }
            }
        }
        /// <summary>
        /// リモートフォルダが存在しない場合、階層を遡って自動作成する補助メソッド
        /// </summary>
        static void CreateRemoteDirectoryIfNeeded(SftpClient client, string remoteDirPath)
        {
            // すでに存在すれば何もしない
            if (client.Exists(remoteDirPath)) return;

            // 親ディレクトリのパスを取得して再帰的に作成する処理
            string parentDir = Path.GetDirectoryName(remoteDirPath)?.Replace("\\", "/");

            if (!string.IsNullOrEmpty(parentDir) && parentDir != "/")
            {
                CreateRemoteDirectoryIfNeeded(client, parentDir);
            }

            // フォルダを作成
            client.CreateDirectory(remoteDirPath);
            Console.WriteLine($"リモートフォルダを作成しました: {remoteDirPath}");
        }

        public enum SftpFunction
        {
            /// <summary>
            /// 
            /// </summary>
            Numbering = 0,
            /// <summary>
            /// 
            /// </summary>
            Relation = 1,
            /// <summary>
            /// 
            /// </summary>
            Registration = 2
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

        public static string GetSftpCOntrolFileBasename(SftpFunction func, int no)
        {
            var basename = $"XXXX_B-{func}_{no}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}";
            return basename;
        }

        const string query = "SELECT id, name, email " +
                     "FROM users " +
                     "WHERE status = 'active' " +
                     "ORDER BY created_at DESC;";
        public static void GetTsvValue()
        {
            string filePath = "sample.tsv";
            if (!File.Exists(filePath))
            {
                // error
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

        public class ApiResponse
        {
            public string ReturnCode { get; set; } = string.Empty;
            public ResultContent Results { get; set; } = new();
        }

        public class ResultContent
        {
            public string Error { get; set; } = string.Empty;
        }

        public static void ResultJson()
        {
            string jsonString = @"{
  ""ReturnCode"": ""00"",
  ""RESULTS"": { ""ERROR"": """" }
}";

            // 大文字小文字を区別しないオプションを設定
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // デシリアライズ
            ApiResponse? response = JsonSerializer.Deserialize<ApiResponse>(jsonString, options);

            // 結果の確認
            Console.WriteLine($"ReturnCode: {response?.ReturnCode}");       // 出力: 00
            Console.WriteLine($"Error: {response?.Results?.Error}");
        }
    }
}
