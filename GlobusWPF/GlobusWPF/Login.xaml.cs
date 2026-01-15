using GlobusTourApp.Data;
using GlobusTourApp.Models;
using GlobusWPF;
using System.Windows;

namespace GlobusTourApp
{
    public partial class LoginWindow : Window
    {
        private DatabaseHelper dbHelper;

        public LoginWindow()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();

            // Обработка необработанных исключений
            App.Current.DispatcherUnhandledException += (sender, e) =>
            {
                MessageBox.Show($"Ошибка: {e.Exception.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Введите логин и пароль";
                return;
            }

            User user = dbHelper.Authenticate(login, password);

            if (user != null)
            {
                if (user.Role == "Менеджер" || user.Role == "Администратор")
                {
                    MainWindow mainWindow = new MainWindow(user);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    lblMessage.Text = "У вас нет прав для входа в эту систему";
                }
            }
            else
            {
                lblMessage.Text = "Неверный логин или пароль";
            }
        }

        private void btnGuest_Click(object sender, RoutedEventArgs e)
        {
            // Гость - пользователь без прав
            MainWindow mainWindow = new MainWindow(null);
            mainWindow.Show();
            this.Close();
        }
    }
}