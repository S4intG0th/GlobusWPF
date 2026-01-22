using GlobusWPF.Data;
using GlobusWPF.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
        private bool sortAscending = false;
        private bool isInitialized = false;

        public AplicationsWindow(User user)
        {
            InitializeComponent();
            isInitialized = true;
            LoadAplications();
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

                    // Прямой запрос к таблице Aplications с JOIN на Tours
                    string query = @"
                SELECT 
                    a.[Код заявки] as AplicationId,
                    a.[Код тура] as TourId,
                    a.[Код клиента] as ClientId,
                    a.[Дата заявки] as AplicationDate,
                    a.[Статус заявки] as Status,
                    a.[Количество человек] as PeopleCount,
                    a.[Общая стоимость (руб.)] as TotalPrice,
                    ISNULL(a.Комментарий, '') as Comment,
                    t.[Наименование тура] as TourName
                FROM Aplications a
                LEFT JOIN Tours t ON a.[Код тура] = t.[Код тура]
                ORDER BY a.[Дата заявки] DESC"; // Сортировка по дате заявки

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var aplication = new Aplication
                            {
                                AplicationId = reader["AplicationId"] != DBNull.Value ?
                                              Convert.ToInt32(reader["AplicationId"]) : 0,
                                TourId = reader["TourId"] != DBNull.Value ?
                                        Convert.ToInt32(reader["TourId"]) : 0,
                                ClientId = reader["ClientId"] != DBNull.Value ?
                                          Convert.ToInt32(reader["ClientId"]) : 0,
                                AplicationDate = reader["AplicationDate"] != DBNull.Value ?
                                               Convert.ToDateTime(reader["AplicationDate"]) : DateTime.Now,
                                Status = reader["Status"] != DBNull.Value ?
                                        reader["Status"].ToString() : "Новые",
                                PeopleCount = reader["PeopleCount"] != DBNull.Value ?
                                             Convert.ToInt32(reader["PeopleCount"]) : 1,
                                TotalPrice = reader["TotalPrice"] != DBNull.Value ?
                                            Convert.ToInt32(reader["TotalPrice"]) : 0,
                                Comment = reader["Comment"] != DBNull.Value ?
                                         reader["Comment"].ToString() : "",
                                TourName = reader["TourName"] != DBNull.Value ?
                                          reader["TourName"].ToString() : ""
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

        private void ApplyFiltersAndSort()
        {
            try
            {
                // Проверяем, что элементы управления существуют
                if (txtSearch == null || cbStatusFilter == null || lvAplications == null)
                {
                    return;
                }

                string searchText = txtSearch.Text?.ToLower() ?? string.Empty;

                // Безопасное получение выбранного статуса
                string selectedStatus = "Все";
                if (cbStatusFilter.SelectedItem is ComboBoxItem selectedItem)
                {
                    selectedStatus = selectedItem.Content?.ToString() ?? "Все";
                }

                filteredAplications = allAplications.Where(a =>
                {
                    if (a == null) return false;

                    string clientInfo = a.ClientName ?? $"Клиент #{a.ClientId}";
                    string tourName = a.TourName ?? string.Empty;
                    string status = a.Status ?? string.Empty;

                    bool matchesSearch = string.IsNullOrWhiteSpace(searchText) ||
                                        clientInfo.ToLower().Contains(searchText) ||
                                        tourName.ToLower().Contains(searchText) ||
                                        a.AplicationId.ToString().Contains(searchText);

                    bool matchesStatus = selectedStatus == "Все" ||
                                        string.Equals(status, selectedStatus, StringComparison.OrdinalIgnoreCase);

                    return matchesSearch && matchesStatus;
                }).ToList();

                SortAplications();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortAplications()
        {
            try
            {
                // Проверяем, что ListView существует
                if (lvAplications == null)
                {
                    return;
                }

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
                            ? filteredAplications.OrderBy(a => a?.AplicationDate ?? DateTime.MinValue)
                            : filteredAplications.OrderByDescending(a => a?.AplicationDate ?? DateTime.MaxValue);
                        break;

                    case "Status":
                        sorted = sortAscending
                            ? filteredAplications.OrderBy(a => a?.Status ?? "")
                            : filteredAplications.OrderByDescending(a => a?.Status ?? "");
                        break;

                    case "Price":
                        sorted = sortAscending
                            ? filteredAplications.OrderBy(a => a?.TotalPrice ?? 0)
                            : filteredAplications.OrderByDescending(a => a?.TotalPrice ?? 0);
                        break;

                    case "Client":
                        sorted = sortAscending
                            ? filteredAplications.OrderBy(a => a?.ClientName ?? "")
                            : filteredAplications.OrderByDescending(a => a?.ClientName ?? "");
                        break;
                }

                lvAplications.ItemsSource = sorted.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сортировки: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Устанавливаем пустой источник при ошибке
                if (lvAplications != null)
                {
                    lvAplications.ItemsSource = new List<Aplication>();
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInitialized) return;
            ApplyFiltersAndSort();
        }

        private void cbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized) return;
            ApplyFiltersAndSort();
        }

        private void cbSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized) return;

            if (cbSortBy.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                string newSortBy = selectedItem.Tag.ToString();

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
            if (!isInitialized) return;

            if (lvAplications.SelectedItem is Aplication selectedAplication)
            {
                EditAplication(selectedAplication);
            }
        }

        private void ViewAplication_Click(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;

            if (sender is Button button && button.Tag != null && int.TryParse(button.Tag.ToString(), out int aplicationId))
            {
                var aplication = allAplications.FirstOrDefault(a => a != null && a.AplicationId == aplicationId);
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
            if (!isInitialized) return;

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
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Заявка подтверждена успешно!", "Успех",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                    LoadAplications();
                                }
                            }
                        }
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
            if (!isInitialized) return;

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
            if (!isInitialized) return;

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
            if (!isInitialized) return;
            LoadAplications();
        }
    }
}