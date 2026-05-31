namespace PhotoboothParty.Resources.Strings
{
    using System;
    
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class AppResources
    {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AppResources() { }

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PhotoboothParty.Resources.Strings.AppResources", typeof(AppResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture
        {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }

        public static string SettingsTitle => ResourceManager.GetString("SettingsTitle", resourceCulture);
        public static string SettingsFolderSelection => ResourceManager.GetString("SettingsFolderSelection", resourceCulture);
        public static string SettingsUseDefaultGallery => ResourceManager.GetString("SettingsUseDefaultGallery", resourceCulture);
        public static string SettingsFolder => ResourceManager.GetString("SettingsFolder", resourceCulture);
        public static string SettingsSelectFolder => ResourceManager.GetString("SettingsSelectFolder", resourceCulture);
        public static string SettingsHomeScreen => ResourceManager.GetString("SettingsHomeScreen", resourceCulture);
        public static string SettingsLine1 => ResourceManager.GetString("SettingsLine1", resourceCulture);
        public static string SettingsLine2 => ResourceManager.GetString("SettingsLine2", resourceCulture);
        public static string SettingsLine3 => ResourceManager.GetString("SettingsLine3", resourceCulture);
        public static string SettingsText => ResourceManager.GetString("SettingsText", resourceCulture);
        public static string SettingsSize => ResourceManager.GetString("SettingsSize", resourceCulture);
        public static string SettingsColor => ResourceManager.GetString("SettingsColor", resourceCulture);
        public static string SettingsCameraSound => ResourceManager.GetString("SettingsCameraSound", resourceCulture);
        public static string SettingsFlashEffect => ResourceManager.GetString("SettingsFlashEffect", resourceCulture);
        public static string SettingsKeepScreenOn => ResourceManager.GetString("SettingsKeepScreenOn", resourceCulture);
        public static string SettingsSave => ResourceManager.GetString("SettingsSave", resourceCulture);
        public static string SettingsCancel => ResourceManager.GetString("SettingsCancel", resourceCulture);
        public static string DefaultTitleLine2 => ResourceManager.GetString("DefaultTitleLine2", resourceCulture);
        public static string CaptureInstruction => ResourceManager.GetString("CaptureInstruction", resourceCulture);
        public static string NewPhotoIn => ResourceManager.GetString("NewPhotoIn", resourceCulture);
        public static string ErrorFolderNotFound => ResourceManager.GetString("ErrorFolderNotFound", resourceCulture);
        public static string ErrorCameraAccess => ResourceManager.GetString("ErrorCameraAccess", resourceCulture);
        public static string SettingsHardwareFlash => ResourceManager.GetString("SettingsHardwareFlash", resourceCulture);
        public static string SettingsLine1ColorLabel => ResourceManager.GetString("SettingsLine1ColorLabel", resourceCulture);
        public static string SettingsLine2ColorLabel => ResourceManager.GetString("SettingsLine2ColorLabel", resourceCulture);
        public static string SettingsLine3ColorLabel => ResourceManager.GetString("SettingsLine3ColorLabel", resourceCulture);
        public static string SettingsLine1SizeLabel => ResourceManager.GetString("SettingsLine1SizeLabel", resourceCulture);
        public static string SettingsLine2SizeLabel => ResourceManager.GetString("SettingsLine2SizeLabel", resourceCulture);
        public static string SettingsLine3SizeLabel => ResourceManager.GetString("SettingsLine3SizeLabel", resourceCulture);
    }
}
