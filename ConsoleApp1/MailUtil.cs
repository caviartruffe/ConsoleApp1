using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Renci.SshNet.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class MailUtil
    {
        public static async Task SendNormal(DocumentInfo info)
        {
            var message = ComposeMessage(info, Settings.Default.MessageTextNormal);
            await SendMessage(message);
        }

        public static async Task SendError(DocumentInfo info)
        {
            var message = ComposeMessage(info, Settings.Default.MessageTextError);
            await SendMessage(message);
        }

        public static async Task SendMessage(MimeMessage message)
        {
            // 2. SMTPクライアントを使用した送信処理
            using (var client = new SmtpClient())
            {
                try
                {
                    // SMTPサーバーへの接続
                    // 引数: (ホスト名, ポート番号, セキュリティ設定)
                    await client.ConnectAsync(Settings.Default.SmtpServer, Settings.Default.SmtpPort, SecureSocketOptions.Auto);

                    // アカウント認証（必要な場合）
                    if (!string.IsNullOrEmpty(Settings.Default.SmtpUser))
                        await client.AuthenticateAsync(Settings.Default.SmtpUser, Settings.Default.SmtpPassword);

                    // メールの送信
                    await client.SendAsync(message);
                    Console.WriteLine("メールの送信に成功しました。");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"エラーが発生しました: {ex.Message}");
                }
                finally
                {
                    // サーバーから安全に切断
                    await client.DisconnectAsync(true);
                }
            }
        }

        private static MimeMessage ComposeMessage(DocumentInfo iinfo, string template)
        {
            // 1. メールメッセージの作成
            var message = new MimeMessage();

            // 送信元（名前, メールアドレス）
            message.From.Add(new MailboxAddress("送信者名", "sender@example.com"));
            // 宛先（名前, メールアドレス）
            message.To.Add(new MailboxAddress("受信者名", "receiver@example.com"));

            // 件名
            message.Subject = "MailKitからのテストメール";

            // メール本文置換
            var temp = template.Replace("", "");

            // 本文（テキスト形式）
            message.Body = new TextPart("plain")
            {
                Text = "こんにちは。\nこれはMailKitを使用して送信されたメールです。"
            };

            return message;
        }

    }
}
