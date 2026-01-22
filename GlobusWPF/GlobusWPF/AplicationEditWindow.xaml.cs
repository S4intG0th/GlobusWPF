using GlobusWPF.Data;
using GlobusWPF.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GlobusWPF
{
    public partial class AplicationEditWindow : Window
    {
        private Aplication currentAplication;
        private List<Tour> availableTours;
        private bool isEditMode;
        private int selectedTourPrice = 0;

        public AplicationEditWindow(Aplication aplication = null)
        {
            InitializeComponent();

            if (aplication == null)
            {
                // Новая заявка
                currentAplication = new Aplication
                {
                    AplicationId = 0,
                    TourId = 0,
                    ClientId = 1,
                    AplicationDate = DateTime.Now,
                    Status = "Новые",
                    PeopleCount = 1,
                    TotalPrice = 0,
                    Comment = ""
                };
                isEditMode = false;
                lblTitle.Text = "Новая заявка";
            }
            else
            {
                // Редактирование существующей заявки
                currentAplication = aplication;
                isEditMode = true;
                lblTitle.Text = $"Редактирование заявки №{aplication.AplicationId}";
                txtAplicationId.Text = aplication.AplicationId.ToString();
            }

            LoadTours();
            LoadAplicationData();
        }

        private void LoadTours()
        {
            try
            {
                availableTours = new List<Tour>();

                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    // Загружаем все туры из таблицы Tours
                    string query = @"
                        SELECT 
                            [Код тура],
                            [Наименование тура],
                            [Стоимость (руб.)],
                            [Свободных мест]
                        FROM Tours 
                        ORDER BY [Наименование тура]";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tour = new Tour
                            {
                                TourId = reader["Код тура"] != DBNull.Value ?
                                        Convert.ToInt32(reader["Код тура"]) : 0,
                                TourName = reader["Наименование тура"] != DBNull.Value ?
                                         reader["Наименование тура"].ToString() : "",
                                BasePrice = reader["Стоимость (руб.)"] != DBNull.Value ?
                                          Convert.ToInt32(reader["Стоимость (руб.)"]) : 0,
                                FreeSeats = reader["Свободных мест"] != DBNull.Value ?
                                          Convert.ToInt32(reader["Свободных мест"]) : 0
                            };
                            availableTours.Add(tour);
                        }
                    }
                }

                cbTour.ItemsSource = availableTours;

                if (availableTours.Count == 0)
                {
                    MessageBox.Show("В базе данных нет туров!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки туров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAplicationData()
        {
            try
            {
                if (isEditMode)
                {
                    // Заполняем данные из существующей заявки
                    txtClientId.Text = currentAplication.ClientId.ToString();
                    dpAplicationDate.SelectedDate = currentAplication.AplicationDate;
                    txtPeopleCount.Text = currentAplication.PeopleCount.ToString();
                    txtComment.Text = currentAplication.Comment ?? "";
                    txtTotalPrice.Text = currentAplication.TotalPrice.ToString("N0");

                    // Устанавливаем выбранный тур
                    foreach (Tour tour in cbTour.Items)
                    {
                        if (tour.TourId == currentAplication.TourId)
                        {
                            cbTour.SelectedItem = tour;
                            selectedTourPrice = tour.BasePrice;
                            break;
                        }
                    }

                    // Устанавливаем статус
                    foreach (ComboBoxItem item in cbStatus.Items)
                    {
                        if (item.Content.ToString() == currentAplication.Status)
                        {
                            cbStatus.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                {
                    // Значения по умолчанию для новой заявки
                    txtClientId.Text = "1";
                    dpAplicationDate.SelectedDate = DateTime.Now;
                    txtPeopleCount.Text = "1";
                    cbStatus.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cbTour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTour.SelectedItem is Tour selectedTour)
            {
                selectedTourPrice = selectedTour.BasePrice;
                currentAplication.TourId = selectedTour.TourId;
                currentAplication.TourName = selectedTour.TourName;

                // Пересчитываем стоимость
                UpdateTotalPrice();
            }
        }

        private void txtClientId_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtClientId_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtClientId.Text) &&
                int.TryParse(txtClientId.Text, out int clientId))
            {
                currentAplication.ClientId = clientId;
            }
        }

        private void txtPeopleCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtPeopleCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPeopleCount.Text))
            {
                if (int.TryParse(txtPeopleCount.Text, out int peopleCount))
                {
                    currentAplication.PeopleCount = peopleCount;
                    UpdateTotalPrice();
                }
            }
        }

        private void UpdateTotalPrice()
        {
            if (selectedTourPrice > 0 && currentAplication.PeopleCount > 0)
            {
                currentAplication.TotalPrice = selectedTourPrice * currentAplication.PeopleCount;
                txtTotalPrice.Text = currentAplication.TotalPrice.ToString("N0");
            }
            else
            {
                currentAplication.TotalPrice = 0;
                txtTotalPrice.Text = "0";
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. ВАЛИДАЦИЯ ПОЛЕЙ

                // Проверяем тур
                if (cbTour.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тур!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    cbTour.Focus();
                    return;
                }

                // Проверяем код клиента
                if (!int.TryParse(txtClientId.Text, out int clientId) || clientId <= 0)
                {
                    MessageBox.Show("Введите корректный код клиента (положительное число)!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtClientId.Focus();
                    return;
                }

                // Проверяем количество человек
                if (!int.TryParse(txtPeopleCount.Text, out int peopleCount) || peopleCount <= 0)
                {
                    MessageBox.Show("Введите корректное количество человек (минимум 1)!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPeopleCount.Focus();
                    return;
                }

                // Проверяем статус
                if (cbStatus.SelectedItem is not ComboBoxItem selectedStatusItem)
                {
                    MessageBox.Show("Выберите статус заявки!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 2. ОБНОВЛЯЕМ ДАННЫЕ В ОБЪЕКТЕ
                currentAplication.ClientId = clientId;
                currentAplication.AplicationDate = dpAplicationDate.SelectedDate ?? DateTime.Now;
                currentAplication.PeopleCount = peopleCount;
                currentAplication.Status = selectedStatusItem.Content.ToString();
                currentAplication.Comment = txtComment.Text.Trim();

                // Получаем выбранный тур
                Tour selectedTour = (Tour)cbTour.SelectedItem;
                currentAplication.TourId = selectedTour.TourId;
                currentAplication.TourName = selectedTour.TourName;

                // Пересчитываем стоимость
                currentAplication.TotalPrice = selectedTour.BasePrice * peopleCount;

                // 3. СОХРАНЕНИЕ В БАЗЕ ДАННЫХ
                using (SqlConnection conn = new SqlConnection(DatabaseHelper.ConnectionString))
                {
                    conn.Open();

                    if (isEditMode)
                    {
                        // ОБНОВЛЕНИЕ существующей заявки
                        string updateQuery = @"
                            UPDATE Aplications SET
                            [Код тура] = @TourId,
                            [Код клиента] = @ClientId,
                            [Дата заявки] = @AplicationDate,
                            [Статус заявки] = @Status,
                            [Количество человек] = @PeopleCount,
                            [Общая стоимость (руб.)] = @TotalPrice,
                            Комментарий = @Comment
                            WHERE [Код заявки] = @AplicationId";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@TourId", currentAplication.TourId);
                            cmd.Parameters.AddWithValue("@ClientId", currentAplication.ClientId);
                            cmd.Parameters.AddWithValue("@AplicationDate", currentAplication.AplicationDate);
                            cmd.Parameters.AddWithValue("@Status", currentAplication.Status);
                            cmd.Parameters.AddWithValue("@PeopleCount", currentAplication.PeopleCount);
                            cmd.Parameters.AddWithValue("@TotalPrice", currentAplication.TotalPrice);
                            cmd.Parameters.AddWithValue("@Comment",
                                string.IsNullOrEmpty(currentAplication.Comment) ?
                                (object)DBNull.Value : currentAplication.Comment);
                            cmd.Parameters.AddWithValue("@AplicationId", currentAplication.AplicationId);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Заявка успешно обновлена!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                DialogResult = true;
                            }
                            else
                            {
                                MessageBox.Show("Не удалось обновить заявку!", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        // СОЗДАНИЕ новой заявки
                        // Получаем следующий ID для новой заявки
                        int nextId = GetNextAplicationId(conn);

                        string insertQuery = @"
                            INSERT INTO Aplications 
                            ([Код заявки], [Код тура], [Код клиента], [Дата заявки], 
                             [Статус заявки], [Количество человек], [Общая стоимость (руб.)], Комментарий)
                            VALUES (@AplicationId, @TourId, @ClientId, @AplicationDate, 
                                    @Status, @PeopleCount, @TotalPrice, @Comment)";

                        using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@AplicationId", nextId);
                            cmd.Parameters.AddWithValue("@TourId", currentAplication.TourId);
                            cmd.Parameters.AddWithValue("@ClientId", currentAplication.ClientId);
                            cmd.Parameters.AddWithValue("@AplicationDate", currentAplication.AplicationDate);
                            cmd.Parameters.AddWithValue("@Status", currentAplication.Status);
                            cmd.Parameters.AddWithValue("@PeopleCount", currentAplication.PeopleCount);
                            cmd.Parameters.AddWithValue("@TotalPrice", currentAplication.TotalPrice);
                            cmd.Parameters.AddWithValue("@Comment",
                                string.IsNullOrEmpty(currentAplication.Comment) ?
                                (object)DBNull.Value : currentAplication.Comment);

                            cmd.ExecuteNonQuery();

                            // Сохраняем ID новой заявки
                            currentAplication.AplicationId = nextId;

                            MessageBox.Show($"Заявка успешно создана! Номер: {nextId}", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            DialogResult = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetNextAplicationId(SqlConnection conn)
        {
            // Получаем максимальный ID и прибавляем 1
            string query = "SELECT ISNULL(MAX([Код заявки]), 0) + 1 FROM Aplications";
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