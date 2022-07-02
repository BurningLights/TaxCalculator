﻿using System;
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
        private string errorMessage = "";
        private bool isPending = false;

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

        public string ErrorMessage { 
            get => errorMessage;
            set
            {
                string oldMessage = errorMessage;
                SetPropertyValue(ref errorMessage, value);
                if (string.IsNullOrEmpty(oldMessage) != string.IsNullOrEmpty(value))
                {
                    RaisePropertyChanged(nameof(HasError));
                }
            } 
        }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool IsPending
        {
            get => isPending;
            set => SetPropertyValue(ref isPending, value);
        }

        public IList<string> Countries => taxService.SupportedCountries().ToList();

        public ICommand CalculateCommand { get; private set; }

        private void ResetTaxInfo()
        {
            Taxes = null;
            OriginTaxRate = null;
            DestinationTaxRate = null;
            ErrorMessage = "";
        }

        private bool FromAddressHasRequired() => FromAddress.Country != null && FromAddress.City != null && FromAddress.Zip != null && FromAddress.StreetAddress != null;
        private bool ToAddressHasRequired() => ToAddress.Country != null && ToAddress.City != null && ToAddress.Zip != null && ToAddress.StreetAddress != null;

        public TaxPageViewModel(ITaxService taxService)
        {
            this.taxService = taxService;
            CalculateCommand = new Command(
                async () => {
                    IsPending = true;
                    (CalculateCommand as Command)?.ChangeCanExecute();

                    try
                    {
                        Task<decimal> originTaxes = taxService.GetTaxRate(FromAddress);
                        Task<decimal> destTaxes = taxService.GetTaxRate(ToAddress);
                        Task<decimal> taxToCollect = taxService.CalculateTaxes(FromAddress, ToAddress, SubTotal, Shipping);

                        try
                        {
                            OriginTaxRate = await originTaxes;
                            DestinationTaxRate = await destTaxes;
                            Taxes = await taxToCollect;
                        }
                        catch (ServiceException ex)
                        {
                            Debug.WriteLine(ex);
                            ErrorMessage = "Calculating taxes failed. Please check that your addresses are valid.";
                        }
                    }
                    catch (ServiceInputException ex)
                    {
                        ErrorMessage = ex.Message;
                    }
                    
                    IsPending = false;
                    (CalculateCommand as Command)?.ChangeCanExecute();
                },
                () => FromAddressHasRequired() && ToAddressHasRequired() && !IsPending
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
