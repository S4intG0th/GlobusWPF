using System;
using System.Data.SqlClient;
using System.Text;
using System.Windows;

namespace GlobusWPF.Data
{
    public static class DatabaseHelper
    {
        public static string ConnectionString =>
@"Server=(localdb)\MSSQLLocalDB;Database=GlobusWPFdb;Integrated Security=True;";

        public static string GetDatabaseStructureInfo()
        {
            StringBuilder info = new StringBuilder();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // 1. Получаем список всех таблиц
                    info.AppendLine("=== ТАБЛИЦЫ В БАЗЕ ДАННЫХ ===");
                    string tablesQuery = @"
                        SELECT 
                            TABLE_SCHEMA,
                            TABLE_NAME,
                            TABLE_TYPE
                        FROM INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_TYPE = 'BASE TABLE'
                        ORDER BY TABLE_NAME";

                    using (SqlCommand cmd = new SqlCommand(tablesQuery, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string schema = reader["TABLE_SCHEMA"].ToString();
                            string tableName = reader["TABLE_NAME"].ToString();
                            info.AppendLine($"  {schema}.{tableName}");
                        }
                    }

                    info.AppendLine("\n=== СТРУКТУРА ТАБЛИЦЫ Aplications ===");

                    // 2. Проверяем столбцы таблицы Aplications
                    string columnsQuery = @"
                        SELECT 
                            COLUMN_NAME,
                            DATA_TYPE,
                            IS_NULLABLE,
                            CHARACTER_MAXIMUM_LENGTH
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = 'Aplications'
                        ORDER BY ORDINAL_POSITION";

                    using (SqlCommand cmd = new SqlCommand(columnsQuery, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            info.AppendLine("Таблица 'Aplications' не найдена!");

                            // Ищем похожие таблицы
                            info.AppendLine("\n=== ПОИСК ПОХОЖИХ ТАБЛИЦ ===");
                            string similarTablesQuery = @"
                                SELECT TABLE_NAME 
                                FROM INFORMATION_SCHEMA.TABLES
                                WHERE TABLE_NAME LIKE '%lication%' 
                                   OR TABLE_NAME LIKE '%заяв%'
                                   OR TABLE_NAME LIKE '%appli%'
                                ORDER BY TABLE_NAME";

                            using (SqlCommand cmd2 = new SqlCommand(similarTablesQuery, conn))
                            using (SqlDataReader reader2 = cmd2.ExecuteReader())
                            {
                                while (reader2.Read())
                                {
                                    info.AppendLine($"  Возможно: {reader2["TABLE_NAME"]}");
                                }
                            }
                        }
                        else
                        {
                            while (reader.Read())
                            {
                                string columnName = reader["COLUMN_NAME"].ToString();
                                string dataType = reader["DATA_TYPE"].ToString();
                                string nullable = reader["IS_NULLABLE"].ToString();
                                string maxLength = reader["CHARACTER_MAXIMUM_LENGTH"].ToString();

                                info.AppendLine($"  {columnName} ({dataType})");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                info.AppendLine($"Ошибка при получении информации: {ex.Message}");
            }

            return info.ToString();
        }
    }
}