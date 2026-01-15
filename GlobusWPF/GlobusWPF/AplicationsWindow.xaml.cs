using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GlobusTourApp.Data;
using GlobusTourApp.Models;

namespace GlobusTourApp
{
    public partial class AplicationsWindow : Window
    {
        private DatabaseHelper dbHelper;
        private User currentUser;
        private List<Aplication> allAplications;

        public AplicationsWindow(User user)
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            currentUser = user;

            LoadAplications();
        }

        private void LoadAplications()
        {
            try
            {
                allAplications = dbHelper.GetAllAplications();
                lvAplications.ItemsSource = allAplications;
                UpdateStatus($"Загружено заявок: {allAplications.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatus(string message)
        {
            // Можно добавить статусную строку
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterBookings();
        }

        private void cbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterBookings();
        }

        private void FilterBookings()
        {
            try
            {
                var filtered = allAplications.AsEnumerable();

                // Поиск
                string searchText = txtSearch.Text.ToLower();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filtered = filtered.Where(b =>
                        b.ClientName.ToLower().Contains(searchText) ||
                        b.TourName.ToLower().Contains(searchText) ||
                        b.AplicationId.ToString().Contains(searchText));
                }

                // Фильтрация по статусу
                string selectedStatus = (cbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
                if (selectedStatus != "Все")
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
            if (sender is Button button && button.Tag is int aplicationId)
            {
                var aplication = allAplications.FirstOrDefault(b => b.AplicationId == aplicationId);
                if (aplication != null)
                {
                    string details = $"Заявка №{aplication.AplicationId}\n" +
                                   $"Клиент: {aplication.ClientName}\n" +
                                   $"Тур: {aplication.TourName}\n" +
                                   $"Дата бронирования: {aplication.AplicationDate:dd.MM.yyyy}\n" +
                                   $"Статус: {aplication.Status}\n" +
                                   $"Количество человек: {aplication.PeopleCount}\n" +
                                   $"Общая стоимость: {aplication.TotalPrice:N0} руб.\n" +
                                   $"Комментарий: {aplication.Comment}";

                    MessageBox.Show(details, "Детали заявки",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ConfirmAplication_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int aplicationId)
            {
                var aplication = allAplications.FirstOrDefault(b => b.AplicationId == aplicationId);
                if (aplication != null)
                {
                    try
                    {
                        // Проверяем наличие свободных мест
                        if (dbHelper.UpdateAplicationStatus(aplicationId, "Подтвержденная"))
                        {
                            // Обновляем количество мест
                            dbHelper.UpdateTourSeats(aplication.TourId, -aplication.PeopleCount);

                            MessageBox.Show("Заявка успешно подтверждена!", "Успех",
                                          MessageBoxButton.OK, MessageBoxImage.Information);

                            // Обновляем список
                            LoadAplications();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось подтвердить заявку", "Ошибка",
                                          MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при подтверждении заявки: {ex.Message}", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnNewAplication_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функционал создания новой заявки будет реализован в следующей версии",
                          "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAplications();
        }
    }
}