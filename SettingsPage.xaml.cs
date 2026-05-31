using Microsoft.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;

namespace PhotoboothParty;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        
        entryCaptureTimer.Text = Preferences.Default.Get("capture_timer", 10).ToString();
        entryPreviewDuration.Text = Preferences.Default.Get("preview_duration", 5).ToString();
        entryInactivityTimeout.Text = Preferences.Default.Get("inactivity_timeout", 20).ToString();

        entryLine1Text.Text = Preferences.Default.Get("title_line1_text", "Noces d'Argent \U0001F4F8");
        entryLine1Color.Text = Preferences.Default.Get("title_line1_color", "Silver");
        entryLine1Size.Text = Preferences.Default.Get("title_line1_size", 36.0).ToString();

        entryLine2Text.Text = Preferences.Default.Get("title_line2_text", "Appuyez sur la t\u00E9l\u00E9commande pour commencer");
        entryLine2Color.Text = Preferences.Default.Get("title_line2_color", "White");
        entryLine2Size.Text = Preferences.Default.Get("title_line2_size", 18.0).ToString();

        switchKeepScreenOn.IsToggled = Preferences.Default.Get("keep_screen_on", true);

        WeakReferenceMessenger.Default.Register<ShutterPressedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                lblBluetoothTest.Text = "Signal Re\u00E7u \u2705 !";
                lblBluetoothTest.TextColor = Colors.LightGreen;
            });
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<ShutterPressedMessage>(this);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (int.TryParse(entryCaptureTimer.Text, out int captureTimer))
            Preferences.Default.Set("capture_timer", captureTimer);
            
        if (int.TryParse(entryPreviewDuration.Text, out int previewDuration))
            Preferences.Default.Set("preview_duration", previewDuration);
            
        if (int.TryParse(entryInactivityTimeout.Text, out int inactivityTimeout))
            Preferences.Default.Set("inactivity_timeout", inactivityTimeout);

        Preferences.Default.Set("title_line1_text", entryLine1Text.Text ?? "");
        Preferences.Default.Set("title_line1_color", entryLine1Color.Text ?? "Silver");
        if (double.TryParse(entryLine1Size.Text, out double size1)) Preferences.Default.Set("title_line1_size", size1);

        Preferences.Default.Set("title_line2_text", entryLine2Text.Text ?? "");
        Preferences.Default.Set("title_line2_color", entryLine2Color.Text ?? "White");
        if (double.TryParse(entryLine2Size.Text, out double size2)) Preferences.Default.Set("title_line2_size", size2);

        Preferences.Default.Set("keep_screen_on", switchKeepScreenOn.IsToggled);
        DeviceDisplay.Current.KeepScreenOn = switchKeepScreenOn.IsToggled;

        await Navigation.PopAsync();
    }
}
