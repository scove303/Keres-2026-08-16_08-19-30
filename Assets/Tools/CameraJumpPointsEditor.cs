using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Tools
{
    [CustomEditor(typeof(CameraJumpPoints))]
    public class CameraJumpPointsEditor : UnityEditor.Editor
    {
        private static CameraJumpPoints[] allPoints;

        // Store previous pose
        private static Vector3 previousPosition;
        private static Quaternion previousRotation;
        private static bool hasPreviousPose = false;
        private static Transform currentJumpTarget = null;

        [InitializeOnLoadMethod]
        static void Init()
        {
            SceneView.duringSceneGui += OnSceneGUIStatic;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        static void OnHierarchyChanged()
        {
            allPoints = Object.FindObjectsByType<CameraJumpPoints>(FindObjectsSortMode.None);
        }

        static void OnSceneGUIStatic(SceneView sceneView)
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
            {
                if (allPoints == null)
                {
                    allPoints = Object.FindObjectsByType<CameraJumpPoints>(FindObjectsSortMode.None);
                }

                bool needsRefresh = false;

                foreach (var cp in allPoints)
                {
                    if (cp == null)
                    {
                        needsRefresh = true;
                        continue;
                    }

                    foreach (var pt in cp.points)
                    {
                        if (
                            pt.key == e.keyCode
                            && pt.ctrl == e.control
                            && pt.alt == e.alt
                            && pt.shift == e.shift
                        )
                        {
                            if (pt.target == null)
                                continue;

                            if (currentJumpTarget == pt.target && hasPreviousPose)
                            {
                                // Jump back
                                sceneView.pivot = previousPosition;
                                sceneView.rotation = previousRotation;
                                currentJumpTarget = null;
                                hasPreviousPose = false;
                                sceneView.Repaint();
                            }
                            else
                            {
                                // Save previous pose
                                previousPosition = sceneView.pivot;
                                previousRotation = sceneView.rotation;
                                hasPreviousPose = true;
                                currentJumpTarget = pt.target;

                                // Jump to target exactly matching camera position
                                sceneView.rotation = pt.target.rotation;
                                sceneView.pivot =
                                    pt.target.position
                                    + pt.target.forward * sceneView.cameraDistance;
                                sceneView.Repaint();
                            }

                            e.Use();
                            return;
                        }
                    }
                }

                if (needsRefresh)
                {
                    allPoints = Object.FindObjectsByType<CameraJumpPoints>(FindObjectsSortMode.None);
                }
            }

            // Lock camera movement when in View Mode
            if (hasPreviousPose && currentJumpTarget != null)
            {
                bool isNavigating = false;

                if (e.type == EventType.KeyDown)
                {
                    if (
                        e.keyCode == KeyCode.W
                        || e.keyCode == KeyCode.A
                        || e.keyCode == KeyCode.S
                        || e.keyCode == KeyCode.D
                        || e.keyCode == KeyCode.Q
                        || e.keyCode == KeyCode.E
                        || e.keyCode == KeyCode.UpArrow
                        || e.keyCode == KeyCode.DownArrow
                        || e.keyCode == KeyCode.LeftArrow
                        || e.keyCode == KeyCode.RightArrow
                    )
                    {
                        isNavigating = true;
                    }
                }
                else if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
                {
                    // Right click (1), Middle click (2), or Alt+Left click (orbit)
                    if (e.button == 1 || e.button == 2 || (e.button == 0 && e.alt))
                    {
                        isNavigating = true;
                    }
                }
                else if (e.type == EventType.ScrollWheel)
                {
                    isNavigating = true;
                }

                if (isNavigating)
                {
                    string hotkeyStr = "your shortcut";
                    if (allPoints != null)
                    {
                        foreach (var cp in allPoints)
                        {
                            if (cp == null)
                                continue;
                            foreach (var pt in cp.points)
                            {
                                if (pt.target == currentJumpTarget)
                                {
                                    hotkeyStr = pt.GetKeyString();
                                    break;
                                }
                            }
                        }
                    }

                    sceneView.ShowNotification(
                        new GUIContent($"View Mode Locked. Press {hotkeyStr} again to escape.")
                    );
                    e.Use();
                }
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.style.paddingTop = 15;
            root.style.paddingBottom = 15;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;

            // Title
            Label title = new Label("Camera Bookmarks");
            title.style.fontSize = 22;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 15;
            title.style.color = new StyleColor(new Color(0.3f, 0.7f, 1f));
            root.Add(title);

            // Help Box Custom
            VisualElement helpBox = new VisualElement();
            helpBox.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 0.8f));
            helpBox.style.borderLeftWidth = 4;
            helpBox.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.7f, 1f));
            helpBox.style.borderTopRightRadius = 6;
            helpBox.style.borderBottomRightRadius = 6;
            helpBox.style.paddingTop = 12;
            helpBox.style.paddingBottom = 12;
            helpBox.style.paddingLeft = 12;
            helpBox.style.paddingRight = 12;
            helpBox.style.marginBottom = 20;

            Label helpText = new Label(
                "• Jump instantly via hotkeys in Scene View.\n• Press the exact same hotkey again to return.\n• Use 'Capture Current View' for quick setup."
            );
            helpText.style.whiteSpace = WhiteSpace.Normal;
            helpText.style.color = new StyleColor(new Color(0.85f, 0.85f, 0.85f));
            helpBox.Add(helpText);
            root.Add(helpBox);

            // List Container
            VisualElement listContainer = new VisualElement();
            root.Add(listContainer);

            SerializedProperty pointsProp = serializedObject.FindProperty("points");
            System.Action rebuildList = null;

            rebuildList = () =>
            {
                listContainer.Clear();
                serializedObject.Update();

                for (int i = 0; i < pointsProp.arraySize; i++)
                {
                    int index = i;
                    SerializedProperty pointProp = pointsProp.GetArrayElementAtIndex(index);

                    // Card UI
                    VisualElement card = new VisualElement();
                    card.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 0.9f));
                    card.style.borderTopLeftRadius = 8;
                    card.style.borderTopRightRadius = 8;
                    card.style.borderBottomLeftRadius = 8;
                    card.style.borderBottomRightRadius = 8;
                    card.style.paddingTop = 15;
                    card.style.paddingBottom = 15;
                    card.style.paddingLeft = 15;
                    card.style.paddingRight = 15;
                    card.style.marginBottom = 12;

                    // Subtle shadow/border
                    card.style.borderTopWidth = 1;
                    card.style.borderBottomWidth = 1;
                    card.style.borderLeftWidth = 1;
                    card.style.borderRightWidth = 1;
                    card.style.borderTopColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
                    card.style.borderBottomColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
                    card.style.borderLeftColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
                    card.style.borderRightColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));

                    // Header
                    VisualElement header = new VisualElement();
                    header.style.flexDirection = FlexDirection.Row;
                    header.style.justifyContent = Justify.SpaceBetween;
                    header.style.marginBottom = 12;

                    PropertyField nameField = new PropertyField(
                        pointProp.FindPropertyRelative("name"),
                        ""
                    );
                    nameField.style.flexGrow = 1;
                    nameField.style.unityFontStyleAndWeight = FontStyle.Bold;

                    Button deleteBtn = new Button(() =>
                    {
                        pointsProp.DeleteArrayElementAtIndex(index);
                        serializedObject.ApplyModifiedProperties();
                        rebuildList();
                    });
                    deleteBtn.text = "✕";
                    deleteBtn.style.backgroundColor = new StyleColor(
                        new Color(0.8f, 0.25f, 0.25f, 0.9f)
                    );
                    deleteBtn.style.color = new StyleColor(Color.white);
                    deleteBtn.style.borderTopLeftRadius = 4;
                    deleteBtn.style.borderTopRightRadius = 4;
                    deleteBtn.style.borderBottomLeftRadius = 4;
                    deleteBtn.style.borderBottomRightRadius = 4;
                    deleteBtn.style.width = 24;
                    deleteBtn.style.height = 24;
                    deleteBtn.style.marginLeft = 15;
                    deleteBtn.style.unityFontStyleAndWeight = FontStyle.Bold;

                    header.Add(nameField);
                    header.Add(deleteBtn);
                    card.Add(header);

                    // Target Row with Sync button
                    VisualElement targetRow = new VisualElement();
                    targetRow.style.flexDirection = FlexDirection.Row;
                    targetRow.style.marginBottom = 12;

                    PropertyField targetField = new PropertyField(
                        pointProp.FindPropertyRelative("target"),
                        "Target Object"
                    );
                    targetField.style.flexGrow = 1;
                    targetRow.Add(targetField);

                    Button syncBtn = new Button(() =>
                    {
                        SceneView sv = SceneView.lastActiveSceneView;
                        if (sv != null && sv.camera != null)
                        {
                            Transform t = (Transform)
                                pointProp.FindPropertyRelative("target").objectReferenceValue;
                            if (t != null)
                            {
                                Undo.RecordObject(t, "Sync Target View");
                                t.position = sv.camera.transform.position;
                                t.rotation = sv.camera.transform.rotation;
                            }
                        }
                    });
                    syncBtn.text = "Sync to Scene";
                    syncBtn.tooltip =
                        "Updates this object's transform to match your current Scene View.";
                    syncBtn.style.marginLeft = 10;
                    syncBtn.style.paddingLeft = 8;
                    syncBtn.style.paddingRight = 8;
                    syncBtn.style.borderTopLeftRadius = 4;
                    syncBtn.style.borderTopRightRadius = 4;
                    syncBtn.style.borderBottomLeftRadius = 4;
                    syncBtn.style.borderBottomRightRadius = 4;
                    syncBtn.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
                    targetRow.Add(syncBtn);

                    card.Add(targetRow);

                    // Key binding Row
                    VisualElement keyRow = new VisualElement();
                    keyRow.style.flexDirection = FlexDirection.Row;
                    keyRow.style.alignItems = Align.Center;

                    Label keyLabel = new Label("Shortcut Key");
                    keyLabel.style.width = 120;
                    keyRow.Add(keyLabel);

                    Button recordBtn = new Button();
                    recordBtn.style.flexGrow = 1;
                    recordBtn.style.height = 30;
                    recordBtn.style.borderTopLeftRadius = 6;
                    recordBtn.style.borderTopRightRadius = 6;
                    recordBtn.style.borderBottomLeftRadius = 6;
                    recordBtn.style.borderBottomRightRadius = 6;
                    recordBtn.style.unityFontStyleAndWeight = FontStyle.Bold;

                    System.Action updateBtnText = () =>
                    {
                        SerializedProperty keyP = pointProp.FindPropertyRelative("key");
                        SerializedProperty cP = pointProp.FindPropertyRelative("ctrl");
                        SerializedProperty aP = pointProp.FindPropertyRelative("alt");
                        SerializedProperty sP = pointProp.FindPropertyRelative("shift");

                        if ((KeyCode)keyP.intValue == KeyCode.None)
                        {
                            recordBtn.text = "Not Set (Click to Record)";
                            recordBtn.style.backgroundColor = new StyleColor(
                                new Color(0.3f, 0.3f, 0.3f)
                            );
                        }
                        else
                        {
                            string s = "";
                            if (cP.boolValue)
                                s += "Ctrl + ";
                            if (aP.boolValue)
                                s += "Alt + ";
                            if (sP.boolValue)
                                s += "Shift + ";
                            s += ((KeyCode)keyP.intValue).ToString();
                            recordBtn.text = s;
                            recordBtn.style.backgroundColor = new StyleColor(
                                new Color(0.15f, 0.45f, 0.8f)
                            );
                        }
                    };

                    updateBtnText();
                    bool isRecording = false;

                    recordBtn.clicked += () =>
                    {
                        if (isRecording)
                        {
                            isRecording = false;
                            updateBtnText();
                            return;
                        }
                        isRecording = true;
                        recordBtn.text = "Press any key... (ESC to cancel)";
                        recordBtn.style.backgroundColor = new StyleColor(
                            new Color(0.8f, 0.4f, 0.1f)
                        );
                        recordBtn.Focus();
                    };

                    recordBtn.RegisterCallback<KeyDownEvent>(
                        (evt) =>
                        {
                            if (!isRecording)
                                return;

                            if (evt.keyCode == KeyCode.Escape)
                            {
                                isRecording = false;
                                updateBtnText();
                                evt.StopPropagation();
                                return;
                            }

                            if (
                                evt.keyCode != KeyCode.None
                                && evt.keyCode != KeyCode.LeftAlt
                                && evt.keyCode != KeyCode.RightAlt
                                && evt.keyCode != KeyCode.LeftControl
                                && evt.keyCode != KeyCode.RightControl
                                && evt.keyCode != KeyCode.LeftShift
                                && evt.keyCode != KeyCode.RightShift
                                && evt.keyCode != KeyCode.LeftCommand
                                && evt.keyCode != KeyCode.RightCommand
                            )
                            {
                                pointProp.FindPropertyRelative("key").intValue = (int)evt.keyCode;
                                pointProp.FindPropertyRelative("ctrl").boolValue = evt.ctrlKey;
                                pointProp.FindPropertyRelative("alt").boolValue = evt.altKey;
                                pointProp.FindPropertyRelative("shift").boolValue = evt.shiftKey;

                                serializedObject.ApplyModifiedProperties();
                                isRecording = false;
                                updateBtnText();
                                evt.StopPropagation();
                            }
                        }
                    );

                    recordBtn.focusable = true;
                    keyRow.Add(recordBtn);
                    card.Add(keyRow);

                    listContainer.Add(card);
                }

                // Buttons layout
                VisualElement buttonsRow = new VisualElement();
                buttonsRow.style.flexDirection = FlexDirection.Row;
                buttonsRow.style.marginTop = 15;

                // Capture Current View Button
                Button captureBtn = new Button(() =>
                {
                    SceneView sv = SceneView.lastActiveSceneView;
                    if (sv != null && sv.camera != null)
                    {
                        GameObject go = new GameObject(
                            "Camera Point " + (pointsProp.arraySize + 1)
                        );
                        CameraJumpPoints tgt = (CameraJumpPoints)target;
                        go.transform.SetParent(tgt.transform);
                        go.transform.position = sv.camera.transform.position;
                        go.transform.rotation = sv.camera.transform.rotation;

                        Undo.RegisterCreatedObjectUndo(go, "Capture Camera Point");

                        pointsProp.arraySize++;
                        SerializedProperty newElem = pointsProp.GetArrayElementAtIndex(
                            pointsProp.arraySize - 1
                        );
                        newElem.FindPropertyRelative("name").stringValue = go.name;
                        newElem.FindPropertyRelative("target").objectReferenceValue = go.transform;
                        newElem.FindPropertyRelative("key").intValue = (int)KeyCode.None;
                        newElem.FindPropertyRelative("ctrl").boolValue = false;
                        newElem.FindPropertyRelative("alt").boolValue = false;
                        newElem.FindPropertyRelative("shift").boolValue = false;

                        serializedObject.ApplyModifiedProperties();
                        rebuildList();
                    }
                    else
                    {
                        Debug.LogWarning("No active Scene View found to capture.");
                    }
                });
                captureBtn.text = "+ Capture Scene View";
                captureBtn.style.flexGrow = 1;
                captureBtn.style.height = 40;
                captureBtn.style.fontSize = 14;
                captureBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
                captureBtn.style.backgroundColor = new StyleColor(new Color(0.25f, 0.65f, 0.4f));
                captureBtn.style.color = new StyleColor(Color.white);
                captureBtn.style.borderTopLeftRadius = 8;
                captureBtn.style.borderTopRightRadius = 8;
                captureBtn.style.borderBottomLeftRadius = 8;
                captureBtn.style.borderBottomRightRadius = 8;
                captureBtn.style.marginRight = 5;

                // Add Empty Button
                Button addEmptyBtn = new Button(() =>
                {
                    pointsProp.arraySize++;
                    SerializedProperty newElem = pointsProp.GetArrayElementAtIndex(
                        pointsProp.arraySize - 1
                    );
                    newElem.FindPropertyRelative("name").stringValue =
                        "New Empty " + pointsProp.arraySize;
                    newElem.FindPropertyRelative("target").objectReferenceValue = null;
                    newElem.FindPropertyRelative("key").intValue = (int)KeyCode.None;
                    newElem.FindPropertyRelative("ctrl").boolValue = false;
                    newElem.FindPropertyRelative("alt").boolValue = false;
                    newElem.FindPropertyRelative("shift").boolValue = false;

                    serializedObject.ApplyModifiedProperties();
                    rebuildList();
                });
                addEmptyBtn.text = "Add Empty";
                addEmptyBtn.style.width = 100;
                addEmptyBtn.style.height = 40;
                addEmptyBtn.style.borderTopLeftRadius = 8;
                addEmptyBtn.style.borderTopRightRadius = 8;
                addEmptyBtn.style.borderBottomLeftRadius = 8;
                addEmptyBtn.style.borderBottomRightRadius = 8;

                buttonsRow.Add(captureBtn);
                buttonsRow.Add(addEmptyBtn);

                listContainer.Add(buttonsRow);
            };

            rebuildList();

            // Listen for Unity serialization changes (e.g. Undo/Redo)
            root.TrackPropertyValue(
                pointsProp,
                (prop) =>
                {
                    rebuildList();
                }
            );

            root.Add(ScovySignature.CreateSignatureBox());

            return root;
        }
    }
}
