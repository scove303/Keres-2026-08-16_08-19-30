// ===============================================================================
// DistributeTool - Professional Object Alignment & Distribution
//
// Creator: Scove
// Last Updated: 2024-05-08
// Version: 2.0
//
// Purpose:
// This tool helps organize multiple objects by distributing them evenly along
// the X, Y, or Z axis. It's essential for creating fences, grids, or UI in 3D space.
//
// Key Features:
// 1. Distribute Between Bounds: Keeps the first and last object in place, fills the gap.
// 2. Fixed Spacing: Moves objects based on a specific numerical offset.
// 3. Smart Sorting: Automatically sorts objects by position before distributing.
// 4. Undo Support: Full integration with Unity's Undo system.
//
// How to Use:
// 1. Place this script in an 'Editor' folder.
// 2. Open via: Menu -> Tools -> Distribute Tool.
// 3. Select 3 or more objects in the Hierarchy/Scene.
// 4. Click the desired axis button (X, Y, or Z).
// ===============================================================================

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Tools
{
    public class DistributeTool : EditorWindow
    {
        private float fixedSpacing = 1.0f;
        private bool useFixedSpacing = false;

        [MenuItem("Tools/Distribute Tool")]
        public static void ShowWindow()
        {
            GetWindow<DistributeTool>("Distribute");
        }

        private void OnGUI()
        {
            GUILayout.Label("Distribution Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Mode Selection
            useFixedSpacing = EditorGUILayout.Toggle("Use Fixed Spacing", useFixedSpacing);

            if (useFixedSpacing)
            {
                fixedSpacing = EditorGUILayout.FloatField("Distance Offset", fixedSpacing);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Linear Mode: Objects will be distributed evenly between the first and last selected items.",
                    MessageType.Info
                );
            }

            EditorGUILayout.Space();
            GUILayout.Label("Distribute Along Axis:", EditorStyles.label);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("X Axis", GUILayout.Height(30)))
                Distribute(0);
            if (GUILayout.Button("Y Axis", GUILayout.Height(30)))
                Distribute(1);
            if (GUILayout.Button("Z Axis", GUILayout.Height(30)))
                Distribute(2);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Helpful Reminder
            if (Selection.transforms.Length < 3)
            {
                EditorGUILayout.HelpBox(
                    "Please select at least 3 objects to distribute.",
                    MessageType.Warning
                );
            }
            else
            {
                GUILayout.Label(
                    $"Objects Selected: {Selection.transforms.Length}",
                    EditorStyles.miniLabel
                );
            }
        }

        private void Distribute(int axis) // 0=x, 1=y, 2=z
        {
            Transform[] selection = Selection.transforms;

            if (selection.Length < 3)
            {
                Debug.LogWarning("[DistributeTool] You need to select at least 3 objects.");
                return;
            }

            // Register Undo for all selected objects
            Undo.RecordObjects(selection, "Distribute Objects");

            // Sort selection by position on the chosen axis to maintain visual order
            var sorted = selection.OrderBy(t => t.position[axis]).ToList();

            if (useFixedSpacing)
            {
                // Fixed Spacing Logic: Move each object relative to the first one
                Vector3 startPos = sorted[0].position;
                for (int i = 1; i < sorted.Count; i++)
                {
                    Vector3 newPos = sorted[i].position;
                    newPos[axis] = startPos[axis] + (fixedSpacing * i);
                    sorted[i].position = newPos;
                }
            }
            else
            {
                // Linear Distribution Logic: Fill the space between first and last
                float start = sorted.First().position[axis];
                float end = sorted.Last().position[axis];
                float totalDistance = end - start;

                // Avoid division by zero if objects are at the same spot
                if (Mathf.Abs(totalDistance) < 0.0001f)
                    return;

                float step = totalDistance / (sorted.Count - 1);

                for (int i = 0; i < sorted.Count; i++)
                {
                    Vector3 newPos = sorted[i].position;
                    newPos[axis] = start + (step * i);
                    sorted[i].position = newPos;
                }
            }

            Debug.Log(
                $"<color=#FFCC00><b>[DistributeTool]</b></color> Distributed {sorted.Count} objects along the {(axis == 0 ? "X" : axis == 1 ? "Y" : "Z")} axis."
            );
        }
    }
}
#endif
