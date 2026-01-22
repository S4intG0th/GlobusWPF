using System;

namespace GlobusWPF.Models
{
    public class Client
    {
        public int ClientId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Passport { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }

        public Client()
        {
            FullName = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
            Passport = string.Empty;
            Address = string.Empty;
            Notes = string.Empty;
        }
    }
}