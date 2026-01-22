using GlobusWPF.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GlobusWPF.ViewModels
{
    public class AplicationEditViewModel : INotifyPropertyChanged
    {
        private Aplication _currentAplication;
        private Tour _selectedTour;
        private bool _isEditMode;
        private bool _isTourSelected;

        public Aplication CurrentAplication
        {
            get => _currentAplication;
            set
            {
                _currentAplication = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowTitle));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public Tour SelectedTour
        {
            get => _selectedTour;
            set
            {
                _selectedTour = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
                IsTourSelected = value != null;
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        public bool IsTourSelected
        {
            get => _isTourSelected;
            set
            {
                _isTourSelected = value;
                OnPropertyChanged();
            }
        }

        public string WindowTitle => IsEditMode
            ? $"Редактирование заявки №{CurrentAplication.AplicationId}"
            : "Новая заявка";

        public int TotalPrice
        {
            get
            {
                if (CurrentAplication == null || SelectedTour == null)
                    return 0;

                return CurrentAplication.PeopleCount * SelectedTour.Price;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateTotalPrice()
        {
            if (CurrentAplication != null && SelectedTour != null)
            {
                CurrentAplication.TotalPrice = TotalPrice;
            }
            OnPropertyChanged(nameof(TotalPrice));
        }
    }
}