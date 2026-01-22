using GlobusWPF.Data;
using GlobusWPF.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GlobusWPF
{
    public partial class ToursWindow : Window
    {
        private List<Tour> allTours = new List<Tour>();
        private List<Tour> filteredTours = new List<Tour>();

        public ToursWindow()
        {
            InitializeComponent();
            LoadTours();
            this.Loaded += ToursWindow_Loaded;
        }
        private void ToursWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Теперь все элементы XAML созданы
            ApplyFilters();
        }

        private void LoadTours()
        {
            try
            {
                allTours.Clear();

                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    string query = @"
                SELECT 
                    [Код тура],
                    [Наименование тура],
                    Страна,
                    [Продолжительность (дней)],
                    [Дата начала],
                    [Стоимость (руб.)],
                    [Тип автобуса],
                    Вместимость,
                    [Свободных мест],
                    [Имя файла фото]
                FROM Tours
                ORDER BY [Код тура] DESC"; // Сортируем по ID, чтобы новые были сверху

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tour = new Tour
                            {
                                TourId = reader["Код тура"] != DBNull.Value ? Convert.ToInt32(reader["Код тура"]) : 0,
                                TourName = reader["Наименование тура"] != DBNull.Value ? reader["Наименование тура"].ToString() : "",
                                CountryName = reader["Страна"] != DBNull.Value ? reader["Страна"].ToString() : "",
                                DurationDays = reader["Продолжительность (дней)"] != DBNull.Value ? Convert.ToInt32(reader["Продолжительность (дней)"]) : 0,
                                StartDate = reader["Дата начала"] != DBNull.Value ? Convert.ToDateTime(reader["Дата начала"]) : DateTime.MinValue,
                                BasePrice = reader["Стоимость (руб.)"] != DBNull.Value ? Convert.ToInt32(reader["Стоимость (руб.)"]) : 0,
                                BusTypeName = reader["Тип автобуса"] != DBNull.Value ? reader["Тип автобуса"].ToString() : "",
                                Capacity = reader["Вместимость"] != DBNull.Value ? Convert.ToInt32(reader["Вместимость"]) : 0,
                                FreeSeats = reader["Свободных мест"] != DBNull.Value ? Convert.ToInt32(reader["Свободных мест"]) : 0,
                                PhotoFileName = reader["Имя файла фото"] != DBNull.Value ? reader["Имя файла фото"].ToString() : ""
                            };

                            allTours.Add(tour);
                        }
                    }
                }

                ApplyFilters();

                // Выводим отладочную информацию
                Debug.WriteLine($"Загружено туров: {allTours.Count}");
                foreach (var tour in allTours.Take(5))
                {
                    Debug.WriteLine($"Тур ID: {tour.TourId}, Название: {tour.TourName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки туров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                string searchText = txtSearch?.Text?.ToLower() ?? string.Empty;

                filteredTours = allTours.Where(t =>
                {
                    if (t == null) return false;

                    bool matchesSearch = string.IsNullOrWhiteSpace(searchText) ||
                                        t.TourName.ToLower().Contains(searchText) ||
                                        t.CountryName.ToLower().Contains(searchText) ||
                                        t.BusTypeName.ToLower().Contains(searchText);

                    bool matchesActive = chkActiveTours?.IsChecked != true ||
                                        t.StartDate >= DateTime.Now.Date;

                    bool matchesFewSeats = chkFewSeats?.IsChecked != true ||
                                          t.IsFewSeats;

                    return matchesSearch && matchesActive && matchesFewSeats;
                }).ToList();

                if (lvTours != null)
                {
                    lvTours.ItemsSource = filteredTours;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        // ДОБАВЛЯЕМ ВСЕ ОБРАБОТЧИКИ:

        private void btnNewTour_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new TourEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadTours(); // Перезагружает все туры с правильными ID из БД
            }
        }

        private void btnEditTour_Click(object sender, RoutedEventArgs e)
        {
            if (lvTours.SelectedItem is Tour selectedTour)
            {
                var editWindow = new TourEditWindow(selectedTour);
                if (editWindow.ShowDialog() == true)
                {
                    LoadTours();
                }
            }
            else
            {
                MessageBox.Show("Выберите тур для редактирования", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDeleteTour_Click(object sender, RoutedEventArgs e)
        {
            if (lvTours.SelectedItem is Tour selectedTour)
            {
                var result = MessageBox.Show($"Удалить тур '{selectedTour.TourName}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                        {
                            conn.Open();

                            string checkQuery = "SELECT COUNT(*) FROM Aplic WHERE [Код тура] = @TourId";
                            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                            {
                                checkCmd.Parameters.AddWithValue("@TourId", selectedTour.TourId);
                                int aplicationCount = (int)checkCmd.ExecuteScalar();

                                if (aplicationCount > 0)
                                {
                                    MessageBox.Show("Нельзя удалить тур, на который есть заявки!", "Ошибка",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }

                            string deleteQuery = "DELETE FROM Tours WHERE [Код тура] = @TourId";
                            using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                            {
                                deleteCmd.Parameters.AddWithValue("@TourId", selectedTour.TourId);
                                deleteCmd.ExecuteNonQuery();
                            }

                            MessageBox.Show("Тур удален успешно!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadTours();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления тура: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите тур для удаления", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTours();
        }
    }
}