using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    internal class ConverterManager
    {
        enum FileType { Text, Image, Audio }
        record FileItem(FileType Type, string Name, int ProcessTimeMs);
        static async Task Run()
        {
            // テストデータ：Text1が重い処理（3秒）をしている間に、
            // 後続のImageやAudioが「追い越して」どんどん処理されるかテストします。
            var files = new List<FileItem>
        {
            new(FileType.Text,  "Text_1",  3000), // ★3秒かかる重い処理
            new(FileType.Image, "Image_1", 1000), // 1秒
            new(FileType.Text,  "Text_2",  1000), // ★Text_1が終わるまで待機
            new(FileType.Audio, "Audio_1", 1000), // 1秒
            new(FileType.Image, "Image_2", 1000)  // Image_1が終わるまで待機
        };

            var workerChannels = new Dictionary<FileType, Channel<FileItem>>();
            var workerTasks = new List<Task>();

            //　対象のファイルを繰り返し、拡張子からタイプわけしてタスク追加
            // 各ファイル種ごとに「独立した処理ライン」を立ち上げる
            foreach (FileType type in Enum.GetValues<FileType>())
            {
                var channel = Channel.CreateUnbounded<FileItem>();
                workerChannels[type] = channel;
                workerTasks.Add(ProcessFileQueueAsync(type, channel.Reader));
            }

            // キューへ投入
            Console.WriteLine("[システム] キューへの一斉登録を開始します。");
            foreach (var file in files)
            {
                await workerChannels[file.Type].Writer.WriteAsync(file);
            }

            // 投入完了を通知
            foreach (var channel in workerChannels.Values) channel.Writer.Complete();

            // すべての処理が終わるまで待機
            await Task.WhenAll(workerTasks);
            Console.WriteLine("[システム] すべての処理が終了しました。");
        }

        static async Task ProcessFileQueueAsync(FileType type, ChannelReader<FileItem> reader)
        {
            // 割り当てられた種類専用のループ
            await foreach (var file in reader.ReadAllAsync())
            {
                // ここで await するため、同じ種類（例: Text_2）は「Text_1」が終わるまでここでピタッと止まります
                Console.WriteLine($"▶️ [開始] {file.Name} ({file.Type}) の処理を始めました。");

                // 実際の処理（ファイルごとに設定された時間を待機）
                await Task.Delay(file.ProcessTimeMs);

                Console.WriteLine($"  [完了] ❌ {file.Name} ({file.Type}) が終わりました。");
            }
        }

    }
}
