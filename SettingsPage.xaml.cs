using Microsoft.Maui.Storage;
using CommunityToolkit.Mvvm.Messaging;

namespace PhotoboothParty;

public partial class SettingsPage : ContentPage
{
    private string _tempCustomFolderPath = "";

    public SettingsPage()
    {
        InitializeComponent();
        
        entryCaptureTimer.Text = Preferences.Default.Get("capture_timer", 10).ToString();
        entryPreviewDuration.Text = Preferences.Default.Get("preview_duration", 5).ToString();
        entryInactivityTimeout.Text = Preferences.Default.Get("inactivity_timeout", 20).ToString();

        entryLine1Text.Text = Preferences.Default.Get("title_line1_text", "Noces d'Argent \U0001F4F8");
        entryLine1Color.Text = Preferences.Default.Get("title_line1_color", "Silver");
        entryLine1Size.Text = Preferences.Default.Get("title_line1_size", 36.0).ToString();

        entryLine2Text.Text = Preferences.Default.Get("title_line2_text", "Souriez et passez un bon moment !");
        entryLine2Color.Text = Preferences.Default.Get("title_line2_color", "White");
        entryLine2Size.Text = Preferences.Default.Get("title_line2_size", 18.0).ToString();

        entryLine3Text.Text = Preferences.Default.Get("title_line3_text", "");
        entryLine3Color.Text = Preferences.Default.Get("title_line3_color", "White");
        entryLine3Size.Text = Preferences.Default.Get("title_line3_size", 16.0).ToString();

        switchKeepScreenOn.IsToggled = Preferences.Default.Get("keep_screen_on", true);
        switchPlaySound.IsToggled = Preferences.Default.Get("play_shutter_sound", true);
        switchPlayFlash.IsToggled = Preferences.Default.Get("play_flash_effect", true);
        
        bool useDefaultGallery = Preferences.Default.Get("use_default_gallery", true);
        switchUseDefaultGallery.IsToggled = useDefaultGallery;
        layoutCustomFolder.IsVisible = !useDefaultGallery;
        
        _tempCustomFolderPath = Preferences.Default.Get("custom_gallery_path", "");
        lblCustomFolderPath.Text = string.IsNullOrWhiteSpace(_tempCustomFolderPath) ? "Aucun dossier s\u00E9lectionn\u00E9" : _tempCustomFolderPath;

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

    private void OnDefaultGalleryToggled(object sender, ToggledEventArgs e)
    {
        layoutCustomFolder.IsVisible = !e.Value;
    }

    private async void OnSelectFolderClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await CommunityToolkit.Maui.Storage.FolderPicker.Default.PickAsync(default);
            if (result.IsSuccessful)
            {
                _tempCustomFolderPath = result.Folder.Path;
                lblCustomFolderPath.Text = _tempCustomFolderPath;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de s\u00E9lectionner le dossier : {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
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
        Preferences.Default.Set("title_line2_color", string.IsNullOrWhiteSpace(entryLine2Color.Text) ? "White" : entryLine2Color.Text);
        if (double.TryParse(entryLine2Size.Text, out double size2))
            Preferences.Default.Set("title_line2_size", size2);

        Preferences.Default.Set("title_line3_text", entryLine3Text.Text ?? "");
        Preferences.Default.Set("title_line3_color", string.IsNullOrWhiteSpace(entryLine3Color.Text) ? "White" : entryLine3Color.Text);
        if (double.TryParse(entryLine3Size.Text, out double size3))
            Preferences.Default.Set("title_line3_size", size3);

        Preferences.Default.Set("keep_screen_on", switchKeepScreenOn.IsToggled);
        DeviceDisplay.Current.KeepScreenOn = switchKeepScreenOn.IsToggled;
        
        Preferences.Default.Set("play_shutter_sound", switchPlaySound.IsToggled);
        Preferences.Default.Set("play_flash_effect", switchPlayFlash.IsToggled);

        Preferences.Default.Set("use_default_gallery", switchUseDefaultGallery.IsToggled);
        Preferences.Default.Set("custom_gallery_path", _tempCustomFolderPath);

        await Navigation.PopAsync();
    }
}
