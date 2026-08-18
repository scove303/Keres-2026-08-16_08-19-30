// ===============================================================================
// PlayFromHereTool - Professional Instant Testing Workflow
//
// Creator: Scove
// Last Updated: 2026-03-03
// Version: 3.0
//
// Purpose:
// Speeds up level testing by instantly teleporting the Player to your current
// Scene View camera position. It allows you to teleport and optionally start
// Play Mode immediately with fully customizable hotkeys.
//
// Key Features:
// 1. Custom Hotkeys: User-definable modifiers and keys stored in EditorPrefs.
// 2. Smart Detection: Automatically finds the player using Selection -> Tag -> Name.
// 3. Two Modes: "Teleport Only" for setup, and "Play From Here" for testing.
// 4. UI Feedback: Displays notifications directly in the Scene View.
//
// How to Use:
// 1. Place this script in an 'Editor' folder.
// 2. Open Settings: Menu -> Tools -> Play From Here Settings.
// 3. Assign your preferred keys (e.g., Ctrl+Alt+P for Teleport).
// 4. In the Scene View, press your shortcut to move the player.
// ===============================================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Assets.Tools
{
    [InitializeOnLoad]
    public class PlayFromHereTool : EditorWindow
    {
        // Customizable Shortcuts for Mode 1 (Teleport Only)
        private static EventModifiers teleportModifier;
        private static KeyCode teleportKey;

        // Customizable Shortcuts for Mode 2 (Teleport & Play)
        private static EventModifiers playModifier;
        private static KeyCode playKey;

        [InitializeOnLoadMethod]
        static void Init()
        {
            LoadSettings();
            // Subscribe to SceneView GUI to listen for keyboard inputs
            SceneView.duringSceneGui += OnSceneGUI;
        }

        [MenuItem("Tools/Play From Here Settings")]
        public static void ShowWindow()
        {
            GetWindow<PlayFromHereTool>("Play From Here");
        }

        private static void LoadSettings()
        {
            // Load settings from EditorPrefs, fallback to default values
            teleportModifier = (EventModifiers)
                EditorPrefs.GetInt(
                    "PFH_TeleportMod",
                    (int)(EventModifiers.Control | EventModifiers.Alt)
                );
            teleportKey = (KeyCode)EditorPrefs.GetInt("PFH_TeleportKey", (int)KeyCode.P);

            playModifier = (EventModifiers)
                EditorPrefs.GetInt(
                    "PFH_PlayMod",
                    (int)(EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift)
                );
            playKey = (KeyCode)EditorPrefs.GetInt("PFH_PlayKey", (int)KeyCode.P);
        }

        private static void SaveSettings()
        {
            EditorPrefs.SetInt("PFH_TeleportMod", (int)teleportModifier);
            EditorPrefs.SetInt("PFH_TeleportKey", (int)teleportKey);
            EditorPrefs.SetInt("PFH_PlayMod", (int)playModifier);
            EditorPrefs.SetInt("PFH_PlayKey", (int)playKey);
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingTop = 15;
            root.style.paddingBottom = 15;
            root.style.paddingLeft = 15;
            root.style.paddingRight = 15;

            Label title = new Label("Shortcut Configuration");
            title.style.fontSize = 22;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = new StyleColor(new Color(0.3f, 0.7f, 1f));
            title.style.marginBottom = 15;
            root.Add(title);

            // Warning Box for Conflicts
            VisualElement warningBox = new VisualElement();
            warningBox.style.backgroundColor = new StyleColor(new Color(0.8f, 0.2f, 0.2f, 0.3f));
            warningBox.style.borderLeftWidth = 4;
            warningBox.style.borderLeftColor = new StyleColor(new Color(0.9f, 0.3f, 0.3f));
            warningBox.style.paddingTop = 10;
            warningBox.style.paddingBottom = 10;
            warningBox.style.paddingLeft = 10;
            warningBox.style.marginBottom = 15;
            warningBox.style.display = DisplayStyle.None;

            Label warningLabel = new Label("Conflict: Mode 1 and Mode 2 have the same hotkey!");
            warningLabel.style.color = new StyleColor(new Color(0.9f, 0.5f, 0.5f));
            warningLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            warningBox.Add(warningLabel);
            root.Add(warningBox);

            System.Action checkConflicts = () =>
            {
                bool conflict = (teleportModifier == playModifier && teleportKey == playKey);
                warningBox.style.display = conflict ? DisplayStyle.Flex : DisplayStyle.None;
            };

            // Mode 1 Card
            Label mode1Title = new Label("Mode 1: Teleport Only");
            mode1Title.style.fontSize = 16;
            mode1Title.style.unityFontStyleAndWeight = FontStyle.Bold;
            mode1Title.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            mode1Title.style.marginBottom = 10;
            root.Add(mode1Title);

            VisualElement card1 = CreateCard();
            EnumFlagsField tMod = new EnumFlagsField("Modifier Keys", teleportModifier);
            tMod.RegisterValueChangedCallback(evt =>
            {
                teleportModifier = (EventModifiers)evt.newValue;
                SaveSettings();
                checkConflicts();
            });
            card1.Add(tMod);

            EnumField tKey = new EnumField("Main Key", teleportKey);
            tKey.RegisterValueChangedCallback(evt =>
            {
                teleportKey = (KeyCode)evt.newValue;
                SaveSettings();
                checkConflicts();
            });
            tKey.style.marginTop = 10;
            card1.Add(tKey);
            root.Add(card1);

            // Mode 2 Card
            Label mode2Title = new Label("Mode 2: Play From Here (Teleport + Play)");
            mode2Title.style.fontSize = 16;
            mode2Title.style.unityFontStyleAndWeight = FontStyle.Bold;
            mode2Title.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            mode2Title.style.marginTop = 15;
            mode2Title.style.marginBottom = 10;
            root.Add(mode2Title);

            VisualElement card2 = CreateCard();
            EnumFlagsField pMod = new EnumFlagsField("Modifier Keys", playModifier);
            pMod.RegisterValueChangedCallback(evt =>
            {
                playModifier = (EventModifiers)evt.newValue;
                SaveSettings();
                checkConflicts();
            });
            card2.Add(pMod);

            EnumField pKey = new EnumField("Main Key", playKey);
            pKey.RegisterValueChangedCallback(evt =>
            {
                playKey = (KeyCode)evt.newValue;
                SaveSettings();
                checkConflicts();
            });
            pKey.style.marginTop = 10;
            card2.Add(pKey);
            root.Add(card2);

            checkConflicts();

            // Reset Button
            Button resetBtn = new Button(() =>
            {
                teleportModifier = EventModifiers.Control | EventModifiers.Alt;
                teleportKey = KeyCode.P;
                playModifier = EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift;
                playKey = KeyCode.P;
                SaveSettings();

                root.Clear();
                CreateGUI(); // Rebuild
            });
            resetBtn.text = "Reset to Defaults";
            resetBtn.style.height = 40;
            resetBtn.style.marginTop = 20;
            resetBtn.style.backgroundColor = new StyleColor(new Color(0.7f, 0.3f, 0.3f));
            resetBtn.style.color = new StyleColor(Color.white);
            resetBtn.style.fontSize = 14;
            resetBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            resetBtn.style.borderTopLeftRadius = 6;
            resetBtn.style.borderTopRightRadius = 6;
            resetBtn.style.borderBottomLeftRadius = 6;
            resetBtn.style.borderBottomRightRadius = 6;
            root.Add(resetBtn);

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

        private static void OnSceneGUI(SceneView view)
        {
            Event e = Event.current;

            // Only listen for key down events
            if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
            {
                // Clean modifiers (ignore CapsLock, etc.)
                EventModifiers currentMods =
                    e.modifiers
                    & (
                        EventModifiers.Shift
                        | EventModifiers.Control
                        | EventModifiers.Alt
                        | EventModifiers.Command
                    );

                // Try Mode 2 first (stricter modifiers)
                if (e.keyCode == playKey && currentMods == playModifier)
                {
                    ExecuteTeleport(true);
                    e.Use();
                }
                // Try Mode 1
                else if (e.keyCode == teleportKey && currentMods == teleportModifier)
                {
                    ExecuteTeleport(false);
                    e.Use();
                }
            }
        }

        private static void ExecuteTeleport(bool startPlayMode)
        {
            if (EditorApplication.isPlaying)
                return;

            GameObject player = FindPlayerObject();

            if (player == null)
            {
                Debug.LogWarning(
                    "<b>[PlayFromHere]</b> Player not found! Tag your object 'Player', name it 'Player', or select it manually."
                );
                return;
            }

            if (
                SceneView.lastActiveSceneView == null
                || SceneView.lastActiveSceneView.camera == null
            )
                return;

            Camera sceneCam = SceneView.lastActiveSceneView.camera;

            // Record Undo
            Undo.RecordObject(player.transform, "Teleport Player");

            // Teleport
            player.transform.position = sceneCam.transform.position;

            // Rotate Y axis (Yaw) to match camera
            Vector3 camRot = sceneCam.transform.rotation.eulerAngles;
            player.transform.rotation = Quaternion.Euler(0, camRot.y, 0);

            // Notify
            string msg = startPlayMode ? "Teleported & Starting Play..." : "Player Teleported Here";
            SceneView.lastActiveSceneView.ShowNotification(
                new GUIContent($"{msg}\nTarget: {player.name}")
            );
            Debug.Log($"<color=#00FFFF><b>[PlayFromHere]</b></color> {msg}");

            if (startPlayMode)
            {
                EditorApplication.isPlaying = true;
            }
        }

        private static GameObject FindPlayerObject()
        {
            // 1. Check current selection (The user knows best)
            if (Selection.activeGameObject != null)
                return Selection.activeGameObject;

            // 2. Check Tag "Player"
            try
            {
                GameObject tagPlayer = GameObject.FindGameObjectWithTag("Player");
                if (tagPlayer != null)
                    return tagPlayer;
            }
            catch { }

            // 3. Check exact name "Player"
            GameObject namePlayer = GameObject.Find("Player");
            if (namePlayer != null)
                return namePlayer;

            // 4. Search for objects containing common names
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(
                FindObjectsSortMode.None
            );
            foreach (var obj in allObjects)
            {
                string n = obj.name.ToLower();
                if (n.Contains("player") || n.Contains("character") || n.Contains("controller"))
                {
                    return obj;
                }
            }

            return null;
        }
    }
}
#endif
