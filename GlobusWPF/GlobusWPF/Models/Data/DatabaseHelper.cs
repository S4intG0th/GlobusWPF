using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using GlobusTourApp.Models;

namespace GlobusTourApp.Data
{
    public class DatabaseHelper
    {
        private readonly string connectionString;

        public DatabaseHelper()
        {
            connectionString = ConfigurationManager.ConnectionStrings["GlobusConnection"].ConnectionString;
        }

        // Проверка авторизации
        public User Authenticate(string login, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Users WHERE Login = @Login AND Password = @Password";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Login", login);
                cmd.Parameters.AddWithValue("@Password", password);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new User
                    {
                        UserId = (int)reader["UserId"],
                        Role = reader["Role"].ToString(),
                        FullName = reader["FullName"].ToString(),
                        Login = reader["Login"].ToString(),
                        Password = reader["Password"].ToString()
                    };
                }
                return null;
            }
        }

        // Получение всех туров (для гостя)
        public List<Tour> GetAllTours()
        {
            List<Tour> tours = new List<Tour>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT t.*, c.CountryName, bt.BusTypeName 
                    FROM Tours t
                    JOIN Countries c ON t.CountryId = c.CountryId
                    JOIN BusTypes bt ON t.BusTypeId = bt.BusTypeId
                    WHERE t.StartDate >= GETDATE()
                    ORDER BY t.StartDate";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tours.Add(new Tour
                    {
                        TourId = (int)reader["TourId"],
                        TourName = reader["TourName"].ToString(),
                        CountryName = reader["CountryName"].ToString(),
                        DurationDays = (int)reader["DurationDays"],
                        StartDate = (DateTime)reader["StartDate"],
                        BasePrice = (decimal)reader["BasePrice"],
                        BusTypeName = reader["BusTypeName"].ToString(),
                        Capacity = (int)reader["Capacity"],
                        FreeSeats = (int)reader["FreeSeats"],
                        PhotoFileName = reader["PhotoFileName"].ToString()
                    });
                }
            }

            return tours;
        }

        // Получение всех заявок (для менеджера)
        public List<Aplication> GetAllAplicationss()
        {
            List<Aplication> aplications = new List<Aplication>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT b.*, u.FullName, t.TourName
                    FROM Bookings b
                    JOIN Users u ON b.ClientId = u.UserId
                    JOIN Tours t ON b.TourId = t.TourId
                    ORDER BY b.BookingDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    aplications.Add(new Aplication
                    {
                        AplicationId = (int)reader["AplicationId"],
                        TourId = (int)reader["TourId"],
                        ClientId = (int)reader["ClientId"],
                        ClientName = reader["FullName"].ToString(),
                        TourName = reader["TourName"].ToString(),
                        AplicationDate = (DateTime)reader["AplicationDate"],
                        Status = reader["Status"].ToString(),
                        PeopleCount = (int)reader["PeopleCount"],
                        TotalPrice = (decimal)reader["TotalPrice"],
                        Comment = reader["Comment"].ToString()
                    });
                }
            }

            return aplications;
        }

        // Обновление статуса заявки
        public bool UpdateAplicationStatus(int aplicationId, string status)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Bookings SET Status = @Status WHERE BookingId = @BookingId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@BookingId", aplicationId);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Обновление количества свободных мест
        public bool UpdateTourSeats(int tourId, int seatsChange)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    UPDATE Tours 
                    SET FreeSeats = FreeSeats + @SeatsChange
                    WHERE TourId = @TourId AND FreeSeats + @SeatsChange >= 0";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TourId", tourId);
                cmd.Parameters.AddWithValue("@SeatsChange", seatsChange);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}