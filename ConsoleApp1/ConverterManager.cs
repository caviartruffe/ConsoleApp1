using manage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace manage
{
    public class ConverterManager
    {
        public static async Task<bool> RunAsync(InfoDocument docInfo)
        {
            // 変換タイプと対応するキュー
            var convertChannels = new Dictionary<InfoUploadFile.FileTypes, Channel<InfoUploadFile>>();
            var convertTasks = new List<Task>();

            // ファイルタイプ別に変換用キューを生成
            InfoUploadFile.FileTypes[] convertFileTypes =
            {
                InfoUploadFile.FileTypes.OfficeFile,
                InfoUploadFile.FileTypes.AutoCadFile,
                InfoUploadFile.FileTypes.IcadMxFile,
            };
            foreach (var type in convertFileTypes)
            {
                var channel = Channel.CreateUnbounded<InfoUploadFile>();
                convertChannels[type] = channel;
                convertTasks.Add(ConvertFileQueueAsync(type, channel.Reader));
            }

            // 変換用キューへファイル情報を登録
            foreach (var fileInfo in docInfo.UploadFileInfos)
            {
                // 変換対象外ファイルはスキップ
                if (fileInfo.FileType == InfoUploadFile.FileTypes.Others)
                    continue;
                await convertChannels[fileInfo.FileType].Writer.WriteAsync(fileInfo);
            }

            // すべてのキューの登録完了を通知
            foreach (var channel in convertChannels.Values)
                channel.Writer.Complete();

            // すべての処理が終わるまで待機
            await Task.WhenAll(convertTasks);

            // すべてのドキュメントが変換できたか確認
            foreach (var fileInfo in docInfo.UploadFileInfos)
            {
                // いずれかのファイルで変換エラーが発生した場合falseを返す
                if (fileInfo.ConvertError)
                    return false;
            }

            return true;
        }

        static async Task ConvertFileQueueAsync(InfoUploadFile.FileTypes type, ChannelReader<InfoUploadFile> reader)
        {
            // 割り当てられた種類専用のループ
            await foreach (var file in reader.ReadAllAsync())
            {
                // 1. 起動パラメータの設定 (.NET仕様の初期化子)
                var startInfo = new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = "",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true 
                };

                // 2. タイムアウト用のCancellationTokenを生成 (10秒)
                // .NET 9の環境でスレッドをブロックしない「非同期タイムアウト」を実現します
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(100));

                // 3. usingのスコープ変数を活用してリソースを自動解放
                try
                {
                    Console.WriteLine("外部プログラムを起動します...");
                    using var process = Process.Start(startInfo);

                    if (process is null)
                    {
                        throw new InvalidOperationException("プロセスの起動に失敗しました。");
                    }

                    Console.WriteLine($"起動成功（PID: {process.Id}）。終了を待機します...");

                    try
                    {
                        // 非同期で終了待機
                        await process.WaitForExitAsync(cts.Token);

                        Console.WriteLine($"プログラムが正常に終了しました。終了コード: {process.ExitCode}");
                    }
                    catch (OperationCanceledException)
                    {
                        // 4. タイムアウト時の強制終了処理
                        Console.WriteLine("タイムアウト時間を超過したため、強制終了します。");

                        if (!process.HasExited)
                        {
                            // 子プロセスまで丸ごと強制終了
                            process.Kill(entireProcessTree: true);
                            // 強制終了が完了するまで非同期で待機
                            await process.WaitForExitAsync(); 
                            Console.WriteLine("プロセスを完全に強制終了しました。");
                        }
                    }
                }
                // 5. 例外処理
                catch (Win32Exception ex)
                {
                    // ファイルが存在しない(NativeErrorCode: 2) や、アクセス権がない(5) などのOSエラー
                    Console.Error.WriteLine($"[起動エラー] {ex.Message} (OSコード: {ex.NativeErrorCode})");
                }
                catch (InvalidOperationException ex)
                {
                    // 不適切なプロセスの操作時に発生
                    Console.Error.WriteLine($"[操作エラー] {ex.Message}");
                }
                catch (Exception ex)
                {
                    // その他の予期せぬエラー
                    Console.Error.WriteLine($"[システムエラー] {ex.Message}");
                }

                // ※ `using var process` により、メソッドを抜けるか例外発生時に自動で Dispose()             }
            }
        }
    }
}
