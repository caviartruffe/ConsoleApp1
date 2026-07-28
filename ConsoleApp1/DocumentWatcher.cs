using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    /// <summary>
    /// アップロードフォルダ監視と一連の処理を制御
    /// </summary>
    public class DocumentWatcher
    {
        // スレッド間で安全にデータをやり取りするためのキュー（Channel）
        // 監視スレッドが書き込み、メインスレッドが読み込みます
        private static readonly Channel<string> _folderQueue = Channel.CreateUnbounded<string>();
        // 重複チェック用のスレッドセーフなセット（値はダミー）
        private static readonly ConcurrentDictionary<string, byte> _existingItems = new();
        // 監視対象のルートフォルダ
        private static readonly string _targetPath = @"./TargetDirectory";

        // 監視間隔（ミリ秒）
        private static readonly int _monitorIntervalMs = 2000;

        /// <summary>
        /// アップロードフォルダの監視
        /// </summary>
        /// <returns></returns>
        public static async Task Run()
        {
            // 事前に監視用フォルダを作成
            if (!Directory.Exists(_targetPath)) Directory.CreateDirectory(_targetPath);
            Console.WriteLine($"[メイン] フォルダ監視を開始します。対象: {_targetPath}");
            Console.WriteLine("[メイン] 新しいフォルダを作成するか、配置してみてください。\n");

            // 1. フォルダ監視処理を「別スレッド（バックグラウンド）」で常に動かし続ける
            CancellationTokenSource cts = new CancellationTokenSource();
            Task monitorTask = Task.Run(() => StartFolderMonitorAsync(_targetPath, _monitorIntervalMs, cts.Token));

            // 2. 元スレッド（メイン）ではキューを常に参照し、データが追加されたら別処理を行う
            try
            {
                // キュー（Channel）にデータが追加されるのを非同期で待ち受ける無限ループ
                // データが投入されると ReadAsync() が即座に反応して動き出します
                while (await _folderQueue.Reader.WaitToReadAsync(cts.Token))
                {
                    while (_folderQueue.Reader.TryRead(out var newFolderPath))
                    {
                        // 追加されたフォルダに対する「別の処理」を実行
                        ExecuteBusinessLogic(newFolderPath);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[メイン] 監視がキャンセルされました。");
            }

            // 監視タスクの終了を待つ
            await monitorTask;
        }

        /// <summary>
        /// 一定間隔でフォルダ内をスキャンし、新着があればキューに送る
        /// </summary>
        /// <param name="path"></param>
        /// <param name="intervalMs"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task StartFolderMonitorAsync(string path, int intervalMs, CancellationToken token)
        {
            // 既知のフォルダを記録しておくセット（重複検知用）

            //　残存フォルダを追加

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 現在のフォルダ一覧を取得
                    string[] currentFolders = Directory.GetDirectories(path);

                    foreach (var folder in currentFolders)
                    {
                        // 以前の確認時に存在しなかった新しいフォルダを発見した場合
                        if(_existingItems.TryAdd(folder, 0))
                        {
                            Console.WriteLine($"[監視スレッド] ✨ 新しいフォルダを発見: {Path.GetFileName(folder)}");
                            // キュー（Channel）にデータを追加（元スレッドへ通知される）
                            await _folderQueue.Writer.WriteAsync(folder, token);
                        }
                    }

                    // 一定間隔で待機（非ブロッキング）
                    await Task.Delay(intervalMs, token);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"[監視スレッド] ⚠️ エラーが発生しました: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// キュー取り出し時に呼び出し
        /// </summary>
        /// <param name="item"></param>
        public void RemoveProcessedItem(string item)
        {
            _existingItems.TryRemove(item, out _);
        }

        /// <summary>
        /// 【元スレッドでの処理】キューから取り出したフォルダを処理するメソッド
        /// </summary>
        private static void ExecuteBusinessLogic(string folderPath)
        {
            Console.WriteLine($"  [元スレッド ⚡ 処理開始] 📦 ターゲット: {Path.GetFileName(folderPath)}");

            // ここに独自の処理を記述します（例: 中身のファイルの解析、DB登録、別への転送など）
            // 今回はダミーで少し待機
            Thread.Sleep(1000);

            Console.WriteLine($"  [元スレッド ⚡ 処理完了] ✅ {Path.GetFileName(folderPath)} の処理が終了しました。");
        }
    }
}
