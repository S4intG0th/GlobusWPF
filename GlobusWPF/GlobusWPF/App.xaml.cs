using System.Windows;
using GlobusWPF.Data;

namespace GlobusWPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Тестируем подключение и создаем тестовые данные
            if (DatabaseHelper.TestConnection())
            {
                // Раскомментируйте, если нужно заполнить БД тестовыми данными
                // DataSeeder.SeedTestData();
            }
            else
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Проверьте подключение.",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }
    }
}