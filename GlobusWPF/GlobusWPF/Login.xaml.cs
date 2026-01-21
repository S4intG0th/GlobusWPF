using GlobusWPF.Data;
using GlobusWPF.Models;
using System;
using System.Data.SqlClient;
using System.Windows;

namespace GlobusWPF
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Используем ConnectionString из DatabaseHelper
                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    // Проверяем структуру таблицы - у вас может быть другая структура
                    string query = @"
                        SELECT [Роль], [ФИО], [Логин], [Пароль]
                        FROM Users 
                        WHERE Логин = @login AND Пароль = @password";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Создаем объект User
                                User user = new User
                                {
                                    Role = reader["Роль"]?.ToString() ?? "",
                                    FullName = reader["ФИО"]?.ToString() ?? "",
                                    Login = reader["Логин"]?.ToString() ?? "",
                                    Password = password // Сохраняем пароль
                                };

                                // Открываем главное окно с передачей пользователя
                                MainWindow mainWindow = new MainWindow(user);
                                mainWindow.Show();

                                this.Close();
                            }
                            else
                            {
                                // Проверяем тестовых пользователей если в БД нет
                                User testUser = CheckTestUsers(login, password);
                                if (testUser != null)
                                {
                                    MainWindow mainWindow = new MainWindow(testUser);
                                    mainWindow.Show();
                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show("Неверный логин или пароль!", "Ошибка входа",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Если ошибка БД, проверяем тестовых пользователей
                MessageBox.Show($"Ошибка подключения к базе данных. Используется тестовый режим.\n{ex.Message}",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);

                User testUser = CheckTestUsers(login, password);
                if (testUser != null)
                {
                    MainWindow mainWindow = new MainWindow(testUser);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!", "Ошибка входа",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private User CheckTestUsers(string login, string password)
        {
            // Тестовые пользователи
            if (login == "manager@globus.ru" && password == "9k3l5m")
            {
                return new User
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
                return new User
                {
                    UserId = 2,
                    Role = "Администратор",
                    FullName = "Петров Иван Сергеевич",
                    Login = login,
                    Password = password
                };
            }

            return null;
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            // Создаем гостевого пользователя
            User guestUser = new User
            {
                UserId = 0,
                Role = "Гость",
                FullName = "Гость",
                Login = "guest",
                Password = ""
            };

            MainWindow mainWindow = new MainWindow(guestUser);
            mainWindow.Show();

            this.Close();
        }
    }
}