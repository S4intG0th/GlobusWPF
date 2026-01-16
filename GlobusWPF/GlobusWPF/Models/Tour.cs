using System;
using System.IO;

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
        public decimal? DiscountPrice { get; set; }

        public string PhotoFileName { get; set; }

        // Добавьте это свойство
        public string PhotoPath
        {
            get
            {
                if (string.IsNullOrEmpty(PhotoFileName))
                    return "/Images/no-image.png"; // путь к заглушке

                // Проверяем, существует ли файл
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", PhotoFileName);
                return File.Exists(fullPath) ? fullPath : "/Images/no-image.png";
            }
        }

        // Для подсветки
        public bool IsSpecialOffer => DiscountPrice.HasValue &&
                                      (BasePrice - DiscountPrice.Value) / BasePrice > 0.15m;
        public bool IsFewSeats => FreeSeats < Capacity * 0.1;
        public bool IsStartingSoon => (StartDate - DateTime.Now).TotalDays < 7;
    }
}   