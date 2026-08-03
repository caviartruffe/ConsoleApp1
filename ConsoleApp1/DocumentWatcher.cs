using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace manage
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

        private static readonly ConcurrentDictionary<int, InfoDocument> _sftpPool = new ConcurrentDictionary<int, InfoDocument>();

        // 監視間隔（ミリ秒）
        private static readonly int _monitorIntervalMs = 2000;
        private static readonly int _monitorSftpIntervalMs = 2000;

        /// <summary>
        /// アップロードフォルダの監視
        /// </summary>
        /// <returns></returns>
        public static async Task RunAsync()
        {
            // 事前に監視用フォルダを作成
            if (!Directory.Exists(_targetPath)) Directory.CreateDirectory(_targetPath);
            Console.WriteLine($"[メイン] フォルダ監視を開始します。対象: {_targetPath}");
            Console.WriteLine("[メイン] 新しいフォルダを作成するか、配置してみてください。\n");

            // 1. フォルダ監視処理を「別スレッド（バックグラウンド）」で常に動かし続ける
            CancellationTokenSource cts = new CancellationTokenSource();
            Task monitorTask = Task.Run(() => StartFolderMonitorAsync(_targetPath, _monitorIntervalMs, cts.Token));

            // SFTPサーバの実行結果監視処理を別スレッドで常に動かし続ける
            CancellationTokenSource ctsMonitorSftp = new CancellationTokenSource();
            Task monitorSftpTask = Task.Run(() => MoniteringSftpHostAsync(_targetPath, _monitorSftpIntervalMs, ctsMonitorSftp.Token));
            

            // 2. 元スレッド（メイン）ではキューを常に参照し、データが追加されたら別処理を行う
            try
            {
                // キュー（Channel）にデータが追加されるのを非同期で待ち受ける無限ループ
                // データが投入されると ReadAsync() が即座に反応して動き出します
                while (await _folderQueue.Reader.WaitToReadAsync(cts.Token))
                {
                    while (_folderQueue.Reader.TryRead(out var newFolderPath))
                    {
                        RemoveProcessedItem(newFolderPath);

                        await DocumentProcessingAsyc(newFolderPath);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[メイン] 監視がキャンセルされました。");
            }

            // 監視タスクの終了を待つ
            await monitorTask;
            await monitorSftpTask;
        }

        /// <summary>
        /// キューから取り出したフォルダを処理するメソッド
        /// </summary>
        private static async Task DocumentProcessingAsyc(string folderPath)
        {
            // 今回はダミーで少し待機
            //Thread.Sleep(1000);
            try
            {
                // DBと照合してInfoDOcumuentを生成
                var docInfo = new InfoDocument();
                if (docInfo == null)
                {
                    // エラーならここでおわり
                    return;
                }
                // ConvertManagerにPDF変換を依頼
                var convertState = await ConverterManager.RunAsync(docInfo);
                if (!convertState)
                {
                    // エラーならここでおわり
                    return;
                }

                // SftpManagerにSFTP連携を依頼
                if (!_sftpPool.TryAdd(docInfo.DocRegId, docInfo))
                {
                    // falseは基本的に起きない
                }

                var sftpState = await SftpManager.RunAsync(_sftpPool);
                if (!convertState)
                {
                    // エラーならここでおわり
                    return;
                }
                // UploadFolder内の対象を削除

                // 成功メール送信
                // 送信するか確認
            }
            catch (Exception ex)
            {
                // ログ

            }
        }

        /// <summary>
        /// 一定間隔でSFTPサーバーを監視
        /// </summary>
        /// <param name="path"></param>
        /// <param name="intervalMs"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task MoniteringSftpHostAsync(string path, int intervalMs, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 現在のフォルダ一覧を取得
                    string[] currentFolders = Directory.GetDirectories(path);

                    foreach (var folder in currentFolders)
                    {
                        // 以前の確認時に存在しなかった新しいフォルダを発見した場合
                        if (_existingItems.TryAdd(folder, 0))
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
        /// 一定間隔でフォルダ内をスキャンし、新着があればキューに送る
        /// </summary>
        /// <param name="path"></param>
        /// <param name="intervalMs"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task StartFolderMonitorAsync(string path, int intervalMs, CancellationToken token)
        {
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
        public static void RemoveProcessedItem(string item)
        {
            _existingItems.TryRemove(item, out _);
        }


    }
}
