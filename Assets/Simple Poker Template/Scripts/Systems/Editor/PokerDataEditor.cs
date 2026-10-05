using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SimplePoker.Editor
{
    public class PokerDataEditor : EditorWindow
    {

        private Dictionary<string, List<SerializedObjectContainer>> groupedSerializedObjects = new Dictionary<string, List<SerializedObjectContainer>>();
        private Dictionary<string, Texture2D> arrowTextures = new Dictionary<string, Texture2D>();
        private Vector2 scrollPosition;

        private const string RESOURCES_PATH = "Assets/Simple Poker Template/Resources/Scriptable Objects/";

        [MenuItem("Window/Poker Editor/Assets Editor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(PokerDataEditor));
        }

        private struct Directory
        {
            public string Path;
            public bool ShowButtons;
        }

        private void OnEnable()
        {
            Debug.Log($"{Application.dataPath}");
            Directory[] directories = new Directory[] {
            new Directory{Path=$"{RESOURCES_PATH}PokerGameAssetData/", ShowButtons=false},
            new Directory{Path=$"{RESOURCES_PATH}PlayerData", ShowButtons=false},
            new Directory{Path=$"{RESOURCES_PATH}DeckData", ShowButtons=false},
            new Directory{Path=$"{RESOURCES_PATH}CpuData", ShowButtons=true},
            new Directory{Path=$"{RESOURCES_PATH}LevelData", ShowButtons=true},
        };

            LoadArrowTextures();

            foreach (Directory directory in directories)
            {
                FindAndLoadScriptableObjects(directory.Path, directory.ShowButtons);
            }
        }

        private void LoadArrowTextures()
        {
            arrowTextures["Up"] = ResizeTexture(Resources.Load<Texture2D>("Icons/arrow_right"), 10);
            arrowTextures["Down"] = ResizeTexture(Resources.Load<Texture2D>("Icons/arrow_down"), 10);
        }

        private Texture2D ResizeTexture(Texture2D texture, int size)
        {
            Texture2D resizedTexture = new Texture2D(size, size);
            Graphics.ConvertTexture(texture, resizedTexture);
            return resizedTexture;
        }

        private void FindAndLoadScriptableObjects(string directory, bool showButtons = true)
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { directory });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

                if (scriptableObject != null)
                {
                    SerializedObject serializedObject = new SerializedObject(scriptableObject);

                    Type objectType = scriptableObject.GetType();

                    if (!groupedSerializedObjects.ContainsKey(objectType.Name))
                    {
                        groupedSerializedObjects[objectType.Name] = new List<SerializedObjectContainer>();
                    }

                    groupedSerializedObjects[objectType.Name].Add(new SerializedObjectContainer(serializedObject, showButtons));
                }
            }
        }


        private void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var pair in groupedSerializedObjects)
            {
                string typeName = pair.Key;
                List<SerializedObjectContainer> serializedObjectContainers = pair.Value;

                GUILayout.Space(10f);
                GUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                Texture2D arrowTexture = serializedObjectContainers[0].isExpanded ? arrowTextures["Down"] : arrowTextures["Up"];
                GUIStyle buttonStyle = new GUIStyle(); 
                buttonStyle.fontSize = 15; 
                buttonStyle.fontStyle = FontStyle.Bold;
                buttonStyle.normal.textColor = Color.white;


                if (GUILayout.Button(new GUIContent(typeName, arrowTexture), buttonStyle, GUILayout.Height(30)))
                {
                    foreach (var serializedObjectContainer in serializedObjectContainers)
                    {
                        serializedObjectContainer.isExpanded = !serializedObjectContainer.isExpanded;

                    }
                }


                if (pair.Value[0].showButtons && GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    CreateNewScriptableObject(typeName);
                }


                EditorGUILayout.EndHorizontal();

                if (serializedObjectContainers.Count > 0 && serializedObjectContainers[0].isExpanded)
                {
                    List<SerializedObjectContainer> containersCopy = new List<SerializedObjectContainer>(serializedObjectContainers);

                    foreach (var serializedObjectContainer in containersCopy)
                    {
                        EditorGUILayout.BeginHorizontal("box");
                        Texture2D arrowChildTexture = serializedObjectContainer.childExpanded ? arrowTextures["Down"] : arrowTextures["Up"];

                        if (GUILayout.Button(new GUIContent(serializedObjectContainer.serializedObject.targetObject.name, arrowChildTexture), EditorStyles.boldLabel))
                        {
                            serializedObjectContainer.childExpanded = !serializedObjectContainer.childExpanded;
                        }

                        if (serializedObjectContainer.childExpanded)
                        {
                            ShoWDataGUI(serializedObjectContainer.serializedObject.targetObject.name, serializedObjectContainer.serializedObject);
                        }
                        if (serializedObjectContainer.showButtons)
                        {
                            if (GUILayout.Button("Delete", GUILayout.Width(60)))
                            {
                                DeleteScriptableObject(serializedObjectContainer);
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndVertical();
            }


            if (GUILayout.Button("Documentation"))
            {
                Application.OpenURL("https://simple-poker-template.gitbook.io/documentation");
            }

            GUILayout.EndScrollView();
        }

        private void CreateNewScriptableObject(string typeName)
        {
            string directoryPath = RESOURCES_PATH + typeName;
            if (!AssetDatabase.IsValidFolder(directoryPath))
            {
                string parentDirectory = Path.GetDirectoryName(directoryPath);
                AssetDatabase.CreateFolder(parentDirectory, Path.GetFileName(directoryPath));
            }

            ScriptableObject newScriptableObject = CreateInstance(typeName);

            string[] existingAssets = AssetDatabase.FindAssets("t:" + typeName, new[] { directoryPath });

            string newName = typeName + "_" + (existingAssets.Length + 1);

            string newPath = directoryPath + "/" + newName + ".asset";
            AssetDatabase.CreateAsset(newScriptableObject, newPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            groupedSerializedObjects[typeName].Add(new SerializedObjectContainer(new SerializedObject(newScriptableObject)));

            Selection.activeObject = newScriptableObject;
        }

        private void DeleteScriptableObject(SerializedObjectContainer serializedObjectContainer)
        {
            groupedSerializedObjects[serializedObjectContainer.serializedObject.targetObject.GetType().Name].Remove(serializedObjectContainer);

            string assetPath = AssetDatabase.GetAssetPath(serializedObjectContainer.serializedObject.targetObject);

            AssetDatabase.DeleteAsset(assetPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        private void ShoWDataGUI(string name, SerializedObject serializedObject)
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(name, EditorStyles.boldLabel);

            DrawSerializedObject(serializedObject);

            GUILayout.EndVertical();
        }

        private void DrawSerializedObject(SerializedObject serializedObject)
        {
            serializedObject.Update();
            SerializedProperty iterator = serializedObject.GetIterator();

            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceValue is Sprite)
                {
                    GUILayout.BeginHorizontal("box");

                    GUILayout.Label(iterator.displayName);

                    EditorGUILayout.PropertyField(iterator, GUIContent.none, true, GUILayout.Width(200));

                    Sprite sprite = (Sprite)iterator.objectReferenceValue;
                    Texture2D texture = sprite.texture;

                    if (texture != null)
                    {
                        GUILayout.Label(texture, GUILayout.Width(50), GUILayout.Height(50));
                    }
                    else
                    {
                        GUILayout.Label("No preview available", GUILayout.Width(120), GUILayout.Height(50));
                    }

                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal("box");

                    GUILayout.Label(iterator.displayName);
                    EditorGUILayout.PropertyField(iterator, GUIContent.none, true, GUILayout.Width(400));
                    GUILayout.EndHorizontal();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        public class SerializedObjectContainer
        {
            public SerializedObject serializedObject;
            public bool isExpanded;
            public bool childExpanded;
            public bool showButtons; 

            public SerializedObjectContainer(SerializedObject serializedObject, bool showButtons = true)
            {
                this.serializedObject = serializedObject;
                isExpanded = false;
                childExpanded = false;
                this.showButtons = showButtons;
            }
        }

    }
}