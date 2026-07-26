using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace manage
{
    public class DBAccess
    {
        public async Task funcAsync()
        {

            string connectionString = "Host=localhost;Username=myuser;Password=mypassword;Database=mydb";
            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            // 接続を開いてトランザクションを開始
            await using var connection = await dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1つ目の更新
                await using var cmd1 = new NpgsqlCommand("UPDATE accounts SET balance = balance - 100 WHERE id = 1", connection);
                await cmd1.ExecuteNonQueryAsync();

                // 2つ目の更新
                await using var cmd2 = new NpgsqlCommand("UPDATE accounts SET balance = balance + 100 WHERE id = 2", connection);
                await cmd2.ExecuteNonQueryAsync();

                // すべて成功したらコミット
                await transaction.CommitAsync();
                Console.WriteLine("トランザクションが成功しました。");
            }
            catch (Exception ex)
            {
                // エラーが起きたら自動または明示的にロールバック
                await transaction.RollbackAsync();
                Console.WriteLine($"エラーが発生したためロールバックしました: {ex.Message}");
            }

        }

        public async Task insertAsync()
        {

            string connectionString = "Host=localhost;Username=myuser;Password=mypassword;Database=mydb";
            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            // 追加するデータ
            string name = "山田 太郎";
            int age = 28;

            string sql = "INSERT INTO users (name, age) VALUES (@name, @age)";

            await using var cmd = dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("age", age);

            // SQLの実行（追加された行数が返る）
            int affectedRows = await cmd.ExecuteNonQueryAsync();

            Console.WriteLine($"{affectedRows} 件のレコードを追加しました。");

        }

        public async Task selectAsync()
        {
            string connectionString = "Host=localhost;Username=myuser;Password=mypassword;Database=mydb";
            await using var dataSource = NpgsqlDataSource.Create(connectionString);

            int minAge = 20;
            string sql = "SELECT id, name, age FROM users WHERE age >= @minAge";

            await using var cmd = dataSource.CreateCommand(sql);
            cmd.Parameters.AddWithValue("minAge", minAge);

            // リーダーを開く
            await using var reader = await cmd.ExecuteReaderAsync();

            // データの読み込みループ
            while (await reader.ReadAsync())
            {
                // 型を指定して安全にデータを取得
                int id = reader.GetInt32(0);          // 0番目の列 (id)
                string name = reader.GetString(1);     // 1番目の列 (name)
                int age = reader.GetInt32(2);          // 2番目の列 (age)

                Console.WriteLine($"ID: {id}, 名前: {name}, 年齢: {age}");
            }

        }
    }
}
