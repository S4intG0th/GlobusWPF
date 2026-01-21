using GlobusWPF.Data;
using GlobusWPF.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GlobusWPF
{
    public partial class AplicationsWindow : Window
    {
        private List<Aplication> allAplications = new List<Aplication>();
        private List<Aplication> filteredAplications = new List<Aplication>();
        private string currentSortBy = "Date";
        private bool sortAscending = false; // По умолчанию сортируем по убыванию даты

        public AplicationsWindow(User user)
        {

            InitializeComponent();
            CheckToursTableStructure();

            // Проверяем структуру базы данных
            string dbInfo = DatabaseHelper.GetDatabaseStructureInfo();
            MessageBox.Show(dbInfo, "Информация о базе данных",
                          MessageBoxButton.OK, MessageBoxImage.Information);

            // Инициализация комбобокса сортировки
            cbSortBy.SelectedIndex = 0;

            LoadAplications();
        }

        private void CheckToursTableStructure()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    string query = @"
                SELECT COLUMN_NAME, DATA_TYPE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'Tours'
                ORDER BY ORDINAL_POSITION";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        StringBuilder columnsInfo = new StringBuilder();
                        columnsInfo.AppendLine("Структура таблицы Tours:");

                        while (reader.Read())
                        {
                            string columnName = reader["COLUMN_NAME"].ToString();
                            string dataType = reader["DATA_TYPE"].ToString();
                            columnsInfo.AppendLine($"  {columnName} ({dataType})");
                        }

                        MessageBox.Show(columnsInfo.ToString(), "Информация о таблице Tours",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки таблицы Tours: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadAplications()
        {
            try
            {
                allAplications.Clear();
                filteredAplications.Clear();

                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    // Запрос с правильными названиями столбцов
                    string query = @"
            SELECT 
                a.[Код заявки] as AplicationId,
                a.[Код тура] as TourId,
                a.[Код клиента] as ClientId,
                a.[Дата заявки] as AplicationDate,
                a.[Статус заявки] as Status,
                a.[Количество человек] as PeopleCount,
                a.[Общая стоимость(руб.)] as TotalPrice,
                ISNULL(a.Комментарий, '') as Comment,
                t.[Наименование тура] as TourName
            FROM Aplications a
            LEFT JOIN Tours t ON a.[Код тура] = t.[Код тура]
            ORDER BY a.[Дата заявки] DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var aplication = new Aplication
                            {
                                AplicationId = ConvertSafe.ToInt32(reader["AplicationId"]),
                                TourId = ConvertSafe.ToInt32(reader["TourId"]),
                                ClientId = ConvertSafe.ToInt32(reader["ClientId"]),
                                AplicationDate = ConvertSafe.ToDateTime(reader["AplicationDate"]),
                                Status = ConvertSafe.ToString(reader["Status"]),
                                PeopleCount = ConvertSafe.ToInt32(reader["PeopleCount"]),
                                TotalPrice = ConvertSafe.ToDecimal(reader["TotalPrice"]),
                                Comment = ConvertSafe.ToString(reader["Comment"]),
                                TourName = ConvertSafe.ToString(reader["TourName"]),
                                // Клиента получаем из таблицы Users (если она существует)
                                ClientName = GetClientName(ConvertSafe.ToInt32(reader["ClientId"]))
                            };

                            allAplications.Add(aplication);
                        }
                    }
                }

                ApplyFiltersAndSort();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetClientName(int clientId)
        {
            try
            {
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

                    // Определяем, какие столбцы использовать для имени
                    string nameColumn = "";
                    if (userColumns.Contains("Фамилия") && userColumns.Contains("Имя"))
                    {
                        string middleNameColumn = userColumns.Contains("Отчество") ? "ISNULL([Отчество], '')" : "''";
                        nameColumn = $"[Фамилия] + ' ' + [Имя] + ' ' + {middleNameColumn}";
                    }
                    else if (userColumns.Contains("LastName") && userColumns.Contains("FirstName"))
                    {
                        nameColumn = "[LastName] + ' ' + [FirstName]";
                    }
                    else if (userColumns.Contains("Логин"))
                    {
                        nameColumn = "[Логин]";
                    }
                    else if (userColumns.Contains("Login"))
                    {
                        nameColumn = "[Login]";
                    }

                    if (!string.IsNullOrEmpty(nameColumn))
                    {
                        string query = $@"
                    SELECT {nameColumn} as FullName
                    FROM Users
                    WHERE [Код пользователя] = @ClientId";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ClientId", clientId);
                            object result = cmd.ExecuteScalar();

                            if (result != null && result != DBNull.Value)
                            {
                                return result.ToString().Trim();
                            }
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки, вернем ID
            }

            return $"Клиент #{clientId}";
        }

        private string GetTourName(int tourId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    // Пробуем разные возможные названия столбцов
                    string[] possibleNameColumns = {
                "Название тура", "Название", "TourName", "Name",
                "Наименование", "Tour", "Тур"
            };

                    foreach (var columnName in possibleNameColumns)
                    {
                        try
                        {
                            string query = $@"
                        SELECT [{columnName}]
                        FROM Tours 
                        WHERE [Код тура] = @TourId";

                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@TourId", tourId);
                                object result = cmd.ExecuteScalar();

                                if (result != null && result != DBNull.Value)
                                {
                                    return result.ToString();
                                }
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    // Если не нашли название, возвращаем ID
                    return $"Тур #{tourId}";
                }
            }
            catch
            {
                return $"Тур #{tourId}";
            }
        }

        // Вспомогательный класс для безопасного преобразования
        private static class ConvertSafe
        {
            public static int ToInt32(object value)
            {
                if (value == null || value == DBNull.Value)
                    return 0;
                return Convert.ToInt32(value);
            }

            public static decimal ToDecimal(object value)
            {
                if (value == null || value == DBNull.Value)
                    return 0;
                return Convert.ToDecimal(value);
            }

            public static DateTime ToDateTime(object value)
            {
                if (value == null || value == DBNull.Value)
                    return DateTime.Now;
                return Convert.ToDateTime(value);
            }

            public static string ToString(object value)
            {
                if (value == null || value == DBNull.Value)
                    return string.Empty;
                return value.ToString();
            }
        }

        private void ApplyFiltersAndSort()
        {
            try
            {
                string searchText = txtSearch?.Text?.ToLower() ?? string.Empty;
                string selectedStatus = (cbStatusFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все";

                filteredAplications = allAplications.Where(a =>
                {
                    if (a == null) return false;

                    string clientInfo = $"Клиент #{a.ClientId}";
                    string tourName = (a.TourName ?? string.Empty).ToLower();
                    string status = (a.Status ?? string.Empty).ToLower();

                    // Поиск
                    bool matchesSearch = string.IsNullOrWhiteSpace(searchText) ||
                                        clientInfo.ToLower().Contains(searchText) ||
                                        tourName.Contains(searchText) ||
                                        a.AplicationId.ToString().Contains(searchText);

                    // Фильтр по статусу
                    bool matchesStatus = selectedStatus == "Все" ||
                                        (a.Status ?? string.Empty).Equals(selectedStatus, StringComparison.OrdinalIgnoreCase);

                    return matchesSearch && matchesStatus;
                }).ToList();

                SortAplications();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortAplications()
        {
            try
            {
                if (filteredAplications == null || filteredAplications.Count == 0)
                {
                    lvAplications.ItemsSource = new List<Aplication>();
                    return;
                }

                IEnumerable<Aplication> sorted = filteredAplications;

                switch (currentSortBy)
                {
                    case "Date":
                        sorted = sortAscending
                            ? filteredAplications.OrderBy(a => a.AplicationDate)
                            : filteredAplications.OrderByDescending(a => a.AplicationDate);
                        break;

                    case "Status":
                        sorted = sortAscending
                            ? filteredAplications.OrderBy(a => a.Status ?? string.Empty)
                            : filteredAplications.OrderByDescending(a => a.Status ?? string.Empty);
                        break;

                    case "Price":
                        sorted = sortAscending
                            ? filteredAplications.OrderBy(a => a.TotalPrice)
                            : filteredAplications.OrderByDescending(a => a.TotalPrice);
                        break;

                    case "Client":
                        sorted = sortAscending
                            ? filteredAplications.OrderBy(a => a.ClientName ?? string.Empty)
                            : filteredAplications.OrderByDescending(a => a.ClientName ?? string.Empty);
                        break;
                }

                lvAplications.ItemsSource = sorted.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сортировки: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void cbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void cbSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSortBy.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                string newSortBy = selectedItem.Tag.ToString();

                // Если уже сортируем по этому полю, меняем направление
                if (currentSortBy == newSortBy)
                {
                    sortAscending = !sortAscending;
                }
                else
                {
                    currentSortBy = newSortBy;
                    sortAscending = true;
                }

                SortAplications();
            }
        }

        private void lvAplications_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvAplications.SelectedItem is Aplication selectedAplication)
            {
                EditAplication(selectedAplication);
            }
        }

        private void ViewAplication_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && int.TryParse(button.Tag.ToString(), out int aplicationId))
            {
                var aplication = allAplications.FirstOrDefault(a => a.AplicationId == aplicationId);
                if (aplication != null)
                {
                    EditAplication(aplication);
                }
            }
        }

        private void EditAplication(Aplication aplication)
        {
            try
            {
                var editWindow = new AplicationEditWindow(aplication);
                if (editWindow.ShowDialog() == true)
                {
                    LoadAplications();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы редактирования: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfirmAplication_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && int.TryParse(button.Tag.ToString(), out int aplicationId))
            {
                var result = MessageBox.Show("Подтвердить выбранную заявку?", "Подтверждение",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                        {
                            conn.Open();
                            string query = "UPDATE Aplications SET [Статус заявки] = 'Подтвержденные' " +
                                          "WHERE [Код заявки] = @AplicationId";

                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@AplicationId", aplicationId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("Заявка подтверждена успешно!", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadAplications();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка подтверждения: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteAplication_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && int.TryParse(button.Tag.ToString(), out int aplicationId))
            {
                var result = MessageBox.Show("Удалить выбранную заявку?", "Подтверждение удаления",
                                           MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                        {
                            conn.Open();
                            string query = "DELETE FROM Aplications WHERE [Код заявки] = @AplicationId";

                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@AplicationId", aplicationId);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Заявка удалена успешно!", "Успех",
                                                  MessageBoxButton.OK, MessageBoxImage.Information);
                                    LoadAplications();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnNewAplication_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var editWindow = new AplicationEditWindow();
                if (editWindow.ShowDialog() == true)
                {
                    LoadAplications();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания новой заявки: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAplications();
        }
    }
}