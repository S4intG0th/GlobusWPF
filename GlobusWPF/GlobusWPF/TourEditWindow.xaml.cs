using GlobusWPF.Data;
using GlobusWPF.Models;
using Microsoft.Win32;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GlobusWPF
{
    public partial class TourEditWindow : Window
    {
        private Tour currentTour;
        private bool isNew;
        private string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

        public TourEditWindow(Tour tour = null)
        {
            InitializeComponent();

            // Создаем папку для изображений, если её нет
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            if (tour == null)
            {
                // Создание нового тура
                // ВРЕМЕННО ставим 0, так как реальный ID еще не известен
                // Реальный ID будет получен из БД после сохранения
                currentTour = new Tour
                {
                    TourId = 0, // ВРЕМЕННОЕ значение
                    TourName = "",
                    CountryName = "",
                    StartDate = DateTime.Now,
                    DurationDays = 1,
                    BasePrice = 1000,
                    BusTypeName = "Автобус",
                    Capacity = 20,
                    FreeSeats = 20,
                    PhotoFileName = ""
                };
                isNew = true;
                Title = "Новый тур";
            }
            else
            {
                // Редактирование существующего
                currentTour = tour;
                isNew = false;
                Title = "Редактирование тура";
            }

            LoadTourData();
        }

        private void LoadTourData()
        {
            txtTourName.Text = currentTour.TourName;
            txtCountry.Text = currentTour.CountryName;
            txtDuration.Text = currentTour.DurationDays.ToString();
            dpStartDate.SelectedDate = currentTour.StartDate;
            txtBasePrice.Text = currentTour.BasePrice.ToString("F0");
            txtCapacity.Text = currentTour.Capacity.ToString();
            txtFreeSeats.Text = currentTour.FreeSeats.ToString();
            txtPhotoFileName.Text = currentTour.PhotoFileName;


            // Устанавливаем тип автобуса
            foreach (ComboBoxItem item in cbBusType.Items)
            {
                if (item.Content.ToString() == currentTour.BusTypeName)
                {
                    cbBusType.SelectedItem = item;
                    break;
                }
            }

            // Загружаем превью фото
            LoadPreviewImage();
        }

        private void LoadPreviewImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(currentTour.PhotoFileName))
                {
                    string fullPath = Path.Combine(imagesFolder, currentTour.PhotoFileName);
                    if (File.Exists(fullPath))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fullPath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        imgPreview.Source = bitmap;
                    }
                    else
                    {
                        // Показываем заглушку
                        imgPreview.Source = new BitmapImage(
                            new Uri("pack://application:,,,/Images/no-image.png"));
                    }
                }
                else
                {
                    // Показываем заглушку если нет фото
                    imgPreview.Source = new BitmapImage(
                        new Uri("pack://application:,,,/Images/no-image.png"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                // Заглушка при ошибке
                imgPreview.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Images/no-image.png"));
            }
        }

        // Методы валидации для каждого поля
        private void txtDuration_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            NumberValidationTextBox(sender, e);
        }

        private void txtBasePrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            DecimalValidationTextBox(sender, e);
        }


        private void txtCapacity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            NumberValidationTextBox(sender, e);
        }

        private void txtFreeSeats_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            NumberValidationTextBox(sender, e);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string newText = textBox.Text + e.Text;

            // Проверяем, является ли строка числом (включая десятичную точку)
            if (decimal.TryParse(newText, out _) || newText == "" || newText == ".")
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void btnBrowsePhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*",
                Title = "Выберите изображение для тура"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string sourceFile = openFileDialog.FileName;
                    string fileName = Path.GetFileName(sourceFile);
                    string destFile = Path.Combine(imagesFolder, fileName);

                    // Копируем файл в папку Images
                    File.Copy(sourceFile, destFile, true);

                    txtPhotoFileName.Text = fileName;
                    currentTour.PhotoFileName = fileName;

                    // Обновляем превью
                    LoadPreviewImage();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void txtBasePrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Пустой метод - ничего не делаем
            // Можно оставить комментарий или удалить вовсе
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация (оставляем как есть)
                if (string.IsNullOrWhiteSpace(txtTourName.Text))
                {
                    MessageBox.Show("Введите название тура", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCountry.Text))
                {
                    MessageBox.Show("Введите страну", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtDuration.Text, out int duration) || duration < 1)
                {
                    MessageBox.Show("Введите корректную продолжительность (минимум 1 день)", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dpStartDate.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату начала", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtBasePrice.Text, out int basePrice) || basePrice <= 0)
                {
                    MessageBox.Show("Введите корректную базовую цену", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtCapacity.Text, out int capacity) || capacity < 1)
                {
                    MessageBox.Show("Введите корректную вместимость", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtFreeSeats.Text, out int freeSeats) || freeSeats < 0)
                {
                    MessageBox.Show("Введите корректное количество свободных мест", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (freeSeats > capacity)
                {
                    MessageBox.Show("Количество свободных мест не может превышать вместимость", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем тип автобуса
                string busType = (cbBusType.SelectedItem as ComboBoxItem)?.Content?.ToString()
                                 ?? currentTour.BusTypeName;

                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    if (isNew)
                    {
                        // Вручную получаем следующий ID
                        int nextId = GetNextTourId(conn);

                        // Теперь указываем ID явно в INSERT
                        string query = @"
            INSERT INTO Tours 
            ([Код тура], [Наименование тура], Страна, [Продолжительность (дней)], 
             [Дата начала], [Стоимость (руб.)], [Тип автобуса], 
             Вместимость, [Свободных мест], [Имя файла фото])
            VALUES (@TourId, @TourName, @Country, @Duration, @StartDate, 
                    @BasePrice, @BusType, @Capacity, @FreeSeats, 
                    @PhotoFileName)";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@TourId", nextId);
                            cmd.Parameters.AddWithValue("@TourName", txtTourName.Text);
                            cmd.Parameters.AddWithValue("@Country", txtCountry.Text);
                            cmd.Parameters.AddWithValue("@Duration", duration);
                            cmd.Parameters.AddWithValue("@StartDate", dpStartDate.SelectedDate);
                            cmd.Parameters.AddWithValue("@BasePrice", basePrice);
                            cmd.Parameters.AddWithValue("@BusType", busType);
                            cmd.Parameters.AddWithValue("@Capacity", capacity);
                            cmd.Parameters.AddWithValue("@FreeSeats", freeSeats);
                            cmd.Parameters.AddWithValue("@PhotoFileName", txtPhotoFileName.Text ?? "");

                            cmd.ExecuteNonQuery();
                            currentTour.TourId = nextId;
                        }
                    }
                    else
                    {
                        // Обновление существующей записи
                        string query = @"
                UPDATE Tours SET
                [Наименование тура] = @TourName,
                Страна = @Country,
                [Продолжительность (дней)] = @Duration,
                [Дата начала] = @StartDate,
                [Стоимость (руб.)] = @BasePrice,
                [Тип автобуса] = @BusType,
                Вместимость = @Capacity,
                [Свободных мест] = @FreeSeats,
                [Имя файла фото] = @PhotoFileName
                WHERE [Код тура] = @TourId";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@TourId", currentTour.TourId);
                            cmd.Parameters.AddWithValue("@TourName", txtTourName.Text);
                            cmd.Parameters.AddWithValue("@Country", txtCountry.Text);
                            cmd.Parameters.AddWithValue("@Duration", duration);
                            cmd.Parameters.AddWithValue("@StartDate", dpStartDate.SelectedDate);
                            cmd.Parameters.AddWithValue("@BasePrice", basePrice);
                            cmd.Parameters.AddWithValue("@BusType", busType);
                            cmd.Parameters.AddWithValue("@Capacity", capacity);
                            cmd.Parameters.AddWithValue("@FreeSeats", freeSeats);
                            cmd.Parameters.AddWithValue("@PhotoFileName", txtPhotoFileName.Text ?? "");

                            cmd.ExecuteNonQuery();
                        }
                    }
                }


                // Обновляем все поля currentTour
                currentTour.TourName = txtTourName.Text;
                currentTour.CountryName = txtCountry.Text;
                currentTour.DurationDays = duration;
                currentTour.StartDate = dpStartDate.SelectedDate.Value;
                currentTour.BasePrice = basePrice;
                currentTour.BusTypeName = busType;
                currentTour.Capacity = capacity;
                currentTour.FreeSeats = freeSeats;
                currentTour.PhotoFileName = txtPhotoFileName.Text;

                MessageBox.Show($"Тур успешно сохранен! ID тура: {currentTour.TourId}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private int GetNextTourId(SqlConnection conn)
        {
            string query = "SELECT ISNULL(MAX([Код тура]), 0) + 1 FROM Tours";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}