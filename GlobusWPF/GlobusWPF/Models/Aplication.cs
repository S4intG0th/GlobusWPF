using System;

namespace GlobusTourApp.Models
{
    public class Aplication
    {
        public int AplicationId { get; set; }
        public int TourId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string TourName { get; set; }
        public DateTime AplicationDate { get; set; }
        public string Status { get; set; }
        public int PeopleCount { get; set; }
        public decimal TotalPrice { get; set; }
        public string Comment { get; set; }
    }
}