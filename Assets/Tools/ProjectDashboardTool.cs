// ===============================================================================
// ProjectDashboardTool - Central Control Panel for Unity Projects
//
// Creator: Scove
// Last Updated: 2026-03-03
// Version: 2.0
//
// Purpose:
// A centralized dashboard to quickly navigate between scenes, start the game
// from the initialization (Boot) scene, and manage save data / PlayerPrefs.
//
// Key Features:
// 1. Dynamic Scene List: Automatically fetches scenes from Build Settings.
// 2. 1-Click Play: Instantly loads the Boot scene and enters Play Mode.
// 3. Data Management: Clear PlayerPrefs, delete save files, or open the Save folder.
// 4. Color-Coded UI: Prevents accidental data deletion with clear visual warnings.
//
// How to Use:
// 1. Place this script in an 'Editor' folder.
// 2. Open via: Menu -> Tools -> Project Dashboard.
// 3. Add your scenes to File -> Build Settings to see them in the list.
// ===============================================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Assets.Tools
{
    public class ProjectDashboardTool : EditorWindow
    {
        private Vector2 sceneScrollPos;

        [MenuItem("Tools/Project Dashboard")]
        public static void ShowWindow()
        {
            // Create a window with a minimum size
            ProjectDashboardTool window = GetWindow<ProjectDashboardTool>("Dashboard");
            window.minSize = new Vector2(300, 450);
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingTop = 15;
            root.style.paddingBottom = 15;
            root.style.paddingLeft = 15;
            root.style.paddingRight = 15;

            // Header
            Label title = new Label("Project Dashboard");
            title.style.fontSize = 22;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = new StyleColor(new Color(0.3f, 0.7f, 1f));
            title.style.marginBottom = 15;
            root.Add(title);

            // Quick Play Card
            VisualElement playCard = CreateCard();
            Label playTitle = new Label("QUICK PLAY");
            playTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            playTitle.style.marginBottom = 10;
            playCard.Add(playTitle);

            Button playBtn = new Button(() => PlayFromBootScene());
            playBtn.text = "▶ PLAY GAME (From Boot Scene)";
            playBtn.style.height = 40;
            playBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.3f));
            playBtn.style.color = new StyleColor(Color.white);
            playBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            playBtn.style.borderTopLeftRadius = 6;
            playBtn.style.borderTopRightRadius = 6;
            playBtn.style.borderBottomLeftRadius = 6;
            playBtn.style.borderBottomRightRadius = 6;
            playCard.Add(playBtn);

            Label helpText = new Label(
                "Automatically saves, loads the first scene in Build Settings, and presses Play."
            );
            helpText.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
            helpText.style.marginTop = 10;
            helpText.style.whiteSpace = WhiteSpace.Normal;
            playCard.Add(helpText);

            root.Add(playCard);

            // Scene Navigation
            VisualElement navCard = CreateCard();
            navCard.style.marginTop = 15;
            Label navTitle = new Label("SCENE NAVIGATION (Build Settings)");
            navTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            navTitle.style.marginBottom = 10;
            navCard.Add(navTitle);

            ScrollView scrollView = new ScrollView();
            scrollView.style.maxHeight = 200;
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 0)
            {
                Label warn = new Label(
                    "No scenes found in Build Settings! Please go to File -> Build Settings."
                );
                warn.style.color = new StyleColor(new Color(0.9f, 0.7f, 0.2f));
                warn.style.whiteSpace = WhiteSpace.Normal;
                navCard.Add(warn);
            }
            else
            {
                for (int i = 0; i < scenes.Length; i++)
                {
                    if (scenes[i].enabled)
                    {
                        string scenePath = scenes[i].path;
                        string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                        VisualElement row = new VisualElement();
                        row.style.flexDirection = FlexDirection.Row;
                        row.style.marginBottom = 5;
                        row.style.alignItems = Align.Center;

                        Label indexLbl = new Label($"[{i}]");
                        indexLbl.style.width = 30;
                        indexLbl.style.unityTextAlign = TextAnchor.MiddleLeft;

                        Button loadBtn = new Button(() => OpenScene(scenePath));
                        loadBtn.text = $"Load {sceneName}";
                        loadBtn.style.flexGrow = 1;
                        loadBtn.style.height = 25;
                        loadBtn.style.borderTopLeftRadius = 4;
                        loadBtn.style.borderTopRightRadius = 4;
                        loadBtn.style.borderBottomLeftRadius = 4;
                        loadBtn.style.borderBottomRightRadius = 4;

                        row.Add(indexLbl);
                        row.Add(loadBtn);
                        scrollView.Add(row);
                    }
                }
                navCard.Add(scrollView);
            }
            root.Add(navCard);

            // Data Management
            VisualElement dataCard = CreateCard();
            dataCard.style.marginTop = 15;
            Label dataTitle = new Label("DATA MANAGEMENT");
            dataTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            dataTitle.style.marginBottom = 10;
            dataCard.Add(dataTitle);

            Button folderBtn = new Button(() =>
                EditorUtility.RevealInFinder(Application.persistentDataPath)
            );
            folderBtn.text = "Open Save Folder (Explorer/Finder)";
            folderBtn.style.height = 30;
            folderBtn.style.borderTopLeftRadius = 4;
            folderBtn.style.borderTopRightRadius = 4;
            folderBtn.style.borderBottomLeftRadius = 4;
            folderBtn.style.borderBottomRightRadius = 4;
            dataCard.Add(folderBtn);

            Button clearBtn = new Button(() =>
            {
                if (
                    EditorUtility.DisplayDialog(
                        "Clear All Data?",
                        "Are you sure you want to delete all PlayerPrefs and JSON save files?\nThis action cannot be undone.",
                        "Yes, Delete Everything",
                        "Cancel"
                    )
                )
                {
                    ClearAllData();
                }
            });
            clearBtn.text = "⚠ Clear PlayerPrefs & Save Data";
            clearBtn.style.height = 35;
            clearBtn.style.marginTop = 10;
            clearBtn.style.backgroundColor = new StyleColor(new Color(0.7f, 0.3f, 0.3f));
            clearBtn.style.color = new StyleColor(Color.white);
            clearBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            clearBtn.style.borderTopLeftRadius = 6;
            clearBtn.style.borderTopRightRadius = 6;
            clearBtn.style.borderBottomLeftRadius = 6;
            clearBtn.style.borderBottomRightRadius = 6;
            dataCard.Add(clearBtn);

            root.Add(dataCard);

            root.Add(ScovySignature.CreateSignatureBox());
        }

        private VisualElement CreateCard()
        {
            VisualElement card = new VisualElement();
            card.style.backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f, 0.9f));
            card.style.borderTopLeftRadius = 10;
            card.style.borderTopRightRadius = 10;
            card.style.borderBottomLeftRadius = 10;
            card.style.borderBottomRightRadius = 10;
            card.style.paddingTop = 15;
            card.style.paddingBottom = 15;
            card.style.paddingLeft = 15;
            card.style.paddingRight = 15;

            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
            card.style.borderBottomColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
            card.style.borderLeftColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
            card.style.borderRightColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
            return card;
        }

        private void PlayFromBootScene()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 0)
            {
                Debug.LogError("<b>[Dashboard]</b> Cannot Play: No scenes in Build Settings.");
                return;
            }

            // Stop playing if currently playing
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            // Save current scene and load Scene 0
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(scenes[0].path);
                EditorApplication.isPlaying = true;
            }
        }

        private void OpenScene(string path)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("<b>[Dashboard]</b> Cannot load scene while in Play Mode.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path);
                Debug.Log(
                    $"<b>[Dashboard]</b> Loaded scene: {Path.GetFileNameWithoutExtension(path)}"
                );
            }
        }

        private void ClearAllData()
        {
            // 1. Clear Unity PlayerPrefs
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // 2. Clear common save files in persistentDataPath
            string persistentPath = Application.persistentDataPath;
            string[] filesToDelete = new string[]
            {
                "save_data.json",
                "player_data.dat",
                "settings.json",
            };

            int deletedCount = 0;

            foreach (string file in filesToDelete)
            {
                string filePath = Path.Combine(persistentPath, file);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    deletedCount++;
                }
            }

            // Alternative: Delete ALL .json files in the folder (Uncomment if needed)
            /*
            string[] allJsonFiles = Directory.GetFiles(persistentPath, "*.json");
            foreach (string file in allJsonFiles) { File.Delete(file); }
            */

            Debug.Log(
                $"<color=#FF5555><b>[Dashboard]</b></color> Cleared PlayerPrefs and {deletedCount} save file(s)."
            );

            // Show notification on the Editor Window
            this.ShowNotification(new GUIContent("Data Cleared Successfully!"));
        }
    }
}
#endif
