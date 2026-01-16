using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using GlobusWPF.Models;

namespace GlobusWPF.Data
{
    public class DatabaseHelper
    {
        public static string ConnectionString =>
    @"Server=(localdb)\MSSQLLocalDB;Database=Globusbd;Integrated Security=True;";

        public static bool TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}