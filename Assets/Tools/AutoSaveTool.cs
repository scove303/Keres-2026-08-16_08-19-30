// ===============================================================================
// AutoSaveTool - Persistent & Robust Auto-Saving for Unity Editor
//
// Creator: Scove
// Last Updated: 2024-05-08
// Version: 2.0
//
// Purpose:
// This tool provides a persistent, background auto-saving mechanism for the Unity Editor.
// It runs automatically when Unity starts and saves all open scenes and modified assets
// at a user-defined interval, even when the settings window is closed.
//
// Key Features:
// 1. Runs persistently in the background via [InitializeOnLoad].
// 2. Saves configuration (interval, status) using EditorPrefs, persisting across Unity sessions.
// 3. Uses System.DateTime for accurate time tracking, unaffected by Play Mode reloads.
// 4. Pauses counting down during Play Mode or compilation to prevent accidental saving.
//
// How to Use:
// 1. Place this script in an 'Editor' folder in your project.
// 2. Open the settings window via: Menu -> Tools -> Auto Save Settings.
// 3. Enable "Bật Auto Save" (Enable Auto Save).
// 4. Set the desired "Thời gian (phút)" (Interval in minutes).
// 5. The tool will now save automatically in the background according to the schedule.
// ===============================================================================

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Assets.Tools
{
    [InitializeOnLoad]
    public class AutoSaveTool : EditorWindow
    {
        private static bool isAutoSaveEnabled;
        private static float saveIntervalMinutes;
        private static bool showDebugLog;
        private static DateTime nextSaveTime;

        private Label statusLabel;
        private VisualElement progressBar;

        [InitializeOnLoadMethod]
        static void Init()
        {
            isAutoSaveEnabled = EditorPrefs.GetBool("AutoSave_Enabled", false);
            saveIntervalMinutes = EditorPrefs.GetFloat("AutoSave_Interval", 5f);
            showDebugLog = EditorPrefs.GetBool("AutoSave_Log", true);

            ResetTimer();
            EditorApplication.update += OnEditorUpdate;
        }

        [MenuItem("Tools/Auto Save Settings")]
        public static void ShowWindow()
        {
            GetWindow<AutoSaveTool>("Auto Save");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingTop = 15;
            root.style.paddingBottom = 15;
            root.style.paddingLeft = 15;
            root.style.paddingRight = 15;

            Label title = new Label("Auto Save Settings");
            title.style.fontSize = 22;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = new StyleColor(new Color(0.3f, 0.7f, 1f));
            title.style.marginBottom = 15;
            root.Add(title);

            // Settings Card
            VisualElement settingsCard = CreateCard();

            Toggle enableToggle = new Toggle("Enable Auto Save");
            enableToggle.value = isAutoSaveEnabled;
            enableToggle.RegisterValueChangedCallback(evt =>
            {
                isAutoSaveEnabled = evt.newValue;
                EditorPrefs.SetBool("AutoSave_Enabled", isAutoSaveEnabled);
                ResetTimer();
                UpdateUIStatus();
            });
            enableToggle.style.unityFontStyleAndWeight = FontStyle.Bold;
            settingsCard.Add(enableToggle);

            Slider intervalSlider = new Slider("Interval (Minutes)", 0.5f, 60f);
            intervalSlider.value = saveIntervalMinutes;
            intervalSlider.style.marginTop = 15;
            intervalSlider.RegisterValueChangedCallback(evt =>
            {
                saveIntervalMinutes = evt.newValue;
                EditorPrefs.SetFloat("AutoSave_Interval", saveIntervalMinutes);
                ResetTimer();
            });

            // Add a label to show exact slider value
            Label intervalLabel = new Label($"Current: {saveIntervalMinutes:F1} min");
            intervalLabel.style.alignSelf = Align.FlexEnd;
            intervalLabel.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            intervalSlider.RegisterValueChangedCallback(evt =>
            {
                intervalLabel.text = $"Current: {evt.newValue:F1} min";
            });

            settingsCard.Add(intervalSlider);
            settingsCard.Add(intervalLabel);

            Toggle logToggle = new Toggle("Show Debug Log");
            logToggle.value = showDebugLog;
            logToggle.style.marginTop = 10;
            logToggle.RegisterValueChangedCallback(evt =>
            {
                showDebugLog = evt.newValue;
                EditorPrefs.SetBool("AutoSave_Log", showDebugLog);
            });
            settingsCard.Add(logToggle);

            root.Add(settingsCard);

            // Status Card
            VisualElement statusCard = CreateCard();
            statusCard.style.marginTop = 15;

            Label statusTitle = new Label("System Status");
            statusTitle.style.fontSize = 16;
            statusTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            statusTitle.style.marginBottom = 15;
            statusCard.Add(statusTitle);

            statusLabel = new Label();
            statusLabel.style.fontSize = 15;
            statusLabel.style.marginBottom = 10;
            statusCard.Add(statusLabel);

            // Progress bar
            VisualElement progressBg = new VisualElement();
            progressBg.style.height = 12;
            progressBg.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f));
            progressBg.style.borderTopLeftRadius = 6;
            progressBg.style.borderTopRightRadius = 6;
            progressBg.style.borderBottomLeftRadius = 6;
            progressBg.style.borderBottomRightRadius = 6;
            progressBg.style.marginBottom = 20;
            progressBg.style.overflow = Overflow.Hidden;

            progressBar = new VisualElement();
            progressBar.style.height = 12;
            progressBar.style.backgroundColor = new StyleColor(new Color(0.3f, 0.7f, 1f));
            progressBar.style.width = Length.Percent(0);
            progressBg.Add(progressBar);

            statusCard.Add(progressBg);

            Button saveNowBtn = new Button(() => SaveNow());
            saveNowBtn.text = "Force Save Now";
            saveNowBtn.style.height = 40;
            saveNowBtn.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.3f));
            saveNowBtn.style.color = new StyleColor(Color.white);
            saveNowBtn.style.fontSize = 14;
            saveNowBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            saveNowBtn.style.borderTopLeftRadius = 6;
            saveNowBtn.style.borderTopRightRadius = 6;
            saveNowBtn.style.borderBottomLeftRadius = 6;
            saveNowBtn.style.borderBottomRightRadius = 6;
            statusCard.Add(saveNowBtn);

            root.Add(statusCard);

            // Periodically update the countdown text and progress bar
            root.schedule.Execute(UpdateUIStatus).Every(100);
            UpdateUIStatus();

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

        private void UpdateUIStatus()
        {
            if (statusLabel == null || progressBar == null)
                return;

            if (!isAutoSaveEnabled)
            {
                statusLabel.text = "Auto Save is DISABLED.";
                statusLabel.style.color = new StyleColor(new Color(0.85f, 0.35f, 0.35f));
                progressBar.style.width = Length.Percent(0);
                return;
            }

            if (EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                statusLabel.text = "PAUSED (Play Mode / Compiling)";
                statusLabel.style.color = new StyleColor(new Color(0.9f, 0.75f, 0.2f));
                progressBar.style.backgroundColor = new StyleColor(new Color(0.9f, 0.75f, 0.2f));
                return;
            }

            TimeSpan timeRemaining = nextSaveTime - DateTime.Now;
            if (timeRemaining.TotalSeconds < 0)
                timeRemaining = TimeSpan.Zero;

            string timeStr = string.Format(
                "{0:00}:{1:00}",
                timeRemaining.Minutes,
                timeRemaining.Seconds
            );
            statusLabel.text = $"Auto-saving in: {timeStr}";
            statusLabel.style.color = new StyleColor(new Color(0.6f, 0.8f, 1f));
            progressBar.style.backgroundColor = new StyleColor(new Color(0.3f, 0.7f, 1f));

            float totalSeconds = saveIntervalMinutes * 60f;
            float elapsed = totalSeconds - (float)timeRemaining.TotalSeconds;
            float percent = Mathf.Clamp01(elapsed / totalSeconds) * 100f;
            progressBar.style.width = Length.Percent(percent);
        }

        private static void OnEditorUpdate()
        {
            if (!isAutoSaveEnabled)
                return;

            if (EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                nextSaveTime = nextSaveTime.AddSeconds(Time.unscaledDeltaTime);
                return;
            }

            if (DateTime.Now >= nextSaveTime)
            {
                SaveNow();
            }
        }

        private static void ResetTimer()
        {
            nextSaveTime = DateTime.Now.AddMinutes(saveIntervalMinutes);
        }

        private static void SaveNow()
        {
            var currentScene = EditorSceneManager.GetActiveScene();
            bool isSaved = false;

            if (currentScene.isDirty && !string.IsNullOrEmpty(currentScene.path))
            {
                EditorSceneManager.SaveOpenScenes();
                isSaved = true;
            }

            AssetDatabase.SaveAssets();
            isSaved = true;

            if (isSaved && showDebugLog)
            {
                Debug.Log(
                    $"<color=#00FF00><b>[AutoSave]</b></color> Project saved automatically at {DateTime.Now.ToString("HH:mm:ss")}"
                );
            }

            ResetTimer();
        }
    }
}
#endif
