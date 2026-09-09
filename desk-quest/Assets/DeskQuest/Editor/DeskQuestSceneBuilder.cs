using System.IO;
using DeskQuest.AR;
using DeskQuest.Audio;
using DeskQuest.Core;
using DeskQuest.Detection;
using DeskQuest.UI;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

namespace DeskQuest.EditorTools
{
    /// <summary>
    /// Assembles the Desk Quest scene in the currently open scene.
    ///
    /// The AR rig itself is created through Unity's own GameObject > XR menu items rather
    /// than by hand. Those menu commands come from the installed AR Foundation version and
    /// always produce a rig wired the way that version expects, including the tracked pose
    /// driver whose configuration differs between input backends. Hand-building the rig is
    /// the single most common way an AR project ends up with a black screen or a camera
    /// that does not track.
    ///
    /// Safe to run more than once: anything already present is reused, not duplicated.
    /// </summary>
    public static class DeskQuestSceneBuilder
    {
        const string k_GeneratedFolder = "Assets/DeskQuest/Generated";
        const string k_PlanePrefabPath = k_GeneratedFolder + "/ARPlaneVisualizer.prefab";
        const string k_PlaneMaterialPath = k_GeneratedFolder + "/ARPlaneVisualizer.mat";

        static readonly string[] k_SessionMenuPaths =
        {
            "GameObject/XR/AR Session"
        };

        static readonly string[] k_OriginMenuPaths =
        {
            "GameObject/XR/XR Origin (AR)",
            "GameObject/XR/XR Origin (Mobile AR)",
            "GameObject/XR/AR Session Origin"
        };

        [MenuItem("Tools/Desk Quest/Set Up Scene", false, 0)]
        public static void SetUpScene()
        {
            ARSession session = EnsureSession();
            if (session == null)
                return;

            XROrigin origin = EnsureOrigin();
            if (origin == null)
                return;

            ARPlaneManager planeManager = EnsureComponent<ARPlaneManager>(origin.gameObject);
            ARRaycastManager raycastManager = EnsureComponent<ARRaycastManager>(origin.gameObject);
            ARAnchorManager anchorManager = EnsureComponent<ARAnchorManager>(origin.gameObject);
            ARSurfaceManager surfaceManager = EnsureComponent<ARSurfaceManager>(origin.gameObject);

            EnsurePlaneVisualizer(planeManager);

            GameObject root = GameObject.Find("Desk Quest");
            if (root == null)
                root = new GameObject("Desk Quest");

            UIManager ui = EnsureComponent<UIManager>(root);
            ARDeskPlacement placement = EnsureComponent<ARDeskPlacement>(root);
            GameAudio audio = EnsureComponent<GameAudio>(root);
            EnvironmentChangeDetector detector = EnsureComponent<EnvironmentChangeDetector>(root);
            GameplayDirector director = EnsureComponent<GameplayDirector>(root);
            ARGameManager gameManager = EnsureComponent<ARGameManager>(root);

            PlacementIndicator indicator = EnsureIndicator(root.transform);

            WirePlacement(placement, raycastManager, anchorManager, origin.Camera, indicator);
            WireDetector(detector, planeManager);
            WireDirector(director, detector, audio);
            WireGameManager(gameManager, surfaceManager, placement, ui, director, planeManager, origin.Camera);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log("[DeskQuest] Scene set up. Save the scene, add it to Build Settings, " +
                      "then run Tools > Desk Quest > Configure Player Settings (Android).");

            Selection.activeGameObject = root;
        }

        static ARSession EnsureSession()
        {
            ARSession session = Object.FindObjectOfType<ARSession>();
            if (session != null)
                return session;

            if (!TryExecuteFirstMenuItem(k_SessionMenuPaths))
            {
                Debug.LogError("[DeskQuest] Could not create an AR Session. Add one manually " +
                               "with GameObject > XR > AR Session, then run this command again.");
                return null;
            }

            return Object.FindObjectOfType<ARSession>();
        }

        static XROrigin EnsureOrigin()
        {
            XROrigin origin = Object.FindObjectOfType<XROrigin>();
            if (origin != null)
                return origin;

            if (!TryExecuteFirstMenuItem(k_OriginMenuPaths))
            {
                Debug.LogError("[DeskQuest] Could not create an XR Origin. Add one manually " +
                               "with GameObject > XR > XR Origin (AR), then run this command again.");
                return null;
            }

            origin = Object.FindObjectOfType<XROrigin>();

            if (origin != null && origin.Camera == null)
                Debug.LogError("[DeskQuest] The XR Origin has no camera assigned. " +
                               "Assign the AR Camera to the XR Origin's Camera field before building.");

            return origin;
        }

        static bool TryExecuteFirstMenuItem(string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                if (EditorApplication.ExecuteMenuItem(paths[i]))
                    return true;
            }

            return false;
        }

        static T EnsureComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component == null)
                component = target.AddComponent<T>();
            return component;
        }

        static PlacementIndicator EnsureIndicator(Transform parent)
        {
            PlacementIndicator existing = Object.FindObjectOfType<PlacementIndicator>();
            if (existing != null)
                return existing;

            GameObject indicatorObject = new GameObject("Placement Indicator");
            indicatorObject.transform.SetParent(parent, false);
            return indicatorObject.AddComponent<PlacementIndicator>();
        }

        /// <summary>
        /// Gives the plane manager something to draw. Without a plane prefab the tracker
        /// still finds surfaces but the player sees nothing, which is indistinguishable
        /// from AR not working at all.
        /// </summary>
        static void EnsurePlaneVisualizer(ARPlaneManager planeManager)
        {
            if (planeManager.planePrefab != null)
                return;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(k_PlanePrefabPath);
            if (prefab == null)
                prefab = CreatePlaneVisualizerPrefab();

            planeManager.planePrefab = prefab;
            EditorUtility.SetDirty(planeManager);
        }

        static GameObject CreatePlaneVisualizerPrefab()
        {
            Directory.CreateDirectory(k_GeneratedFolder);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(k_PlaneMaterialPath);
            if (material == null)
            {
                Shader shader = FindPlaneShader();
                if (shader == null)
                {
                    Debug.LogWarning("[DeskQuest] No suitable shader for the plane visualiser. " +
                                     "Detected planes will not be drawn.");
                    return null;
                }

                material = new Material(shader);
                material.name = "ARPlaneVisualizer";
                // Faint, so the player still sees their real desk through the overlay.
                material.color = new Color(0.45f, 0.85f, 1f, 0.28f);
                AssetDatabase.CreateAsset(material, k_PlaneMaterialPath);
            }

            GameObject temp = new GameObject("ARPlaneVisualizer",
                typeof(MeshFilter),
                typeof(MeshRenderer),
                typeof(ARPlaneMeshVisualizer));

            MeshRenderer renderer = temp.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, k_PlanePrefabPath);
            Object.DestroyImmediate(temp);

            AssetDatabase.SaveAssets();
            return prefab;
        }

        static Shader FindPlaneShader()
        {
            string[] candidates =
            {
                "Sprites/Default",
                "Universal Render Pipeline/Unlit",
                "Unlit/Color"
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                Shader shader = Shader.Find(candidates[i]);
                if (shader != null)
                    return shader;
            }

            return null;
        }

        /// <summary>
        /// The placement component keeps its dependencies private and serialized, so they
        /// are set here through SerializedObject rather than being exposed publicly just
        /// for the sake of editor wiring.
        /// </summary>
        static void WirePlacement(ARDeskPlacement placement, ARRaycastManager raycastManager,
            ARAnchorManager anchorManager, Camera camera, PlacementIndicator indicator)
        {
            SerializedObject serialized = new SerializedObject(placement);
            SetReference(serialized, "m_RaycastManager", raycastManager);
            SetReference(serialized, "m_AnchorManager", anchorManager);
            SetReference(serialized, "m_ARCamera", camera);
            SetReference(serialized, "m_Indicator", indicator);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireGameManager(ARGameManager gameManager, ARSurfaceManager surfaceManager,
            ARDeskPlacement placement, UIManager ui, GameplayDirector director,
            ARPlaneManager planeManager, Camera camera)
        {
            SerializedObject serialized = new SerializedObject(gameManager);
            SetReference(serialized, "m_SurfaceManager", surfaceManager);
            SetReference(serialized, "m_Placement", placement);
            SetReference(serialized, "m_UI", ui);
            SetReference(serialized, "m_Director", director);
            SetReference(serialized, "m_PlaneManager", planeManager);
            SetReference(serialized, "m_ARCamera", camera);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireDirector(GameplayDirector director, EnvironmentChangeDetector detector, GameAudio audio)
        {
            SerializedObject serialized = new SerializedObject(director);
            SetReference(serialized, "m_Detector", detector);
            SetReference(serialized, "m_Audio", audio);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireDetector(EnvironmentChangeDetector detector, ARPlaneManager planeManager)
        {
            SerializedObject serialized = new SerializedObject(detector);
            SetReference(serialized, "m_PlaneManager", planeManager);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetReference(SerializedObject serialized, string propertyName, Object value)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning("[DeskQuest] Could not find serialized field " + propertyName +
                                 " while setting up the scene.");
                return;
            }

            property.objectReferenceValue = value;
        }
    }
}
