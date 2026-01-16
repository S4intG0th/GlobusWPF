using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GlobusWPF.Models;

namespace GlobusWPF
{
    public partial class MainWindow : Window
    {
        private User currentUser;
        private ObservableCollection<Tour> tours;

        public MainWindow(User user)
        {
            InitializeComponent();
            currentUser = user;

            InitializeWindow();
            LoadTours();
        }

        private void InitializeWindow()
        {
            if (currentUser == null)
            {
                statusText.Text = "Режим: Гость";
                menuAplications.Visibility = Visibility.Collapsed;
            }
            else
            {
                statusText.Text = $"Пользователь: {currentUser.FullName} ({currentUser.Role})";
                menuAplications.Visibility = Visibility.Visible;
            }
        }

        private void LoadTours()
        {
            tours = new ObservableCollection<Tour>();

            // Тестовые данные
            tours.Add(new Tour { TourId = 1, TourName = "Италия", CountryName = "Италия", StartDate = DateTime.Now.AddDays(10), BasePrice = 85000, FreeSeats = 12, Capacity = 35 });
            tours.Add(new Tour { TourId = 2, TourName = "Франция", CountryName = "Франция", StartDate = DateTime.Now.AddDays(3), BasePrice = 92500, FreeSeats = 5, Capacity = 45 });

            lvTours.ItemsSource = tours;
        }

        private void RefreshTours_Click(object sender, RoutedEventArgs e)
        {
            LoadTours();
        }

        private void MenuAplications_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser != null && (currentUser.Role == "Менеджер" || currentUser.Role == "Администратор"))
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