using System;

namespace GlobusWPF.Models
{
    public class Tour
    {
        public int TourId { get; set; }
        public string TourName { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public int DurationDays { get; set; }
        public DateTime StartDate { get; set; }
        public decimal BasePrice { get; set; }
        public int BusTypeId { get; set; }
        public string BusTypeName { get; set; }
        public int Capacity { get; set; }
        public int FreeSeats { get; set; }
        public string PhotoFileName { get; set; }
        public decimal? DiscountPrice { get; set; }

        // Для подсветки
        public bool IsSpecialOffer => DiscountPrice.HasValue &&
                                      (BasePrice - DiscountPrice.Value) / BasePrice > 0.15m;
        public bool IsFewSeats => FreeSeats < Capacity * 0.1;
        public bool IsStartingSoon => (StartDate - DateTime.Now).TotalDays < 7;
    }
}   