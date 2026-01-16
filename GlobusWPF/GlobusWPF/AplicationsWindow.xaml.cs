using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GlobusWPF.Models;

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

            // Минимальные тестовые данные
            aplications.Add(new Aplication
            {
                AplicationId = 1,
                ClientName = "Тест 1",
                TourName = "Тур 1",
                Status = "Новая",
                AplicationDate = DateTime.Now.AddDays(-2)
            });

            aplications.Add(new Aplication
            {
                AplicationId = 2,
                ClientName = "Тест 2",
                TourName = "Тур 2",
                Status = "Подтверждена",
                AplicationDate = DateTime.Now.AddDays(-5)
            });

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