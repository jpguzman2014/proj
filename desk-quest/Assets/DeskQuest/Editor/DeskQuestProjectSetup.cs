using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace DeskQuest.EditorTools
{
    /// <summary>
    /// Applies the player settings an ARCore build needs, and reports on the ones that
    /// have to be ticked by hand. Nothing here is required to open the project; it exists
    /// so a first Android build does not fail on settings that are easy to miss.
    /// </summary>
    public static class DeskQuestProjectSetup
    {
        const string k_ApplicationIdentifier = "com.deskquest.prototype";

        [MenuItem("Tools/Desk Quest/Configure Player Settings (Android)", false, 20)]
        public static void ConfigureAndroid()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, k_ApplicationIdentifier);
            PlayerSettings.productName = "Desk Quest";

            // ARCore requires API 24 at minimum.
            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

            // Google Play requires a 64-bit binary, and IL2CPP is the only backend that
            // produces one.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // Pin to OpenGLES3. Vulkan support in ARCore depends on the AR Foundation
            // version and the handset; GLES3 works everywhere ARCore does.
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

            // The control pad in a later milestone is laid out for a portrait phone.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            // Shown by iOS in the camera permission prompt. Android takes its wording from
            // the manifest, which AR Foundation generates.
            PlayerSettings.iOS.cameraUsageDescription =
                "Desk Quest uses the camera to find your desk and draw the game on it. " +
                "Nothing is recorded, stored or sent anywhere.";

            AssetDatabase.SaveAssets();

            Debug.Log("[DeskQuest] Android player settings applied. Run " +
                      "Tools > Desk Quest > Validate Setup to check what is left.");
        }

        [MenuItem("Tools/Desk Quest/Validate Setup", false, 21)]
        public static void ValidateSetup()
        {
            bool allGood = true;

            allGood &= Check(PlayerSettings.Android.minSdkVersion >= AndroidSdkVersions.AndroidApiLevel24,
                "Android minimum API level is 24 or higher (required by ARCore).");

            allGood &= Check(PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP,
                "Android scripting backend is IL2CPP.");

            allGood &= Check((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) != 0,
                "ARM64 is a target architecture.");

            allGood &= Check(IsSceneInBuildSettings(),
                "An enabled scene is present in File > Build Settings.");

            allGood &= Check(IsXrLoaderAssigned(BuildTargetGroup.Android),
                "An XR plug-in provider is enabled for Android " +
                "(Project Settings > XR Plug-in Management > Android > ARCore).");

            if (allGood)
                Debug.Log("[DeskQuest] Setup looks complete. You can build to a device.");
            else
                Debug.LogWarning("[DeskQuest] Setup is incomplete. See the failures above.");
        }

        static bool Check(bool passed, string description)
        {
            if (passed)
                Debug.Log("[DeskQuest] OK: " + description);
            else
                Debug.LogWarning("[DeskQuest] Needs attention: " + description);

            return passed;
        }

        static bool IsSceneInBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Read through reflection on purpose. Enabling the provider is a checkbox in
        /// Project Settings, and reaching into the XR Management editor assembly directly
        /// would tie this file to a package version for the sake of a warning message.
        /// A false result here means "could not confirm", not necessarily "not set".
        /// </summary>
        static bool IsXrLoaderAssigned(BuildTargetGroup targetGroup)
        {
            try
            {
                Type settingsType = FindType("UnityEngine.XR.Management.XRGeneralSettings");
                Type perTargetType = FindType("UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget");

                if (settingsType == null || perTargetType == null)
                    return false;

                FieldInfo keyField = settingsType.GetField("k_SettingsKey", BindingFlags.Public | BindingFlags.Static);
                if (keyField == null)
                    return false;

                string settingsKey = (string)keyField.GetValue(null);

                UnityEngine.Object configObject;
                if (!EditorBuildSettings.TryGetConfigObject(settingsKey, out configObject) || configObject == null)
                    return false;

                MethodInfo settingsForTarget = perTargetType.GetMethod("SettingsForBuildTarget",
                    BindingFlags.Public | BindingFlags.Instance);
                if (settingsForTarget == null)
                    return false;

                object generalSettings = settingsForTarget.Invoke(configObject, new object[] { targetGroup });
                if (generalSettings == null)
                    return false;

                PropertyInfo managerProperty = settingsType.GetProperty("Manager",
                    BindingFlags.Public | BindingFlags.Instance);
                if (managerProperty == null)
                    return false;

                object manager = managerProperty.GetValue(generalSettings, null);
                if (manager == null)
                    return false;

                PropertyInfo loadersProperty = manager.GetType().GetProperty("activeLoaders",
                    BindingFlags.Public | BindingFlags.Instance);
                if (loadersProperty == null)
                    return false;

                System.Collections.ICollection loaders =
                    loadersProperty.GetValue(manager, null) as System.Collections.ICollection;

                return loaders != null && loaders.Count > 0;
            }
            catch (Exception exception)
            {
                Debug.Log("[DeskQuest] Could not read XR Plug-in Management settings: " + exception.Message);
                return false;
            }
        }

        static Type FindType(string fullName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type type = assemblies[i].GetType(fullName, false);
                if (type != null)
                    return type;
            }

            return null;
        }
    }
}
