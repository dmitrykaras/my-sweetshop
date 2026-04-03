using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace my_sweetshop.Views.Contact
{
    public partial class ContactPage : ContentPage
    {
        const string Phone = "+79780945642";
        const string Email = "desert.karas@gmail.com";
        const string Site = "http://moyakonditerskaya.ru";
        public ContactPage()
        {
            InitializeComponent();
        }

        // Поддержка
        async void OnAskQuestion(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://t.me/MoyaKonditerakaya");
        }

        async void OnVkGroup(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://vk.com/id196324878");
        }

        async void OnTelegramGroup(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://t.me/MoyaKonditerakaya");
        }

        // Копирование
        async void OnCopyEmail(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(Email);
            await ShowToast("E-mail скопирован");
        }

        async void OnCopyPhone(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync("+7 978 094 56 42");
            await ShowToast("Телефон скопирован");
        }

        async void OnCopySite(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(Site);
            await ShowToast("Сайт скопирован");
        }

        async void OnCall(object sender, EventArgs e)
        {
            await Launcher.OpenAsync($"tel:{Phone}");
        }

        async void OnInstagram(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://www.instagram.com/karas.anastasiya/");
        }

        async void OnWhatsapp(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://wa.me/79780945642");
        }

        async void OnViber(object sender, EventArgs e)
        {
            var uri = new Uri("viber://chat?number=%2B79780945642");

            if (await Launcher.CanOpenAsync(uri))
            {
                await Launcher.OpenAsync(uri);
            }
            else
            {
                // fallback — Play Market
                await Launcher.OpenAsync("https://play.google.com/store/apps/details?id=com.viber.voip");
            }
        }

        async void OnVk(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://vk.com/id196324878");
        }

        async void OnTelegram(object sender, EventArgs e)
        {
            await Launcher.OpenAsync("https://t.me/MoyaKonditerakaya");
        }

    async Task ShowToast(string text)
        {
            var toast = Toast.Make(text, ToastDuration.Short);
            await toast.Show();
        }

    }
}