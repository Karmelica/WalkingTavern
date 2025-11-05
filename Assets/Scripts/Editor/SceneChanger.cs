using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Editor window for quickly switching between available scenes
    /// </summary>
    public class SceneChanger : EditorWindow
    {
        private int _selectedSceneIndex = 0;
        private string[] _sceneNames;
        private string[] _scenePaths;
        
        [MenuItem("Tools/Scene Changer")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneChanger>("Scene Changer");
            window.minSize = new Vector2(300, 100);
            window.Show();
        }
        
        private void OnEnable()
        {
            RefreshSceneList();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Quick Scene Switcher", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (_sceneNames == null || _sceneNames.Length == 0)
            {
                EditorGUILayout.HelpBox("No scenes found in the project. Add scenes to Build Settings.", MessageType.Warning);
                
                if (GUILayout.Button("Refresh Scene List"))
                {
                    RefreshSceneList();
                }
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select Scene:", GUILayout.Width(80));
            
            var newIndex = EditorGUILayout.Popup(_selectedSceneIndex, _sceneNames);
            if (newIndex != _selectedSceneIndex)
            {
                _selectedSceneIndex = newIndex;
            }
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Load Scene", GUILayout.Height(30)))
            {
                LoadSelectedScene();
            }
            
            if (GUILayout.Button("Refresh List", GUILayout.Height(30), GUILayout.Width(100)))
            {
                RefreshSceneList();
            }
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            EditorGUILayout.HelpBox($"Total scenes in Build Settings: {_sceneNames.Length}", MessageType.Info);
        }
        
        /// <summary>
        /// Refreshes the list of available scenes from Build Settings
        /// </summary>
        private void RefreshSceneList()
        {
            var scenes = EditorBuildSettings.scenes;
            
            if (scenes.Length == 0)
            {
                _sceneNames = new string[0];
                _scenePaths = new string[0];
                return;
            }
            
            _sceneNames = new string[scenes.Length];
            _scenePaths = new string[scenes.Length];
            
            for (int i = 0; i < scenes.Length; i++)
            {
                _scenePaths[i] = scenes[i].path;
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
                _sceneNames[i] = $"{sceneName} ({(scenes[i].enabled ? "Enabled" : "Disabled")})";
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
        private void LoadSelectedScene()
        {
            if (_scenePaths == null || _selectedSceneIndex >= _scenePaths.Length)
            {
                Debug.LogError("Invalid scene selection");
                return;
            }
            
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(_scenePaths[_selectedSceneIndex]);
                Debug.Log($"Loaded scene: {_sceneNames[_selectedSceneIndex]}");
            }
        }
    }
}