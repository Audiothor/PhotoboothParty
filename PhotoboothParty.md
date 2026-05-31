# Spécifications de Production : Application PhotoboothParty .NET MAUI (Full APK & Mode Kiosque)

Ce document contient l'architecture complète, la logique métier temporelle et les procédures de déploiement en conteneur autonome (Full APK hors-store) pour une borne Photobooth événementielle "Zéro Contact".

---

## 1. Architecture du Workflow Temporel

L'intégralité de l'expérience utilisateur est pilotée à distance par une télécommande Bluetooth émulant des commandes clavier Android (Volume/Entrée). L'écran tactile n'est jamais sollicité lors de la prise de vue, mais offre une interface de paramètres riche et intuitive.

```
[Écran d'Accueil (Veille)]
│
▼ Clic Télécommande (Touche Volume / Entrée)
[Écran de Capture (Caméra Active)] ───► Démarre un Watchdog d'Inactivité (Configuration variable)
│                                      │ Si le délai s'écoule sans clic
│                                      ▼
▼ Clic Télécommande                    [Retour Automatique Accueil]
[Compte à Rebours Visuel (3s-10s)]
│
▼ Flash Visuel / Flash Matériel
[Écran d'Aperçu de la Photo]
│
├─── (Si le délai d'aperçu s'écoule) ───► [Retour Automatique à la Caméra Active]
│
└─── (Si NOUVEAU Clic Télécommande) ────► [Relance Immédiate d'une Capture]
```

---

## 2. Configuration du Projet (`.csproj`)

Pour intégrer le flux caméra natif et le bus de messages découplés MVVM, configurez vos dépendances Nuget ainsi (ciblant .NET 10.0) :

```xml
<ItemGroup>
    <PackageReference Include="CommunityToolkit.Maui" Version="14.1.1" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="CommunityToolkit.Maui.Camera" Version="6.0.1" />
</ItemGroup>
```

---

## 3. Interception Matérielle Native (Platforms/Android/)

Pour capter les impulsions de la télécommande Bluetooth sans dépendre du focus d'un composant de l'IHM, nous surchargeons l'activité principale d'Android.

### `MainActivity.cs`
```csharp
using Android.App;
using Android.Content.PM;
using Android.Views;
using CommunityToolkit.Mvvm.Messaging;

namespace PhotoboothParty;

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
```

---

## 4. Écran des Paramètres Interactifs et Visuels (`SettingsPage`)

L'application intègre un écran de paramètres sophistiqué et 100% graphique (sans aucune saisie de chiffres ou de noms de couleurs) :

- **Couleurs de titre en pastilles** : Choix instantané parmi 10 coloris modernes (Blanc, Argent, Or, Corail, Rose, Lavande, Bleu Ciel, Menthe, Orange, Cyan) présentés sous forme de bulles tactiles avec indicateur de sélection.
- **Taille de texte par curseurs** : Réglage précis de la taille de police pour les 3 lignes de texte via des curseurs de glissement (`Slider`), avec affichage dynamique de la taille (ex: `36 px`).
- **Mode de Flash Matériel** : Commande segmented tactiles (Auto, Forcé, Désactivé) pour appliquer le mode de flash matériel approprié sur la caméra physique en complément du flash écran.

---

## 5. Compilation Full APK (Ligne de commande CLI)

Pour contourner les contraintes du Google Play Store et générer un binaire autonome directement installable, exécutez la commande suivante à la racine de votre projet MAUI :

```powershell
dotnet publish -f net10.0-android -c Release /p:AndroidPackageFormat=apk /p:AndroidEnableProfileTracking=true
```

Le fichier compilé se génère à l'emplacement suivant :

```
[Chemin_Projet]\bin\Release\net10.0-android\publish\com.audiothor.PhotoboothParty-Signed.apk
```

---

## 6. Sécurisation de la borne physique (Mode Kiosque d'IHM)

Afin d'éviter qu'un invité n'ouvre par inadvertance le système d'exploitation du smartphone via les signaux clavier de la télécommande, configurez l'appareil cible ainsi :

1. Sur l'appareil Android physique, accédez à : **Paramètres** > **Sécurité** > **Paramètres avancés** > **Épinglage d'écran** (ou Clouage d'application selon le constructeur).
2. Activez l'option.
3. Transférez et installez le fichier Full APK sur le téléphone.
4. Lancez l'application **PhotoboothParty**.
5. Ouvrez le volet des applications récentes d'Android (bouton carré ou geste de balayage vers le haut).
6. Restez appuyé sur l'icône de l'application PhotoboothParty et sélectionnez **Épingler** (ou Pin).

L'appareil est désormais verrouillé exclusivement sur le Photobooth, ignorant les requêtes système externes jusqu'au déverrouillage manuel par code PIN administrateur.
