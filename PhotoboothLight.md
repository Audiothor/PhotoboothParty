# Spécifications de Production : Application PhotoboothLight .NET MAUI (Full APK & Mode Kiosque)

Ce document contient l'architecture complète, la logique métier temporelle et les procédures de déploiement en conteneur autonome (Full APK hors-store) pour une borne Photobooth événementielle "Zéro Contact".

---

## 1. Architecture du Workflow Temporel



L'intégralité de l'expérience utilisateur est pilotée à distance par une télécommande Bluetooth émulant des commandes clavier Android (Volume/Entrée). L'écran tactile n'est jamais sollicité.


[Écran d'Accueil (Veille)]
│
▼ Clic Télécommande (Touche Volume / Entrée)
[Écran de Capture (Caméra Active)] ───► Démarre un Watchdog d'Inactivité (20s)
│                                      │ Si 20s s'écoulent sans clic
▼ Clic Télécommande                    ▼
[Compte à Rebours Visuel (3s)]        [Retour Automatique Accueil]
│
▼ Flash / Capture du flux vidéo
[Écran d'Aperçu de la Photo (5s)]
│
├─── (Si 5s écoulées sans action) ───► [Retour Automatique Accueil]
│
└─── (Si NOUVEAU Clic Télécommande) ─► [Relance Immédiate d'une Capture]


---

## 2. Configuration du Projet (`.csproj`)

Pour intégrer le flux caméra natif et le bus de messages découplés MVVM, configurez vos dépendances Nuget ainsi :

```xml
<ItemGroup>
    <PackageReference Include="CommunityToolkit.Maui" Version="9.0.0"/>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2"/>
</ItemGroup>

Note : Initialisez le toolkit dans votre MauiProgram.cs via l'extension .UseMauiCommunityToolkit().
3. Interception Matérielle Native (Platforms/Android/)

Pour capter les impulsions de la télécommande Bluetooth sans dépendre du focus d'un composant de l'IHM, nous surchargeons l'activité principale d'Android.
MainActivity.cs
C#

using Android.App;
using Android.Content.PM;
using Android.Views;
using CommunityToolkit.Mvvm.Messaging;

namespace MauiPhotobooth;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
    {
        // Intercepte les signaux générés par les télécommandes du commerce
        if (keyCode == Keycode.VolumeUp || keyCode == Keycode.VolumeDown || keyCode == Keycode.Enter)
        {
            // Transmission immédiate au bus d'événements MAUI
            WeakReferenceMessenger.Default.Send(new ShutterPressedMessage());
            
            // Consomme l'événement pour masquer la jauge de volume système d'Android
            return true; 
        }

        return base.OnKeyDown(keyCode, e);
    }
}

public class ShutterPressedMessage { }

4. Écran d'Accueil (MainPage)
MainPage.xaml
XML

<?xml version="1.0" encoding="utf-8" ?>
<ContentPage BackgroundColor="#111111" 
             x:Class="MauiPhotobooth.MainPage" 
             xmlns="[http://schemas.microsoft.com/dotnet/2021/maui](http://schemas.microsoft.com/dotnet/2021/maui)" 
             xmlns:x="[http://schemas.microsoft.com/winfx/2009/xaml](http://schemas.microsoft.com/winfx/2009/xaml)">

    <Grid HorizontalOptions="Center" RowDefinitions="Auto, Auto" VerticalOptions="Center">
        <Label FontAttributes="Bold" FontSize="36" Grid.Row="0" HorizontalOptions="Center" Text="Noces d'Argent 📸" TextColor="Silver"/>
        <Label FontSize="18" Grid.Row="1" HorizontalOptions="Center" Margin="0,20,0,0" Opacity="0.8" Text="Appuyez sur la télécommande pour commencer" TextColor="White"/>
    </Grid>
</ContentPage>

MainPage.xaml.cs
C#

using CommunityToolkit.Mvvm.Messaging;

namespace MauiPhotobooth;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<ShutterPressedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Shell.Current.CurrentPage is MainPage)
                {
                    await Navigation.PushAsync(new CapturePage());
                }
            });
        });
    }
}

5. Gestionnaire Unique de Capture et d'Aperçu (CapturePage)

Pour préserver la mémoire vive de l'appareil et garantir la stabilité de la caméra, les états de prise de vue et d'aperçu cohabitent sur la même page.
CapturePage.xaml
XML

<?xml version="1.0" encoding="utf-8" ?>
<ContentPage BackgroundColor="Black" 
             NavigationPage.HasNavigationBar="False" 
             x:Class="MauiPhotobooth.CapturePage" 
             xmlns="[http://schemas.microsoft.com/dotnet/2021/maui](http://schemas.microsoft.com/dotnet/2021/maui)" 
             xmlns:toolkit="[http://schemas.microsoft.com/dotnet/2022/maui/toolkit](http://schemas.microsoft.com/dotnet/2022/maui/toolkit)" 
             xmlns:x="[http://schemas.microsoft.com/winfx/2009/xaml](http://schemas.microsoft.com/winfx/2009/xaml)">

    <Grid>
        <Grid IsVisible="True" x:Name="gridCamera">
            <toolkit:CameraView x:Name="photoCamera" HorizontalOptions="FillAndExpand" VerticalOptions="FillAndExpand"/>
            <Label FontAttributes="Bold" FontSize="22" HorizontalOptions="Center" Margin="0,40,0,0" Text="Installez-vous et cliquez pour flasher !" TextColor="White" VerticalOptions="Top"/>
            <Label FontAttributes="Bold" FontSize="180" HorizontalOptions="Center" IsVisible="False" Text="3" TextColor="White" VerticalOptions="Center" x:Name="lblCaptureCountdown"/>
        </Grid>

        <Grid IsVisible="False" x:Name="gridPreview">
            <Image Aspect="AspectScaleToFill" HorizontalOptions="FillAndExpand" VerticalOptions="FillAndExpand" x:Name="imgPreview"/>
            <Border BackgroundColor="#A0000000" CornerRadius="12" HorizontalOptions="End" Margin="40" Padding="20,12" StrokeThickness="0" VerticalOptions="End">
                <HorizontalStackLayout Spacing="8">
                    <Label FontSize="16" Text="Retour au menu dans" TextColor="White" VerticalOptions="Center"/>
                    <Label FontAttributes="Bold" FontSize="20" Text="5s" TextColor="Silver" VerticalOptions="Center" x:Name="lblPreviewCountdown"/>
                </HorizontalStackLayout>
            </Border>
        </Grid>
    </Grid>
</ContentPage>

CapturePage.xaml.cs
C#

using CommunityToolkit.Mvvm.Messaging;
using System.Threading;

namespace MauiPhotobooth;

public partial class CapturePage : ContentPage
{
    private CancellationTokenSource _inactivityTokenSource;
    private bool _isProcessingCapture = false;
    private bool _isInPreviewMode = false;

    public CapturePage()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<ShutterPressedMessage>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await ProcessRemoteInput();
            });
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        StartInactivityCountdown(); 
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CancelInactivityCountdown();
        WeakReferenceMessenger.Default.Unregister<ShutterPressedMessage>(this);
    }

    private async Task ProcessRemoteInput()
    {
        if (_isProcessingCapture) return; 

        CancelInactivityCountdown(); 

        if (_isInPreviewMode)
        {
            // RÈGLE : Nouveau clic pendant l'aperçu de 5s = interruption et nouvelle capture immédiate
            _isInPreviewMode = false;
            gridPreview.IsVisible = false;
            gridCamera.IsVisible = true;
        }

        await ExecuteCaptureSequence();
    }

    private async Task ExecuteCaptureSequence()
    {
        _isProcessingCapture = true;
        lblCaptureCountdown.IsVisible = true;

        for (int i = 3; i > 0; i--)
        {
            lblCaptureCountdown.Text = i.ToString();
            await Task.Delay(1000);
        }
        lblCaptureCountdown.IsVisible = false;

        var imageStream = await photoCamera.CaptureImageAsync();
        _isProcessingCapture = false;

        if (imageStream != null)
        {
            imgPreview.Source = ImageSource.FromStream(() => imageStream);
            gridCamera.IsVisible = false;
            gridPreview.IsVisible = true;

            await RunPreviewPersistenceTimer();
        }
        else
        {
            StartInactivityCountdown();
        }
    }

    private async Task RunPreviewPersistenceTimer()
    {
        _isInPreviewMode = true;

        for (int i = 5; i > 0; i--)
        {
            if (!_isInPreviewMode) return;
            lblPreviewCountdown.Text = $"{i}s";
            await Task.Delay(1000);
        }

        if (_isInPreviewMode)
        {
            await Navigation.PopAsync();
        }
    }

    private void StartInactivityCountdown()
    {
        CancelInactivityCountdown();
        _inactivityTokenSource = new CancellationTokenSource();
        var token = _inactivityTokenSource.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(20000, token);
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (Shell.Current.CurrentPage is CapturePage)
                    {
                        await Navigation.PopAsync();
                    }
                });
            }
            catch (TaskCanceledException) { }
        });
    }

    private void CancelInactivityCountdown()
    {
        _inactivityTokenSource?.Cancel();
        _inactivityTokenSource?.Dispose();
        _inactivityTokenSource = null;
    }
}

6. Compilation Full APK (Ligne de commande CLI)

Pour contourner les contraintes du Google Play Store et générer un binaire autonome directement installable, exécutez la commande suivante à la racine de votre projet MAUI :
Bash

dotnet publish -f net8.0-android -c Release /p:AndroidPackageFormat=apk /p:AndroidEnableProfileTracking=true

Le fichier compilé se génère à l'emplacement suivant :

[Chemin_Projet]\bin\Release\net8.0-android\publish\com.companyname.mauiphotobooth-Signed.apk
7. Sécurisation de la borne physique (Mode Kiosque d'IHM)

Afin d'éviter qu'un invité n'ouvre par inadvertance le système d'exploitation du smartphone via les signaux clavier de la télécommande, configurez l'appareil cible ainsi :

    Sur l'appareil Android physique, accédez à : Paramètres > Sécurité > Paramètres avancés > Épinglage d'écran (ou Clouage d'application selon la surcouche).

    Activez l'option.

    Transférez et installez le fichier Full APK sur le téléphone.

    Lancez l'application Photobooth.

    Ouvrez le volet des applications récentes d'Android (bouton carré ou geste de balayage vers le haut).

    Restez appuyé sur l'icône de l'application Photobooth et sélectionnez Épingler (ou Pin).

L'appareil est désormais verrouillé exclusivement sur le Photobooth, ignorant les requêtes système externes jusqu'au déverrouillage manuel par code PIN administrateur.