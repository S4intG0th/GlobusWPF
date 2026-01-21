using System;
using System.Data.SqlClient;
using System.Text;

namespace GlobusWPF.Data
{
    public static class DatabaseHelper
    {
        public static string ConnectionString =>
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GlobusWPFdb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public static string GetDatabaseStructureInfo()
        {
            StringBuilder info = new StringBuilder();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Проверяем структуру таблицы Tours
                    string columnsQuery = @"
                SELECT COLUMN_NAME, DATA_TYPE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'Tours'
                ORDER BY ORDINAL_POSITION";

                    using (SqlCommand cmd = new SqlCommand(columnsQuery, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        info.AppendLine("=== СТРУКТУРА ТАБЛИЦЫ Tours ===");
                        while (reader.Read())
                        {
                            string columnName = reader["COLUMN_NAME"].ToString();
                            string dataType = reader["DATA_TYPE"].ToString();
                            info.AppendLine($"  {columnName} ({dataType})");
                        }
                    }

                    // Если нет столбца 'Цена со скидкой', добавляем его
                    string checkColumnQuery = @"
                IF NOT EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Tours' AND COLUMN_NAME = 'Цена со скидкой'
                )
                BEGIN
                    ALTER TABLE Tours ADD [Цена со скидкой] DECIMAL(10,2) NULL;
                    PRINT 'Столбец добавлен';
                END";

                    using (SqlCommand cmd = new SqlCommand(checkColumnQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                return info.ToString();
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }
        public static void InitializeDatabase()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Проверяем существование базы данных
                    if (!DatabaseExists(conn))
                    {
                        CreateDatabase();
                    }

                    // Пересоздаем таблицы с нуля
                    RecreateTables(conn);

                    Console.WriteLine("База данных успешно инициализирована!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка инициализации базы данных: {ex.Message}");
            }
        }

        private static bool DatabaseExists(SqlConnection conn)
        {
            try
            {
                string checkDbQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = 'GlobusWPFdb'";
                using (SqlCommand cmd = new SqlCommand(checkDbQuery, conn))
                {
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static void CreateDatabase()
        {
            string masterConnString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;";

            using (SqlConnection masterConn = new SqlConnection(masterConnString))
            {
                masterConn.Open();

                string createDbQuery = @"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GlobusWPFdb')
                CREATE DATABASE GlobusWPFdb";

                using (SqlCommand cmd = new SqlCommand(createDbQuery, masterConn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void RecreateTables(SqlConnection conn)
        {
            // Удаляем таблицы если они существуют (в правильном порядке)
            DropTableIfExists(conn, "Aplications");
            DropTableIfExists(conn, "Tours");
            DropTableIfExists(conn, "Users");

            // Создаем таблицы
            CreateUsersTable(conn);
            CreateToursTable(conn);
            CreateAplicationsTable(conn);

            // Добавляем тестовые данные
            SeedTestData(conn);
        }

        private static void DropTableIfExists(SqlConnection conn, string tableName)
        {
            try
            {
                string query = $"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL DROP TABLE {tableName}";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления таблицы {tableName}: {ex.Message}");
            }
        }

        private static void CreateUsersTable(SqlConnection conn)
        {
            string query = @"
            CREATE TABLE Users (
                [Код пользователя] INT IDENTITY(1,1) PRIMARY KEY,
                [Роль] NVARCHAR(50) NOT NULL,
                [ФИО] NVARCHAR(100) NOT NULL,
                [Логин] NVARCHAR(50) UNIQUE NOT NULL,
                [Пароль] NVARCHAR(50) NOT NULL
            )";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void CreateToursTable(SqlConnection conn)
        {
            string query = @"
            CREATE TABLE Tours (
                [Код тура] INT IDENTITY(1,1) PRIMARY KEY,
                [Наименование тура] NVARCHAR(100) NOT NULL,
                Страна NVARCHAR(50) NOT NULL,
                [Продолжительность (дней)] INT NOT NULL,
                [Дата начала] DATE NOT NULL,
                [Стоимость (руб.)] DECIMAL(10,2) NOT NULL,
                [Тип автобуса] NVARCHAR(50) NOT NULL,
                Вместимость INT NOT NULL,
                [Свободных мест] INT NOT NULL,
                [Имя файла фото] NVARCHAR(100),
                [Цена со скидкой] DECIMAL(10,2) NULL
            )";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void CreateAplicationsTable(SqlConnection conn)
        {
            string query = @"
            CREATE TABLE Aplications (
                [Код заявки] INT IDENTITY(1,1) PRIMARY KEY,
                [Код тура] INT NOT NULL,
                [Код клиента] INT NOT NULL,
                [Дата заявки] DATE NOT NULL DEFAULT GETDATE(),
                [Статус заявки] NVARCHAR(50) NOT NULL DEFAULT 'Новые',
                [Количество человек] INT NOT NULL DEFAULT 1,
                [Общая стоимость(руб.)] DECIMAL(10,2) NOT NULL,
                Комментарий NVARCHAR(500) NULL,
                FOREIGN KEY ([Код тура]) REFERENCES Tours([Код тура]),
                FOREIGN KEY ([Код клиента]) REFERENCES Users([Код пользователя])
            )";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void SeedTestData(SqlConnection conn)
        {
            try
            {
                // Очищаем таблицы
                string clearData = @"
                DELETE FROM Aplications;
                DELETE FROM Tours;
                DELETE FROM Users;
                DBCC CHECKIDENT ('Users', RESEED, 0);
                DBCC CHECKIDENT ('Tours', RESEED, 0);
                DBCC CHECKIDENT ('Aplications', RESEED, 0);";

                using (SqlCommand cmd = new SqlCommand(clearData, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Добавляем пользователей
                string insertUsers = @"
                INSERT INTO Users ([Роль], [ФИО], [Логин], [Пароль]) VALUES
                ('Менеджер', 'Сидорова Анна Владимировна', 'manager@globus.ru', '9k3l5m'),
                ('Администратор', 'Петров Иван Сергеевич', 'admin@globus.ru', '7f8d2a'),
                ('Клиент', 'Иванов Сергей Петрович', 'client1@test.ru', '123456'),
                ('Клиент', 'Смирнова Ольга Игоревна', 'client2@test.ru', '123456')";

                using (SqlCommand cmd = new SqlCommand(insertUsers, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Добавляем туры
                string insertTours = @"
                INSERT INTO Tours ([Наименование тура], Страна, [Продолжительность (дней)], 
                                  [Дата начала], [Стоимость (руб.)], [Тип автобуса], 
                                  Вместимость, [Свободных мест], [Имя файла фото], [Цена со скидкой]) VALUES
                ('Экскурсия по Москве', 'Россия', 1, DATEADD(day, 7, GETDATE()), 5000.00, 'Микроавтобус', 15, 5, 'moscow.jpg', NULL),
                ('Отдых в Сочи', 'Россия', 7, DATEADD(day, 14, GETDATE()), 35000.00, 'Автобус', 40, 2, 'sochi.jpg', 28000.00),
                ('Тур в Париж', 'Франция', 5, DATEADD(day, 21, GETDATE()), 60000.00, 'Туристический автобус', 30, 10, 'paris.jpg', 48000.00)";

                using (SqlCommand cmd = new SqlCommand(insertTours, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Добавляем заявки
                string insertAplications = @"
                INSERT INTO Aplications ([Код тура], [Код клиента], [Дата заявки], 
                                        [Статус заявки], [Количество человек], 
                                        [Общая стоимость(руб.)], Комментарий) VALUES
                (1, 3, GETDATE(), 'Новые', 2, 10000.00, 'Хочу экскурсию'),
                (2, 4, DATEADD(day, -1, GETDATE()), 'Подтвержденные', 3, 84000.00, 'Семейный отдых')";

                using (SqlCommand cmd = new SqlCommand(insertAplications, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления тестовых данных: {ex.Message}");
            }
        }

        public static string TestConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    return "Подключение к базе данных успешно!";
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка подключения: {ex.Message}";
            }
        }
    }
}