using GlobusWPF.Data;
using GlobusWPF.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GlobusWPF
{
    public partial class AplicationEditWindow : Window
    {
        private Aplication currentAplication;
        private bool isNew;

        public AplicationEditWindow(Aplication aplication = null)
        {
            InitializeComponent();
            LoadClients();
            LoadTours();

            if (aplication == null)
            {
                // Создание новой заявки
                currentAplication = new Aplication
                {
                    AplicationDate = DateTime.Now,
                    Status = "Новые",
                    PeopleCount = 1
                };
                isNew = true;
                Title = "Новая заявка";
            }
            else
            {
                // Редактирование существующей
                currentAplication = aplication;
                isNew = false;
                Title = "Редактирование заявки";
            }

            LoadAplicationData();
        }

        private void CheckUsersTableStructure(SqlConnection conn)
        {
            try
            {
                string query = @"
            SELECT COLUMN_NAME, DATA_TYPE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'Users'
            ORDER BY ORDINAL_POSITION";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string columnName = reader["COLUMN_NAME"].ToString();
                        string dataType = reader["DATA_TYPE"].ToString();
                        Debug.WriteLine($"Users table column: {columnName} ({dataType})");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking Users table: {ex.Message}");
            }
        }

        private void LoadClients()
        {
            try
            {
                cbClient.Items.Clear();

                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    // Проверим структуру таблицы Users
                    string checkQuery = @"
                SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'Users'";

                    List<string> userColumns = new List<string>();
                    using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userColumns.Add(reader["COLUMN_NAME"].ToString());
                        }
                    }

                    // Строим запрос в зависимости от наличия столбцов
                    string query;
                    if (userColumns.Contains("Фамилия") && userColumns.Contains("Имя"))
                    {
                        string middleNamePart = userColumns.Contains("Отчество") ?
                            " + ' ' + ISNULL([Отчество], '')" : "";

                        query = $@"
                SELECT 
                    [Код пользователя] as UserId,
                    [Фамилия] + ' ' + [Имя]{middleNamePart} as FullName
                FROM Users
                WHERE [Роль] LIKE '%клиент%' OR [Роль] IS NULL
                ORDER BY [Фамилия], [Имя]";
                    }
                    else if (userColumns.Contains("LastName") && userColumns.Contains("FirstName"))
                    {
                        query = @"
                SELECT 
                    [Код пользователя] as UserId,
                    [LastName] + ' ' + [FirstName] as FullName
                FROM Users
                WHERE [Role] LIKE '%client%' OR [Role] IS NULL
                ORDER BY [LastName], [FirstName]";
                    }
                    else if (userColumns.Contains("Логин"))
                    {
                        query = @"
                SELECT 
                    [Код пользователя] as UserId,
                    [Логин] as FullName
                FROM Users
                ORDER BY [Логин]";
                    }
                    else
                    {
                        // Если не нашли подходящих столбцов
                        query = @"
                SELECT 
                    [Код пользователя] as UserId,
                    'Пользователь ' + CAST([Код пользователя] as nvarchar) as FullName
                FROM Users
                ORDER BY [Код пользователя]";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cbClient.Items.Add(new
                            {
                                ClientId = Convert.ToInt32(reader["UserId"]),
                                FullName = reader["FullName"].ToString()
                            });
                        }
                    }

                    if (cbClient.Items.Count == 0)
                    {
                        CreateTestClients();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                CreateTestClients();
            }
        }

        private void CreateTestClients()
        {
            // Создаем тестовых клиентов
            for (int i = 1; i <= 5; i++)
            {
                cbClient.Items.Add(new
                {
                    ClientId = i,
                    FullName = $"Тестовый клиент {i}"
                });
            }
            cbClient.SelectedIndex = 0;
        }

        private void LoadTours()
        {
            try
            {
                cbTour.Items.Clear();

                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    // Используем правильные названия столбцов
                    string query = @"
            SELECT 
                [Код тура] as TourId,
                [Наименование тура] as TourName,
                [Стоимость (руб.)] as Price
            FROM Tours
            WHERE [Дата начала] > GETDATE() -- только будущие туры
            ORDER BY [Наименование тура]";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool hasData = false;
                        while (reader.Read())
                        {
                            hasData = true;
                            cbTour.Items.Add(new
                            {
                                TourId = Convert.ToInt32(reader["TourId"]),
                                TourName = reader["TourName"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"])
                            });
                        }

                        if (!hasData)
                        {
                            // Если нет активных туров, покажем все
                            LoadAllTours(conn);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки туров: {ex.Message}", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                CreateTestTours();
            }
        }

        private void LoadAllTours(SqlConnection conn)
        {
            try
            {
                string query = @"
        SELECT 
            [Код тура] as TourId,
            [Наименование тура] as TourName,
            [Стоимость (руб.)] as Price
        FROM Tours
        ORDER BY [Наименование тура]";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cbTour.Items.Add(new
                        {
                            TourId = Convert.ToInt32(reader["TourId"]),
                            TourName = reader["TourName"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"])
                        });
                    }
                }
            }
            catch
            {
                CreateTestTours();
            }
        }

        private string FindColumn(List<string> availableColumns, string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                if (availableColumns.Contains(name))
                    return name;
            }
            return null;
        }

        private void CreateTestTours()
        {
            // Создаем тестовые туры
            var testTours = new[]
            {
        new { TourId = 1, TourName = "Тур в Москву", Price = 15000m },
        new { TourId = 2, TourName = "Тур в Сочи", Price = 20000m },
        new { TourId = 3, TourName = "Тур в Крым", Price = 25000m }
    };

            foreach (var tour in testTours)
            {
                cbTour.Items.Add(tour);
            }
            cbTour.SelectedIndex = 0;
        }

        private void LoadAplicationData()
        {
            // Загрузка клиента
            foreach (var item in cbClient.Items)
            {
                dynamic client = item;
                if (client.ClientId == currentAplication.ClientId)
                {
                    cbClient.SelectedItem = item;
                    break;
                }
            }

            // Загрузка тура
            foreach (var item in cbTour.Items)
            {
                dynamic tour = item;
                if (tour.TourId == currentAplication.TourId)
                {
                    cbTour.SelectedItem = item;
                    break;
                }
            }

            txtPeopleCount.Text = currentAplication.PeopleCount.ToString();
            UpdateTotalPrice();

            // Установка статуса
            foreach (ComboBoxItem item in cbStatus.Items)
            {
                if (item.Content.ToString() == currentAplication.Status)
                {
                    cbStatus.SelectedItem = item;
                    break;
                }
            }

            dpAplicationDate.SelectedDate = currentAplication.AplicationDate;
            txtComment.Text = currentAplication.Comment;
        }

        private void cbTour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void txtPeopleCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            if (cbTour.SelectedItem != null && int.TryParse(txtPeopleCount.Text, out int peopleCount))
            {
                dynamic selectedTour = cbTour.SelectedItem;
                decimal tourPrice = selectedTour.Price;

                // Обновляем отображение
                txtTourPrice.Text = $"{tourPrice:N0} руб.";
                txtTotalPrice.Text = $"{(tourPrice * peopleCount):N0} руб.";
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (cbClient.SelectedItem == null)
                {
                    MessageBox.Show("Выберите клиента", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cbTour.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тур", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtPeopleCount.Text, out int peopleCount) || peopleCount < 1)
                {
                    MessageBox.Show("Введите корректное количество человек (минимум 1)", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем данные
                dynamic selectedClient = cbClient.SelectedItem;
                dynamic selectedTour = cbTour.SelectedItem;

                // Рассчитываем стоимость
                decimal totalPrice = selectedTour.Price * peopleCount;

                // Сохраняем в базу данных
                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    if (isNew)
                    {
                        // Вставка новой записи
                        string query = @"
                INSERT INTO Aplications 
                ([Код тура], [Код клиента], [Дата заявки], [Статус заявки], 
                 [Количество человек], [Общая стоимость(руб.)], Комментарий)
                VALUES (@TourId, @ClientId, @AplicationDate, @Status, 
                        @PeopleCount, @TotalPrice, @Comment);
                SELECT SCOPE_IDENTITY();";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@TourId", selectedTour.TourId);
                            cmd.Parameters.AddWithValue("@ClientId", selectedClient.ClientId);
                            cmd.Parameters.AddWithValue("@AplicationDate", dpAplicationDate.SelectedDate ?? DateTime.Now);
                            cmd.Parameters.AddWithValue("@Status", (cbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Новые");
                            cmd.Parameters.AddWithValue("@PeopleCount", peopleCount);
                            cmd.Parameters.AddWithValue("@TotalPrice", totalPrice);
                            cmd.Parameters.AddWithValue("@Comment", txtComment.Text ?? "");

                            currentAplication.AplicationId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                    else
                    {
                        // Обновление существующей записи
                        string query = @"
                UPDATE Aplications SET
                [Код тура] = @TourId,
                [Код клиента] = @ClientId,
                [Дата заявки] = @AplicationDate,
                [Статус заявки] = @Status,
                [Количество человек] = @PeopleCount,
                [Общая стоимость(руб.)] = @TotalPrice,
                Комментарий = @Comment
                WHERE [Код заявки] = @AplicationId";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@AplicationId", currentAplication.AplicationId);
                            cmd.Parameters.AddWithValue("@TourId", selectedTour.TourId);
                            cmd.Parameters.AddWithValue("@ClientId", selectedClient.ClientId);
                            cmd.Parameters.AddWithValue("@AplicationDate", dpAplicationDate.SelectedDate ?? currentAplication.AplicationDate);
                            cmd.Parameters.AddWithValue("@Status", (cbStatus.SelectedItem as ComboBoxItem)?.Content.ToString() ?? currentAplication.Status);
                            cmd.Parameters.AddWithValue("@PeopleCount", peopleCount);
                            cmd.Parameters.AddWithValue("@TotalPrice", totalPrice);
                            cmd.Parameters.AddWithValue("@Comment", txtComment.Text ?? currentAplication.Comment);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Заявка успешно сохранена!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}