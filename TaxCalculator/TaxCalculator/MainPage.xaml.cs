using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxCalculator.Services.Taxes;
using TaxCalculator.ViewModels;
using Xamarin.Forms;

namespace TaxCalculator
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new TaxPageViewModel(DependencyService.Get<ITaxService>());
        }
    }
}
