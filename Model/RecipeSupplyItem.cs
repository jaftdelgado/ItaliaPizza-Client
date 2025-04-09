using ItaliaPizzaClient.ItaliaPizzaServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItaliaPizzaClient.Model
{
    public class RecipeSupplyItem : INotifyPropertyChanged
    {
        private double _quantity;
        public int Id => Supply?.Id ?? 0;
        public SupplyDTO Supply { get; set; }

        public double Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
