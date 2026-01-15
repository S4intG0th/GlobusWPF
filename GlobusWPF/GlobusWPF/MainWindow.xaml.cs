using System;
using System.Windows;
using System.Windows.Controls;
using GlobusTourApp.Data;
using GlobusTourApp.Models;
using System.Collections.Generic;

namespace GlobusTourApp
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper dbHelper;
        private User currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            currentUser = user;

            InitializeWindow();
            LoadTours();
        }

        private void InitializeWindow()
        {
            if (currentUser == null)
            {
                // Гость
                statusText.Text = "Режим: Гость";
                menuBookings.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Менеджер/Администратор
                statusText.Text = $"Пользователь: {currentUser.FullName} ({currentUser.Role})";
                menuBookings.Visibility = Visibility.Visible;
            }
        }

        private void LoadTours()
        {
            try
            {
                List<Tour> tours = dbHelper.GetAllTours();

                // Добавляем путь к фото (заглушка, если нет фото)
                foreach (var tour in tours)
                {
                    if (!string.IsNullOrEmpty(tour.PhotoFileName))
                    {
                        tour.PhotoFileName = $"Images/{tour.PhotoFileName}";
                    }
                    else
                    {
                        tour.PhotoFileName = "Images/default_tour.jpg";
                    }
                }

                lvTours.ItemsSource = tours;
                statusText.Text = $"Загружено туров: {tours.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки туров: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshTours_Click(object sender, RoutedEventArgs e)
        {
            LoadTours();
        }

        private void MenuAplications_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser != null &&
               (currentUser.Role == "Менеджер" || currentUser.Role == "Администратор"))
            {
                AplicationsWindow aplicationsWindow = new AplicationsWindow(currentUser);
                aplicationsWindow.ShowDialog();
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}