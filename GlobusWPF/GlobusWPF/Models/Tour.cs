using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace GlobusWPF.Models
{
    public class Tour : INotifyPropertyChanged
    {
        private int _freeSeats;

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

        public int FreeSeats
        {
            get => _freeSeats;
            set
            {
                if (_freeSeats != value)
                {
                    _freeSeats = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FreeSeatsColor));
                }
            }
        }

        public string PhotoFileName { get; set; }
        public decimal? DiscountPrice { get; set; }

        // Вычисляемые свойства для привязки в XAML
        public bool IsSpecialOffer => DiscountPrice.HasValue &&
                                 DiscountPercent > 15;

        // Мало мест (осталось <10% от вместимости автобуса)
        public bool IsFewSeats => Capacity > 0 &&
                                  (FreeSeats / (decimal)Capacity) < 0.1m;

        // Тур скоро начнется (менее 7 дней)
        public bool IsStartingSoon => (StartDate - DateTime.Now).TotalDays < 7;

        public decimal DiscountPercent => HasDiscount ?
            ((BasePrice - DiscountPrice.Value) / BasePrice) * 100 : 0;

        public bool HasDiscount => DiscountPrice.HasValue;

        // Свойства для цветов
        public Brush FreeSeatsColor => IsFewSeats ? Brushes.Red : Brushes.Green;
        public Brush BasePriceColor => HasDiscount ? Brushes.Gray : Brushes.Black;

        // Путь к фото
        public string PhotoPath
        {
            get
            {
                if (string.IsNullOrEmpty(PhotoFileName))
                    return "/Images/no-image.png";

                // Предполагаем, что фото хранятся в папке Images
                string fullPath = $"/Images/{PhotoFileName}";
                return fullPath;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}