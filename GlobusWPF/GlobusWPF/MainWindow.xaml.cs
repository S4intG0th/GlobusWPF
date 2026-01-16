using GlobusWPF.Data;
using GlobusWPF.Models;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

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

            // ВАЖНО: Инициализируем коллекцию ДО загрузки данных
            tours = new ObservableCollection<Tour>();

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
            try
            {
                // Очищаем существующие данные
                tours.Clear();

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
                                PhotoFileName = reader["Имя файла фото"]?.ToString()
                            };  

                            tours.Add(tour);
                        }
                    }
                }

                // Устанавливаем источник данных
                lvTours.ItemsSource = tours;

                // Обновляем статус
                statusText.Text += $" | Загружено туров: {tours.Count}";
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