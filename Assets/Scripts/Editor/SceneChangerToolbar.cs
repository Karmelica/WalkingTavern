using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Editor
{
    /// <summary>
    /// Adds a scene selection dropdown to the Unity toolbar next to the Play button
    /// </summary>
    [InitializeOnLoad]
    public static class SceneChangerToolbar
    {
        private static int _selectedSceneIndex = 0;
        private static string[] _sceneNames;
        private static string[] _scenePaths;
        private static GUIStyle _dropdownStyle;

        static SceneChangerToolbar()
        {
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            EditorApplication.update -= OnUpdate;
            RefreshSceneList();

            // Subscribe to toolbar GUI
            ToolbarExtension.OnToolbarGUI += OnToolbarGUI;
            EditorBuildSettings.sceneListChanged += RefreshSceneList;
        }

        private static void OnToolbarGUI()
        {
            if (_dropdownStyle == null)
            {
                _dropdownStyle = new GUIStyle(EditorStyles.toolbarDropDown)
                {
                    fixedWidth = 150,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            GUILayout.FlexibleSpace();

            if (_sceneNames == null || _sceneNames.Length == 0)
            {
                GUI.enabled = false;
                GUILayout.Button("No scenes", EditorStyles.toolbarButton, GUILayout.Width(150));
                GUI.enabled = true;
                return;
            }

            EditorGUI.BeginChangeCheck();
            _selectedSceneIndex = EditorGUILayout.Popup(_selectedSceneIndex, _sceneNames, _dropdownStyle);

            if (EditorGUI.EndChangeCheck())
            {
                LoadSelectedScene();
            }
        }

        /// <summary>
        /// Refreshes the list of available scenes from Build Settings
        /// </summary>
        private static void RefreshSceneList()
        {
            var scenes = EditorBuildSettings.scenes;

            if (scenes.Length == 0)
            {
                _sceneNames = Array.Empty<string>();
                _scenePaths = Array.Empty<string>();
                return;
            }

            _sceneNames = new string[scenes.Length];
            _scenePaths = new string[scenes.Length];

            for (var i = 0; i < scenes.Length; i++)
            {
                _scenePaths[i] = scenes[i].path;
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
                _sceneNames[i] = sceneName;
            }

            // Update current scene selection
            var currentScene = SceneManager.GetActiveScene();
            for (var i = 0; i < _scenePaths.Length; i++)
            {
                if (_scenePaths[i] != currentScene.path) continue;
                _selectedSceneIndex = i;
                break;
            }

            // Reset selection if out of bounds
            if (_selectedSceneIndex >= _sceneNames.Length)
            {
                _selectedSceneIndex = 0;
            }
        }

        /// <summary>
        /// Loads the currently selected scene
        /// </summary>
        private static void LoadSelectedScene()
        {
            if (_scenePaths == null || _selectedSceneIndex >= _scenePaths.Length)
            {
                Debug.LogError("Invalid scene selection");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(_scenePaths[_selectedSceneIndex]);
            }
        }
    }

    /// <summary>
    /// Provides access to Unity's toolbar for custom GUI elements
    /// </summary>
    public static class ToolbarExtension
    {
        private static System.Type _toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");

        private static System.Reflection.FieldInfo _guiBackendFieldInfo = typeof(VisualElement).GetField(
            "m_IMGUIContainer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public static System.Action OnToolbarGUI;

        static ToolbarExtension()
        {
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (_toolbarType == null)
                return;

            var toolbars = Resources.FindObjectsOfTypeAll(_toolbarType);
            if (toolbars == null || toolbars.Length == 0)
                return;

            var toolbar = toolbars[0] as ScriptableObject;
            if (toolbar == null)
                return;

            var root = toolbar.GetType().GetField("m_Root",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (root == null)
                return;

            var rawRoot = root.GetValue(toolbar);
            if (rawRoot == null)
                return;

            var visualElement = rawRoot as VisualElement;
            if (visualElement == null)
                return;

            var container = visualElement.Q("ToolbarZoneRightAlign");
            if (container == null)
                return;

            var imguiContainer = container.Q<IMGUIContainer>();
            if (imguiContainer == null)
            {
                imguiContainer = new IMGUIContainer();
                imguiContainer.style.flexGrow = 1;
                imguiContainer.onGUIHandler += () => { OnToolbarGUI?.Invoke(); };
                container.Insert(6, imguiContainer);
            }

            EditorApplication.update -= OnUpdate;
        }
    }
}