using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using YazlabTurizm.Views;

namespace YazlabTurizm
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new LoadingPage();
            
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
