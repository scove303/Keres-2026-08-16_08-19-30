// ===============================================================================
// CameraBookmarksTool - Configurable Scene View Camera Save & Load
//
// Creator: Scove
// Last Updated: 2024-05-08
// Version: 3.0
//
// Purpose:
// Allows saving and loading specific Scene View camera angles and positions.
// Includes an Editor Window interface to completely customize the keyboard shortcuts.
//
// How to Use:
// 1. Place this script in an 'Editor' folder in your project.
// 2. Open Settings: Menu -> Tools -> Camera Bookmarks Settings.
// 3. Customize your Save/Load modifiers (e.g., Control, Shift, Alt).
// 4. Customize the shortcut keys for Slot 1 to 9.
// 5. Focus the Scene View and use your assigned shortcuts to Save/Load camera angles.
// ===============================================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Globalization;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Assets.Tools
{
    [InitializeOnLoad]
    public class CameraBookmarksTool : EditorWindow
    {
        // Customizable Shortcut Keys
        private static EventModifiers saveModifier;
        private static EventModifiers loadModifier;
        private static KeyCode[] slotKeys = new KeyCode[9];

        // Replaced static constructor with InitializeOnLoadMethod to avoid EditorPrefs exceptions
        [InitializeOnLoadMethod]
        static void Init()
        {
            LoadSettings();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        [MenuItem("Tools/Camera Bookmarks Settings")]
        public static void ShowWindow()
        {
            GetWindow<CameraBookmarksTool>("Cam Bookmarks");
        }

        private static void LoadSettings()
        {
            // Load settings from EditorPrefs, fallback to Control/Shift if not found
            saveModifier = (EventModifiers)
                EditorPrefs.GetInt("CamBM_SaveMod", (int)EventModifiers.Control);
            loadModifier = (EventModifiers)
                EditorPrefs.GetInt("CamBM_LoadMod", (int)EventModifiers.Shift);

            // Load key bindings for 9 slots, fallback to Alpha1-Alpha9
            for (int i = 0; i < 9; i++)
            {
                int defaultKey = (int)(KeyCode.Alpha1 + i);
                slotKeys[i] = (KeyCode)EditorPrefs.GetInt($"CamBM_Key_{i}", defaultKey);
            }
        }

        private static void SaveSettings()
        {
            EditorPrefs.SetInt("CamBM_SaveMod", (int)saveModifier);
            EditorPrefs.SetInt("CamBM_LoadMod", (int)loadModifier);

            for (int i = 0; i < 9; i++)
            {
                EditorPrefs.SetInt($"CamBM_Key_{i}", (int)slotKeys[i]);
            }
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingTop = 15;
            root.style.paddingBottom = 15;
            root.style.paddingLeft = 15;
            root.style.paddingRight = 15;

            Label title = new Label("Shortcut Configuration");
            title.style.fontSize = 20;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = new StyleColor(new Color(0.3f, 0.7f, 1f));
            title.style.marginBottom = 15;
            root.Add(title);

            // Warning Box
            VisualElement warningBox = new VisualElement();
            warningBox.style.backgroundColor = new StyleColor(new Color(0.8f, 0.6f, 0.1f, 0.3f));
            warningBox.style.borderLeftWidth = 4;
            warningBox.style.borderLeftColor = new StyleColor(new Color(0.9f, 0.7f, 0.1f));
            warningBox.style.paddingTop = 10;
            warningBox.style.paddingBottom = 10;
            warningBox.style.paddingLeft = 10;
            warningBox.style.marginBottom = 15;
            warningBox.style.display = DisplayStyle.None;

            Label warningLabel = new Label(
                "Warning: Save and Load modifiers are the SAME! This will cause conflicts."
            );
            warningLabel.style.color = new StyleColor(new Color(0.9f, 0.8f, 0.5f));
            warningLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            warningBox.Add(warningLabel);
            root.Add(warningBox);

            // Modifiers Card
            VisualElement modifiersCard = CreateCard();
            EnumFlagsField saveField = new EnumFlagsField("Save Modifier", saveModifier);
            saveField.RegisterValueChangedCallback(evt =>
            {
                saveModifier = (EventModifiers)evt.newValue;
                SaveSettings();
                warningBox.style.display =
                    (saveModifier == loadModifier) ? DisplayStyle.Flex : DisplayStyle.None;
            });
            modifiersCard.Add(saveField);

            EnumFlagsField loadField = new EnumFlagsField("Load Modifier", loadModifier);
            loadField.RegisterValueChangedCallback(evt =>
            {
                loadModifier = (EventModifiers)evt.newValue;
                SaveSettings();
                warningBox.style.display =
                    (saveModifier == loadModifier) ? DisplayStyle.Flex : DisplayStyle.None;
            });
            loadField.style.marginTop = 10;
            modifiersCard.Add(loadField);

            warningBox.style.display =
                (saveModifier == loadModifier) ? DisplayStyle.Flex : DisplayStyle.None;
            root.Add(modifiersCard);

            // Slots Section
            Label slotsTitle = new Label("Slot Keys Assignment");
            slotsTitle.style.fontSize = 16;
            slotsTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            slotsTitle.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            slotsTitle.style.marginTop = 15;
            slotsTitle.style.marginBottom = 10;
            root.Add(slotsTitle);

            VisualElement slotsCard = CreateCard();
            for (int i = 0; i < 9; i++)
            {
                int index = i;
                EnumField slotField = new EnumField($"Slot {i + 1}", slotKeys[i]);
                slotField.RegisterValueChangedCallback(evt =>
                {
                    slotKeys[index] = (KeyCode)evt.newValue;
                    SaveSettings();
                });
                if (i > 0)
                    slotField.style.marginTop = 8;
                slotsCard.Add(slotField);
            }
            root.Add(slotsCard);

            // Reset Button
            Button resetBtn = new Button(() =>
            {
                saveModifier = EventModifiers.Control;
                loadModifier = EventModifiers.Shift;
                for (int i = 0; i < 9; i++)
                    slotKeys[i] = KeyCode.Alpha1 + i;
                SaveSettings();

                root.Clear();
                CreateGUI(); // Rebuild
            });
            resetBtn.text = "Reset to Default Settings";
            resetBtn.style.height = 40;
            resetBtn.style.marginTop = 20;
            resetBtn.style.backgroundColor = new StyleColor(new Color(0.7f, 0.3f, 0.3f));
            resetBtn.style.color = new StyleColor(Color.white);
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
            card.style.borderTopLeftRadius = 8;
            card.style.borderTopRightRadius = 8;
            card.style.borderBottomLeftRadius = 8;
            card.style.borderBottomRightRadius = 8;
            card.style.paddingTop = 15;
            card.style.paddingBottom = 15;
            card.style.paddingLeft = 15;
            card.style.paddingRight = 15;

            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
            card.style.borderBottomColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
            card.style.borderLeftColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
            card.style.borderRightColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
            return card;
        }

        static void OnSceneGUI(SceneView view)
        {
            Event e = Event.current;

            if (e.type == EventType.KeyDown)
            {
                // Mask out CapsLock, NumLock, and Function keys. We only care about main modifiers.
                EventModifiers currentMods =
                    e.modifiers
                    & (
                        EventModifiers.Shift
                        | EventModifiers.Control
                        | EventModifiers.Alt
                        | EventModifiers.Command
                    );

                // Check if the pressed key matches any of our custom assigned slot keys
                int pressedSlotIndex = -1;
                for (int i = 0; i < 9; i++)
                {
                    if (e.keyCode == slotKeys[i] && e.keyCode != KeyCode.None)
                    {
                        pressedSlotIndex = i + 1;
                        break;
                    }
                }

                if (pressedSlotIndex != -1)
                {
                    string prefsKey = $"CamBookmark_Slot_{pressedSlotIndex}";

                    // SAVE ACTION
                    if (currentMods == saveModifier)
                    {
                        SaveBookmark(view, prefsKey, pressedSlotIndex);
                        e.Use(); // Consume the event
                    }
                    // LOAD ACTION
                    else if (currentMods == loadModifier)
                    {
                        LoadBookmark(view, prefsKey, pressedSlotIndex);
                        e.Use(); // Consume the event
                    }
                }
            }
        }

        private static void SaveBookmark(SceneView view, string key, int slotIndex)
        {
            Transform camTransform = view.camera.transform;
            float size = view.size;

            // Use InvariantCulture to ensure dot (.) is used for decimals, preventing regional bugs
            string data = string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                camTransform.position.x,
                camTransform.position.y,
                camTransform.position.z,
                camTransform.rotation.x,
                camTransform.rotation.y,
                camTransform.rotation.z,
                camTransform.rotation.w,
                size
            );

            EditorPrefs.SetString(key, data);

            // Show feedback in Scene View
            string message = $"Saved Camera Bookmark to Slot {slotIndex}";
            view.ShowNotification(new GUIContent(message));
            Debug.Log($"<color=#00FF00><b>[CameraBookmarks]</b></color> {message}");
        }

        private static void LoadBookmark(SceneView view, string key, int slotIndex)
        {
            if (!EditorPrefs.HasKey(key))
            {
                string emptyMsg = $"Slot {slotIndex} is empty!";
                view.ShowNotification(new GUIContent(emptyMsg));
                Debug.LogWarning($"<b>[CameraBookmarks]</b> {emptyMsg}");
                return;
            }

            string[] parts = EditorPrefs.GetString(key).Split('|');

            if (parts.Length == 8)
            {
                try
                {
                    // Parse data back to floats using InvariantCulture
                    Vector3 pos = new Vector3(
                        float.Parse(parts[0], CultureInfo.InvariantCulture),
                        float.Parse(parts[1], CultureInfo.InvariantCulture),
                        float.Parse(parts[2], CultureInfo.InvariantCulture)
                    );

                    Quaternion rot = new Quaternion(
                        float.Parse(parts[3], CultureInfo.InvariantCulture),
                        float.Parse(parts[4], CultureInfo.InvariantCulture),
                        float.Parse(parts[5], CultureInfo.InvariantCulture),
                        float.Parse(parts[6], CultureInfo.InvariantCulture)
                    );

                    float size = float.Parse(parts[7], CultureInfo.InvariantCulture);

                    // Apply the saved transform to the Scene View camera
                    view.LookAtDirect(pos, rot, size);

                    // Show feedback in Scene View
                    string message = $"Loaded Camera Bookmark from Slot {slotIndex}";
                    view.ShowNotification(new GUIContent(message));
                    Debug.Log($"<color=#00FFFF><b>[CameraBookmarks]</b></color> {message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"<b>[CameraBookmarks]</b> Failed to parse data for Slot {slotIndex}. Error: {ex.Message}"
                    );
                }
            }
        }
    }
}
#endif
