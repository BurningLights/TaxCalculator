﻿using TaxCalculator.Json;
using TaxCalculator.Rest;
using TaxCalculator.Services.TaxCalculators;
using TaxCalculator.Services.TaxCalculators.TaxJar;
using TaxCalculator.Services.Taxes;
using Xamarin.Forms;

namespace TaxCalculator
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DependencyService.Register<IJsonConverter, NewtonsoftJsonConverter>();
            DependencyService.Register<IHttpRestClient, RestHttpClient>();
            DependencyService.RegisterSingleton<ITaxCalculator>(
                new TaxJarCalculator(DependencyService.Get<IHttpRestClient>(), DependencyService.Get<IJsonConverter>(), "__TEST_TOKEN__")
            );
            DependencyService.RegisterSingleton<ITaxService>(new SimpleTaxService(DependencyService.Get<ITaxCalculator>()));

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
