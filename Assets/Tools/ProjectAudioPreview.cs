// ===============================================================================
// ProjectAudioPreview - Instant Audio Preview in Project Window
//
// Creator: Scove
// Last Updated: 2026-03-03
// Version: 2.0
//
// Purpose:
// Allows users to quickly preview AudioClips directly from the Project window
// without selecting them or looking at the Inspector.
//
// Key Features:
// 1. Hover-to-Show: Play button only appears when hovering over an audio file.
// 2. Play/Stop Toggle: Single button to start and stop the preview.
// 3. High Performance: Cached reflection methods to prevent UI lag.
// 4. Integrated UI: Uses Unity's built-in editor icons for a native feel.
//
// How to Use:
// 1. Place this script in an 'Editor' folder.
// 2. Open the Project window in List View (Two-Column layout).
// 3. Hover over any AudioClip; a Play/Stop icon will appear on the right side.
// ===============================================================================

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace Assets.Tools
{
    [InitializeOnLoad]
    public class ProjectAudioPreview
    {
        // Reflection Cache
        private static MethodInfo playPreviewMethod;
        private static MethodInfo stopAllPreviewMethod;
        private static MethodInfo isPreviewPlayingMethod;

        private static AudioClip currentlyPlayingClip;

        static ProjectAudioPreview()
        {
            // Initialize Reflection once to save performance
            InitReflection();

            // Subscribe to project window item GUI
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowGUI;
        }

        private static void InitReflection()
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            if (audioUtilClass != null)
            {
                playPreviewMethod = audioUtilClass.GetMethod(
                    "PlayPreviewClip",
                    BindingFlags.Static | BindingFlags.Public
                );
                stopAllPreviewMethod = audioUtilClass.GetMethod(
                    "StopAllPreviewClips",
                    BindingFlags.Static | BindingFlags.Public
                );
                isPreviewPlayingMethod = audioUtilClass.GetMethod(
                    "IsPreviewClipPlaying",
                    BindingFlags.Static | BindingFlags.Public
                );
            }
        }

        static void OnProjectWindowGUI(string guid, Rect selectionRect)
        {
            // Optimization: Only process if the row is wide enough (List View)
            if (selectionRect.width < 50)
                return;

            // Only show button if mouse is hovering over the current item
            Event e = Event.current;
            if (!selectionRect.Contains(e.mousePosition))
                return;

            // Check if asset is an AudioClip
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioType audioType = GetAudioType(path);

            if (audioType != AudioType.UNKNOWN)
            {
                DrawPreviewButton(selectionRect, path);
            }
        }

        private static void DrawPreviewButton(Rect rect, string path)
        {
            // Calculate button position (Right-aligned)
            Rect btnRect = new Rect(rect.xMax - 25, rect.y, 20, rect.height);

            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
                return;

            bool isPlaying = IsClipPlaying(clip);

            // Choose icon based on state
            // "d_PlayButton" and "d_PreMatQuad" are internal Unity icons
            GUIContent icon = isPlaying
                ? EditorGUIUtility.IconContent("d_PreMatQuad")
                : EditorGUIUtility.IconContent("d_PlayButton");

            // Styling the button
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.padding = new RectOffset(0, 0, 0, 0);

            // FIX: backgroundColor is a property of GUI, not GUIStyle
            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = isPlaying ? Color.cyan : Color.white;

            if (GUI.Button(btnRect, icon, btnStyle))
            {
                if (isPlaying)
                {
                    StopAllClips();
                }
                else
                {
                    StopAllClips(); // Stop previous before playing new
                    PlayClip(clip);
                }
            }

            // Restore original background color
            GUI.backgroundColor = prevColor;
        }

        private static void PlayClip(AudioClip clip)
        {
            if (playPreviewMethod != null)
            {
                currentlyPlayingClip = clip;
                playPreviewMethod.Invoke(null, new object[] { clip, 0, false });
            }
        }

        private static void StopAllClips()
        {
            if (stopAllPreviewMethod != null)
            {
                stopAllPreviewMethod.Invoke(null, new object[] { });
                currentlyPlayingClip = null;
            }
        }

        private static bool IsClipPlaying(AudioClip clip)
        {
            if (isPreviewPlayingMethod != null && currentlyPlayingClip == clip)
            {
                return (bool)isPreviewPlayingMethod.Invoke(null, new object[] { });
            }
            return false;
        }

        private static AudioType GetAudioType(string path)
        {
            string ext = System.IO.Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".mp3":
                    return AudioType.MPEG;
                case ".wav":
                    return AudioType.WAV;
                case ".ogg":
                    return AudioType.OGGVORBIS;
                case ".aiff":
                    return AudioType.AIFF;
                default:
                    return AudioType.UNKNOWN;
            }
        }
    }
}
#endif
