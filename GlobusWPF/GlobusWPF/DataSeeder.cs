using System;
using System.Data.SqlClient;

namespace GlobusWPF.Data
{
    public class DataSeeder
    {
        //public static void SeedTestData()
        //{
        //    try
        //    {
        //        using (var conn = new SqlConnection(DatabaseHelper.ConnectionString))
        //        {
        //            conn.Open();

        //            // Проверяем, есть ли данные
        //            var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Tours", conn);
        //            int count = (int)checkCmd.ExecuteScalar();

        //            if (count == 0)
        //            {
        //                // Добавляем тестовые данные
        //                string insertQuery = @"
        //                INSERT INTO Tours ([Код тура], [Наименование тура], Страна, [Продолжительность (дней)], 
        //                                  [Дата начала], [Стоимость (руб.)], [Тип автобуса], 
        //                                  Вместимость, [Свободных мест], [Имя файла фото])
        //                VALUES 
        //                (1, 'Экскурсия по Москве', 'Россия', 1, '2024-12-20', 5000, 'Микроавтобус', 15, 5, 'moscow.jpg'),
        //                (2, 'Отдых в Сочи', 'Россия', 7, '2024-12-25', 35000, 'Автобус', 40, 2, 'sochi.jpg'),
        //                (3, 'Тур в Париж', 'Франция', 5, '2024-12-22', 60000, 'Туристический автобус', 30, 10, 'paris.jpg')";

        //                var insertCmd = new SqlCommand(insertQuery, conn);
        //                insertCmd.ExecuteNonQuery();

        //                Console.WriteLine("Добавлены тестовые данные");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Ошибка при добавлении тестовых данных: {ex.Message}");
        //    }
        //}
    }
}