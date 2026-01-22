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
        public int BasePrice { get; set; }
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
        public bool IsSpecialOffer => false;

        // Мало мест (осталось <10% от вместимости автобуса)
        public bool IsFewSeats => Capacity > 0 && FreeSeats > 0 &&
                                  (FreeSeats / (decimal)Capacity) < 0.1m;

        // Тур скоро начнется (менее 7 дней)
        public bool IsStartingSoon => (StartDate - DateTime.Now).TotalDays < 7;


        // Свойства для цветов
        public Brush FreeSeatsColor => IsFewSeats ? Brushes.Red : Brushes.Green;
        public Brush BasePriceColor => Brushes.Black;

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