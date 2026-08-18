#if UNITY_EDITOR
// ===============================================================================
// TimeLord - In-Scene Time Manipulation Tool
//
// Creator: Scove
// Last Updated: 2026-03-03
// Version: 2.0 (Sleek UI & Ergonomic Controls)
//
// Purpose:
// Provides a floating, interactive control panel inside the Scene View during
// Play Mode. Allows developers to easily manipulate game time (slow motion,
// fast forward, or pause) without constantly looking away from the action.
//
// Key Features:
// 1. Floating Dashboard: Clean, unobtrusive UI placed directly in the Scene View.
// 2. Dynamic Slider: Drag to fine-tune the time scale smoothly from 0x to 10x.
// 3. Smart Highlighting: Active speeds light up (Green), making it easy to read.
// 4. Auto-Resume: Clicking a speed preset while paused automatically resumes play.
// 5. Visual Pause State: Prominent pause button changes color when active.
//
// How to Use:
// 1. Place this script in an 'Editor' folder.
// 2. Hit PLAY in Unity.
// 3. Move your mouse to the Scene View and use the top-center Time Lord panel!
// ===============================================================================

using UnityEditor;
using UnityEngine;

namespace Assets.Tools
{
    [InitializeOnLoad]
    public class TimeLord
    {
        // Panel configuration
        private const float PANEL_WIDTH = 340f;
        private const float PANEL_HEIGHT = 105f;

        static TimeLord()
        {
            // Unsubscribe first to prevent double-hooking upon script recompile
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!Application.isPlaying)
                return;

            Handles.BeginGUI();

            float panelWidth = 430f;
            float panelHeight = 40f;
            float posX = (sceneView.position.width - panelWidth) / 2f;
            float posY = 20f;
            Rect panelRect = new Rect(posX, posY, panelWidth, panelHeight);

            // Sleek semi-transparent dark background
            EditorGUI.DrawRect(panelRect, new Color(0.1f, 0.1f, 0.1f, 0.50f));

            // Thin accent line at the top
            EditorGUI.DrawRect(
                new Rect(posX, posY, panelWidth, 2),
                new Color(0.3f, 0.7f, 1f, 0.8f)
            );

            GUILayout.BeginArea(new Rect(posX + 10, posY + 8, panelWidth - 20, panelHeight - 16));
            GUILayout.BeginHorizontal();

            // --- 1. HEADER (Current Speed) ---
            string currentSpeedText = EditorApplication.isPaused
                ? "<color=#FF6B6B>PAUSED</color>"
                : $"<color=#4ECDC4>{Time.timeScale:F2}x</color>";
            GUILayout.Label(
                $"{currentSpeedText}",
                new GUIStyle(EditorStyles.boldLabel)
                {
                    richText = true,
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft,
                },
                GUILayout.Width(50)
            );

            // --- 2. TIME SLIDER (Fine-tune control) ---
            EditorGUI.BeginChangeCheck();
            float newTimeScale = GUILayout.HorizontalSlider(
                Time.timeScale,
                0f,
                10f,
                GUILayout.Width(100),
                GUILayout.Height(20)
            );
            if (EditorGUI.EndChangeCheck())
            {
                Time.timeScale = newTimeScale;
                if (EditorApplication.isPaused && newTimeScale > 0f)
                {
                    EditorApplication.isPaused = false;
                }
            }

            GUILayout.Space(10);

            // --- 3. PRESET SPEED BUTTONS ---
            DrawSpeedButton("0.5x", 0.5f, 35);
            DrawSpeedButton("1x", 1f, 30);
            DrawSpeedButton("2x", 2f, 30);
            DrawSpeedButton("5x", 5f, 30);

            GUILayout.Space(10);

            // --- 4. PAUSE / RESUME BUTTON ---
            Color oldBgColor = GUI.backgroundColor;
            GUI.backgroundColor = EditorApplication.isPaused
                ? new Color(1f, 0.4f, 0.4f)
                : new Color(0.8f, 0.8f, 0.8f);

            string pauseLabel = EditorApplication.isPaused ? "▶ RESUME" : "⏸ PAUSE";
            GUIStyle pauseStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };

            if (GUILayout.Button(pauseLabel, pauseStyle, GUILayout.Width(80), GUILayout.Height(24)))
            {
                EditorApplication.isPaused = !EditorApplication.isPaused;
            }
            GUI.backgroundColor = oldBgColor;

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            Handles.EndGUI();
        }

        /// <summary>
        /// Draws a preset button that automatically highlights green if it matches the current time scale.
        /// </summary>
        private static void DrawSpeedButton(string label, float targetSpeed, float width)
        {
            Color oldBgColor = GUI.backgroundColor;

            // Highlight green if this is the active speed AND the game is not paused
            bool isActive =
                Mathf.Approximately(Time.timeScale, targetSpeed) && !EditorApplication.isPaused;
            if (isActive)
            {
                GUI.backgroundColor = new Color(0.4f, 1f, 0.4f); // Light Green
            }

            if (GUILayout.Button(label, GUILayout.Width(width), GUILayout.Height(24)))
            {
                Time.timeScale = targetSpeed;

                // Auto-resume if the player clicked a speed while paused
                if (EditorApplication.isPaused)
                {
                    EditorApplication.isPaused = false;
                }
            }

            // Restore previous color
            GUI.backgroundColor = oldBgColor;
        }
    }
}
#endif
