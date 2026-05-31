using CommunityToolkit.Mvvm.Messaging;

namespace PhotoboothParty;

public partial class MainPage : ContentPage
{
    private bool _isVisible = false;

    public MainPage()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<ShutterPressedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (!_isVisible)
                        return;

                    lblLine2.Text = "\u23F3 Ouverture de l'appareil photo...";

                    var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                    if (status != PermissionStatus.Granted)
                    {
                        status = await Permissions.RequestAsync<Permissions.Camera>();
                    }

                    if (status == PermissionStatus.Granted)
                    {
                        if (Shell.Current != null)
                        {
                            await Shell.Current.Navigation.PushAsync(new CapturePage());
                        }
                    }
                    else
                    {
                        lblLine2.Text = "\u26A0\uFE0F Permission refus\u00E9e.";
                    }
                }
                catch (Exception ex)
                {
                    lblLine2.Text = "Erreur: " + ex.Message;
                }
            });
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isVisible = true;

        
        lblLine1.Text = Microsoft.Maui.Storage.Preferences.Default.Get("title_line1_text", "Noces d'Argent \U0001F4F8");
        try { lblLine1.TextColor = Color.Parse(Microsoft.Maui.Storage.Preferences.Default.Get("title_line1_color", "Silver")); } catch { lblLine1.TextColor = Colors.Silver; }
        lblLine1.FontSize = Microsoft.Maui.Storage.Preferences.Default.Get("title_line1_size", 36.0);

        lblLine2.Text = Microsoft.Maui.Storage.Preferences.Default.Get("title_line2_text", "Souriez et passez un bon moment !");
        try { lblLine2.TextColor = Color.Parse(Microsoft.Maui.Storage.Preferences.Default.Get("title_line2_color", "White")); } catch { lblLine2.TextColor = Colors.White; }
        lblLine2.FontSize = Microsoft.Maui.Storage.Preferences.Default.Get("title_line2_size", 18.0);

        lblLine3.Text = Microsoft.Maui.Storage.Preferences.Default.Get("title_line3_text", "");
        lblLine3.IsVisible = !string.IsNullOrWhiteSpace(lblLine3.Text);
        try { lblLine3.TextColor = Color.Parse(Microsoft.Maui.Storage.Preferences.Default.Get("title_line3_color", "White")); } catch { lblLine3.TextColor = Colors.White; }
        lblLine3.FontSize = Microsoft.Maui.Storage.Preferences.Default.Get("title_line3_size", 16.0);

        lblVersion.Text = $"PhotoboothParty v{AppInfo.Current.VersionString}";

        DeviceDisplay.Current.KeepScreenOn = Microsoft.Maui.Storage.Preferences.Default.Get("keep_screen_on", true);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _isVisible = false;
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage());
    }
}
