using System.Windows;
using GlobusWPF.Models;

namespace GlobusWPF
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
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

            // Тестовые пользователи
            User user = null;

            if (login == "manager@globus.ru" && password == "9k3l5m")
            {
                user = new User
                {
                    UserId = 1,
                    Role = "Менеджер",
                    FullName = "Сидорова Анна Владимировна",
                    Login = login,
                    Password = password
                };
            }
            else if (login == "admin@globus.ru" && password == "7f8d2a")
            {
                user = new User
                {
                    UserId = 1,
                    Role = "Администратор",
                    FullName = "Петров Иван Сергеевич",
                    Login = login,
                    Password = password
                };
            }

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
            MainWindow mainWindow = new MainWindow(null);
            mainWindow.Show();
            this.Close();
        }
    }
}