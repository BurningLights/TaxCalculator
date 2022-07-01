using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TaxCalculator.Services.Taxes;
using Xamarin.Forms;
using TaxCalculator.Services;
using System.Linq;
using System.Diagnostics;

namespace TaxCalculator.ViewModels
{
    internal class TaxPageViewModel : BaseViewModel
    {
        private readonly ITaxService taxService;
        private decimal shipping;
        private decimal subTotal;
        private decimal? originTaxRate;
        private decimal? destinationTaxRate;
        private decimal? taxes;
        private string statusMessage = "";

        public AddressViewModel FromAddress { get; } = new AddressViewModel();
        public AddressViewModel ToAddress { get; } = new AddressViewModel();
        public decimal Shipping
        {
            get => shipping;
            set
            {
                SetPropertyValue(ref shipping, value);
                ResetTaxInfo();
            }
        }
        public decimal SubTotal { 
            get => subTotal;
            set
            {
                SetPropertyValue(ref subTotal, value);
                ResetTaxInfo();
            }
        }
        public decimal? OriginTaxRate { get => originTaxRate; set => SetPropertyValue(ref originTaxRate, value); }
        public decimal? DestinationTaxRate { get => destinationTaxRate; set => SetPropertyValue(ref destinationTaxRate, value); }
        public decimal? Taxes { 
            get => taxes; 
            private set
            {
                bool propertyChanged = SetPropertyValue(ref taxes, value);
                if (propertyChanged)
                {
                    RaisePropertyChanged(nameof(GrandTotal));
                }
            }
        }
        public decimal? GrandTotal => taxes == null ? null : shipping + subTotal + taxes;
        public string StatusMessage { get => statusMessage; set => SetPropertyValue(ref statusMessage, value); }
        public IList<string> Countries => taxService.SupportedCountries().ToList();

        public ICommand CalculateCommand { get; private set; }

        private void ResetTaxInfo()
        {
            Taxes = null;
            OriginTaxRate = null;
            DestinationTaxRate = null;
            StatusMessage = "";
        }

        private bool FromAddressHasRequired() => FromAddress.Country != null && FromAddress.City != null && FromAddress.Zip != null && FromAddress.StreetAddress != null;
        private bool ToAddressHasRequired() => ToAddress.Country != null && ToAddress.City != null && ToAddress.Zip != null && ToAddress.StreetAddress != null;

        public TaxPageViewModel(ITaxService taxService)
        {
            this.taxService = taxService;
            CalculateCommand = new Command(
                async () => {
                    StatusMessage = "Calculating taxes...";

                    try
                    {
                        Task<decimal> originTaxes = taxService.GetTaxRate(FromAddress);
                        Task<decimal> destTaxes = taxService.GetTaxRate(ToAddress);
                        Task<decimal> taxToCollect = taxService.CalculateTaxes(FromAddress, ToAddress, SubTotal, Shipping);

                        try
                        {
                            OriginTaxRate = await originTaxes.ConfigureAwait(false);
                            DestinationTaxRate = await destTaxes.ConfigureAwait(false);
                            Taxes = await taxToCollect.ConfigureAwait(false);
                            StatusMessage = "";
                        }
                        catch (ServiceException ex)
                        {
                            Debug.WriteLine(ex);
                            StatusMessage = "Calculating taxes failed. Please try again later.";
                        }
                    }
                    catch (ServiceInputException ex)
                    {
                        StatusMessage = ex.Message;
                    }
                },
                () => FromAddressHasRequired() && ToAddressHasRequired()
            );
            FromAddress.PropertyChanged += OnAddressChanged;
            ToAddress.PropertyChanged += OnAddressChanged;
        }

        public void OnAddressChanged(object sender, PropertyChangedEventArgs e)
        {
            (CalculateCommand as Command)?.ChangeCanExecute();
            ResetTaxInfo();
        }
    }
}
