using GlobusWPF.Data;
using GlobusWPF.Models;
using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GlobusWPF
{
    public partial class AplicationsWindow : Window
    {
        private ObservableCollection<Aplication> aplications = new ObservableCollection<Aplication>();

        public AplicationsWindow(User user)
        {
            InitializeComponent();
            LoadAplications();
        }

        private void LoadAplications()
        {
            aplications.Clear();

            using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();
                string query = @"
                SELECT 
                    [Код заявки],
                    [Код тура],
                    [Код клиента],
                    [Дата заявки],
                    [Статус заявки],
                    [Количество человек],
                    [Общая стоимость(руб.)],
                    Комментарий
                FROM Aplications
                ORDER BY [Дата заявки]";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var aplication = new Aplication
                        {
                            AplicationId = Convert.ToInt32(reader["Код заявки"]),
                            TourId = Convert.ToInt32(reader["Код тура"]),
                            ClientId = Convert.ToInt32(reader["Код клиента"]),
                            AplicationDate = Convert.ToDateTime(reader["Дата заявки"]),
                            Status = reader["Статус заявки"]?.ToString() ?? "",
                            PeopleCount = Convert.ToInt32(reader["Количество человек"]),
                            TotalPrice = Convert.ToInt32(reader["Общая стоимость(руб.)"]),
                            Comment = reader["Комментарий"]?.ToString() ?? "",

                        };

                        aplications.Add(aplication);
                    }
                }
            }

            lvAplications.ItemsSource = aplications;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAplications();
        }

        private void cbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterAplications();
        }

        private void FilterAplications()
        {
            try
            {
                if (lvAplications == null || aplications == null)
                {
                    MessageBox.Show("ListView или коллекция не инициализированы", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // ИСПРАВЛЕНИЕ: используем aplications вместо allAplications
                var filtered = aplications.AsEnumerable();

                // Поиск
                string searchText = txtSearch.Text.ToLower();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filtered = filtered.Where(b =>
                        (b.ClientName?.ToLower() ?? "").Contains(searchText) ||
                        (b.TourName?.ToLower() ?? "").Contains(searchText) ||
                        (b.AplicationId.ToString()?.ToLower() ?? "").Contains(searchText));
                }

                // Фильтрация по статусу
                string selectedStatus = (cbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
                if (selectedStatus != null && selectedStatus != "Все")
                {
                    filtered = filtered.Where(b => b.Status == selectedStatus);
                }

                lvAplications.ItemsSource = filtered.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewAplication_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Просмотр заявки");
        }

        private void ConfirmAplication_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Заявка подтверждена!");
        }

        private void btnNewAplication_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Создание новой заявки");
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAplications();
        }
    }
}