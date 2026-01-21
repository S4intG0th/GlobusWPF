using GlobusWPF.Data;
using GlobusWPF.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            ORDER BY [Дата начала]";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tour = new Tour
                            {
                                TourId = Convert.ToInt32(reader["Код тура"]),
                                TourName = reader["Наименование тура"]?.ToString() ?? "",
                                CountryName = reader["Страна"]?.ToString() ?? "",
                                DurationDays = Convert.ToInt32(reader["Продолжительность (дней)"]),
                                StartDate = Convert.ToDateTime(reader["Дата начала"]),
                                BasePrice = Convert.ToDecimal(reader["Стоимость (руб.)"]),
                                BusTypeName = reader["Тип автобуса"]?.ToString() ?? "",
                                Capacity = Convert.ToInt32(reader["Вместимость"]),
                                FreeSeats = Convert.ToInt32(reader["Свободных мест"]),
                                PhotoFileName = reader["Имя файла фото"]?.ToString(),
                                DiscountPrice = null
                            };

                            allTours.Add(tour);
                        }
                    }
                }

                ApplyFilters();
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

                    bool matchesDiscount = chkWithDiscount?.IsChecked != true ||
                                          t.HasDiscount;

                    bool matchesFewSeats = chkFewSeats?.IsChecked != true ||
                                          t.IsFewSeats;

                    return matchesSearch && matchesActive && matchesDiscount && matchesFewSeats;
                }).ToList();

                lvTours.ItemsSource = filteredTours;
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
                LoadTours();
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

                            string checkQuery = "SELECT COUNT(*) FROM Aplications WHERE [Код тура] = @TourId";
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