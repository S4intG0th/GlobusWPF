using GlobusWPF.Data;
using GlobusWPF.Models;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GlobusWPF
{
    public partial class MainWindow : Window
    {
        private User CurrentUser;
        private ObservableCollection<Tour> allTours;
        private ObservableCollection<Tour> filteredTours;

        public MainWindow(User user)
        {
            InitializeComponent();
            CurrentUser = user;

            allTours = new ObservableCollection<Tour>();
            filteredTours = new ObservableCollection<Tour>();

            InitializeWindow();
            LoadTours();
        }

        private void InitializeWindow()
        {
            if (CurrentUser == null)
            {
                statusText.Text = "Режим: Гость";
                menuAplications.Visibility = Visibility.Collapsed;
                menuToursManagement.Visibility = Visibility.Collapsed;
            }
            else
            {
                statusText.Text = $"Пользователь: {CurrentUser.FullName} ({CurrentUser.Role})";

                // Показываем меню заявок для менеджеров и администраторов
                menuAplications.Visibility = (CurrentUser.Role == "Менеджер" || CurrentUser.Role == "Администратор")
                    ? Visibility.Visible : Visibility.Collapsed;

                // Показываем меню управления турами для менеджеров и администраторов
                menuToursManagement.Visibility = (CurrentUser.Role == "Менеджер" || CurrentUser.Role == "Администратор")
                    ? Visibility.Visible : Visibility.Collapsed;
            }
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
                statusText.Text = $"Пользователь: {CurrentUser?.FullName} ({CurrentUser?.Role}) | Загружено туров: {allTours.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки туров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            filteredTours.Clear();

            var filtered = allTours.AsEnumerable();

            if (chkSpecialOffers?.IsChecked == true)
            {
                filtered = filtered.Where(t => t.IsSpecialOffer);
            }

            if (chkFewSeats?.IsChecked == true)
            {
                filtered = filtered.Where(t => t.IsFewSeats);
            }

            if (chkStartingSoon?.IsChecked == true)
            {
                filtered = filtered.Where(t => t.IsStartingSoon);
            }

            foreach (var tour in filtered)
            {
                filteredTours.Add(tour);
            }

            icTours.ItemsSource = filteredTours;
        }

        private void FilterTours(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            chkSpecialOffers.IsChecked = false;
            chkFewSeats.IsChecked = false;
            chkStartingSoon.IsChecked = false;

            ApplyFilters();
        }

        private void BookTour_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                int tourId = Convert.ToInt32(button.Tag);
                var tour = allTours.FirstOrDefault(t => t.TourId == tourId);

                if (tour != null)
                {
                    if (tour.FreeSeats > 0)
                    {
                        MessageBox.Show($"Бронирование тура: {tour.TourName}\n" +
                                      $"Стоимость: {tour.DiscountPrice ?? tour.BasePrice:N0} руб.",
                                      "Бронирование", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("К сожалению, на этот тур нет свободных мест",
                                      "Нет мест", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void RefreshTours_Click(object sender, RoutedEventArgs e)
        {
            LoadTours();
        }

        private void MenuAplications_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null && (CurrentUser.Role == "Менеджер" || CurrentUser.Role == "Администратор"))
            {
                AplicationsWindow aplicationsWindow = new AplicationsWindow(CurrentUser);
                aplicationsWindow.Owner = this;
                aplicationsWindow.ShowDialog();
            }
        }

        private void MenuToursManagement_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser != null && (CurrentUser.Role == "Менеджер" || CurrentUser.Role == "Администратор"))
            {
                ToursWindow toursWindow = new ToursWindow();
                toursWindow.Owner = this;
                toursWindow.ShowDialog();
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }

        public class BoolToTextDecorationConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool boolValue && boolValue)
                    return TextDecorations.Strikethrough;
                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}