// Stub declarations. See ../README.md.
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MenuItem : Attribute
    {
        public MenuItem(string itemName) { }
        public MenuItem(string itemName, bool isValidateFunction) { }
        public MenuItem(string itemName, bool isValidateFunction, int priority) { }
    }

    public static class EditorApplication
    {
        public static bool ExecuteMenuItem(string menuItemPath) { return false; }
    }

    public static class Selection
    {
        public static GameObject activeGameObject { get; set; }
    }

    public class SerializedProperty
    {
        public UnityEngine.Object objectReferenceValue { get; set; }
    }

    public class SerializedObject
    {
        public SerializedObject(UnityEngine.Object obj) { }
        public SerializedProperty FindProperty(string propertyPath) { return null; }
        public void ApplyModifiedPropertiesWithoutUndo() { }
        public void ApplyModifiedProperties() { }
    }

    public static class AssetDatabase
    {
        public static T LoadAssetAtPath<T>(string assetPath) where T : UnityEngine.Object { return null; }
        public static void CreateAsset(UnityEngine.Object asset, string path) { }
        public static void SaveAssets() { }
        public static void Refresh() { }
    }

    public static class PrefabUtility
    {
        public static GameObject SaveAsPrefabAsset(GameObject root, string assetPath) { return null; }
    }

    public static class EditorUtility
    {
        public static void SetDirty(UnityEngine.Object target) { }
    }

    public class EditorBuildSettingsScene { public bool enabled { get; set; } public string path { get; set; } }

    public static class EditorBuildSettings
    {
        public static EditorBuildSettingsScene[] scenes { get; set; }
        public static bool TryGetConfigObject(string name, out UnityEngine.Object result)
        {
            result = null;
            return false;
        }
    }

    public enum BuildTarget { Android, iOS, StandaloneWindows64, StandaloneOSX }
    public enum BuildTargetGroup { Unknown, Standalone, Android, iOS }
    public enum ScriptingImplementation { Mono2x, IL2CPP }
    public enum UIOrientation { Portrait, PortraitUpsideDown, LandscapeRight, LandscapeLeft, AutoRotation }

    public enum AndroidSdkVersions
    {
        AndroidApiLevelAuto = 0,
        AndroidApiLevel22 = 22,
        AndroidApiLevel23 = 23,
        AndroidApiLevel24 = 24,
        AndroidApiLevel26 = 26
    }

    [Flags]
    public enum AndroidArchitecture { None = 0, ARMv7 = 1, ARM64 = 2, X86_64 = 4, All = 7 }

    public static class PlayerSettings
    {
        public static string productName { get; set; }
        public static UIOrientation defaultInterfaceOrientation { get; set; }

        public static void SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget target, string identifier) { }
        public static void SetScriptingBackend(UnityEditor.Build.NamedBuildTarget target, ScriptingImplementation backend) { }
        public static ScriptingImplementation GetScriptingBackend(UnityEditor.Build.NamedBuildTarget target) { return ScriptingImplementation.Mono2x; }
        public static void SetUseDefaultGraphicsAPIs(BuildTarget platform, bool automatic) { }
        public static void SetGraphicsAPIs(BuildTarget platform, GraphicsDeviceType[] apis) { }

        public static class Android
        {
            public static AndroidSdkVersions minSdkVersion { get; set; }
            public static AndroidArchitecture targetArchitectures { get; set; }
        }

        public static class iOS
        {
            public static string cameraUsageDescription { get; set; }
        }
    }
}

namespace UnityEditor.Build
{
    public struct NamedBuildTarget
    {
        public static NamedBuildTarget Android { get { return new NamedBuildTarget(); } }
        public static NamedBuildTarget iOS { get { return new NamedBuildTarget(); } }
    }
}

namespace UnityEditor.SceneManagement
{
    public class EditorSceneManager : UnityEngine.SceneManagement.SceneManager
    {
        public static bool MarkSceneDirty(UnityEngine.SceneManagement.Scene scene) { return true; }
    }
}
