using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Assets.Tools.Folder
{
    /// <summary>
    /// Editor tùy biến cho asset FolderIconSettings.
    /// Khi bạn click chọn file "Folder Icon Manager.asset" trong Project Window,
    /// Unity sẽ dùng class này để vẽ Inspector thay vì vẽ mặc định.
    ///
    /// Inspector gồm 3 phần:
    /// 1. Cờ bật/tắt (Show Folder / Show Overlay)
    /// 2. Danh sách các thư mục đã gán icon (có thể kéo thả, xóa, thêm)
    /// 3. Công cụ đổi màu + lưu texture thành file PNG (để làm icon mới)
    /// </summary>
    [CustomEditor(typeof(FolderIconSettings))]
    internal class FolderIconSettingsEditor : Editor
    {
        // ========================================================
        // THAM CHIẾU (reference) tới dữ liệu đang được chỉnh sửa
        // ========================================================
        private FolderIconSettings settings; // Asset đang được Inspector chỉnh
        private SerializedProperty serializedIcons; // Ô "icons" trong asset, dùng để vẽ danh sách

        // ========================================================
        // TRẠNG THÁI UI (giá trị tạm trong lúc chỉnh, chưa lưu ngay)
        // ========================================================
        private bool showCustomFolders; // Cờ "bật icon thư mục tùy chỉnh"
        private bool showCustomOverlay; // Cờ "bật icon chồng (overlay)"

        // Danh sách có thể kéo thả, thêm bớt của Unity (ReorderableList)
        private ReorderableList iconList;

        // ========================================================
        // PHẦN CÔNG CỤ ĐỔI MÀU TEXTURE (làm icon mới từ icon cũ)
        // ========================================================
        private Texture2D selectedTexture = null; // Texture gốc người dùng chọn
        private Texture2D previewTexture = null; // Texture sau khi đổi màu (bản preview)
        private RenderTexture previewRender; // Vùng nhớ tạm để render preview

        private GUIContent previewContent = new GUIContent(); // Hiển thị previewTexture trong label
        private Color replacementColour = Color.gray; // Màu người dùng chọn để thay thế

        // Thông tin khi lưu texture thành file
        private string textureName = "New Texture"; // Tên file .png
        private string savePath; // Thư mục sẽ lưu file .png

        // ========================================================
        // KÍCH THƯỚC & CĂN CHỈNH (dùng khi vẽ danh sách icon)
        // ========================================================
        private const float MAX_LABEL_WIDTH = 90f; // Giới hạn độ rộng nhãn (tên field)
        private const float MAX_FIELD_WIDTH = 150f; // Giới hạn độ rộng ô nhập dữ liệu

        private const float PROPERTY_HEIGHT = 19f; // Chiều cao mỗi dòng field
        private const float PROPERTY_PADDING = 4f; // Khoảng cách giữa các dòng

        // ========================================================
        // STYLE (cách hiển thị: màu nền, viền...)
        // ========================================================
        private GUIStyle elementStyle; // Nền của từng mục trong danh sách
        private GUIStyle previewStyle; // Căn giữa ảnh preview texture

        private void OnEnable()
        {
            // Unity gọi hàm này mỗi khi Inspector của asset này được mở
            if (target == null)
            {
                return;
            }

            // Lấy asset đang được chỉnh và ô "icons" trong nó
            settings = target as FolderIconSettings;
            serializedIcons = serializedObject.FindProperty("icons");

            // Đọc giá trị hiện tại trong asset để hiển thị lên toggle
            showCustomFolders = settings.showCustomFolders;
            showCustomOverlay = settings.showOverlay;

            // Tạo danh sách icon có thể kéo thả (chỉ tạo 1 lần)
            if (iconList == null)
            {
                iconList = new ReorderableList(serializedObject, serializedIcons)
                {
                    // Hàm vẽ tiêu đề, từng mục và nền của mục
                    drawHeaderCallback = OnHeaderDraw,
                    drawElementCallback = OnElementDraw,
                    drawElementBackgroundCallback = DrawElementBackground,

                    // Hàm tính chiều cao của từng mục
                    elementHeightCallback = GetPropertyHeight,

                    // Không dùng nền mặc định của Unity, tự vẽ nền riêng
                    showDefaultBackground = false,
                };
            }

            // Mặc định chỗ lưu texture là thư mục Assets của dự án
            savePath = Application.dataPath;

            if (selectedTexture != null)
            {
                UpdatePreview();
            }
        }

        private void OnDisable()
        {
            // Dọn vùng nhớ tạm khi đóng Inspector
            ClearPreviewData();
        }

        /// <summary>
        /// Hàm chính: vẽ toàn bộ giao diện Inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Tạo style lần đầu (GUIStyle không nên tạo trong OnEnable)
            if (previewStyle == null)
            {
                previewStyle = new GUIStyle(EditorStyles.label)
                {
                    fixedHeight = 64,
                    alignment = TextAnchor.MiddleCenter,
                };

                elementStyle = new GUIStyle(GUI.skin.box) { };
            }

            // ======================
            // PHẦN 1: CỜ BẬT/TẮT
            // ======================
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            {
                showCustomFolders = EditorGUILayout.ToggleLeft(
                    "Show Folder Textures",
                    showCustomFolders
                );
                showCustomOverlay = EditorGUILayout.ToggleLeft(
                    "Show Overlay Textures",
                    showCustomOverlay
                );
            }
            if (EditorGUI.EndChangeCheck())
            {
                // Người dùng vừa thay đổi toggle -> lưu vào asset + cập nhật cache
                ApplySettings();
            }

            EditorGUILayout.Space(16f);

            // ======================
            // PHẦN 2: DANH SÁCH ICON
            // ======================
            EditorGUI.BeginChangeCheck();
            iconList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                // Người dùng vừa sửa danh sách (thêm/xóa/đổi icon) -> ghi lại
                serializedObject.ApplyModifiedProperties();
            }

            // ======================
            // PHẦN 3: CÔNG CỤ ĐỔI MÀU + LƯU TEXTURE
            // ======================
            DrawTexturePreview();

            // Chỉ cho phép đổi màu / lưu khi đã có texture preview
            EditorGUI.BeginDisabledGroup(previewTexture == null);
            {
                EditorGUI.BeginChangeCheck();
                {
                    replacementColour = EditorGUILayout.ColorField(
                        new GUIContent("Replacement Colour"),
                        replacementColour
                    );
                }
                if (EditorGUI.EndChangeCheck())
                {
                    // Đổi màu từng pixel của preview texture
                    SetPreviewColour();
                }

                DrawTextureSaving();
            }
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Ghi 2 cờ bật/tắt vào asset và yêu cầu cache nạp lại.
        /// Phải gọi RefreshCache vì FolderIconReplacer đọc cờ từ
        /// FolderIconRegistry.ActiveSettings chứ không đọc trực tiếp asset.
        /// </summary>
        private void ApplySettings()
        {
            settings.showCustomFolders = showCustomFolders;
            settings.showOverlay = showCustomOverlay;

            // Đánh dấu asset bị thay đổi và lưu xuống đĩa (không thì mất khi thoát)
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            // Nạp lại cache để icon vẽ ngay với cấu hình mới
            FolderIconRegistry.RefreshCache();
        }

        #region Vẽ danh sách icon (ReorderableList)

        /// <summary>Vẽ tiêu đề "Folders" ở đầu danh sách</summary>
        private void OnHeaderDraw(Rect rect)
        {
            rect.y += 5f;
            rect.x -= 6f;
            rect.width += 12f;

            // Vẽ nền tối cho dòng tiêu đề
            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(
                rect,
                new Color(0.15f, 0.15f, 0.15f, 1f),
                new Color(0.15f, 0.15f, 0.15f, 1f)
            );
            Handles.EndGUI();

            EditorGUI.LabelField(rect, "Folders", EditorStyles.boldLabel);
        }

        /// <summary>
        /// Vẽ từng mục trong danh sách:
        /// bên trái là các field (folder, folderIcon, overlayIcon),
        /// bên phải là ảnh preview ghép icon + overlay.
        /// </summary>
        private void OnElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty property = serializedIcons.GetArrayElementAtIndex(index);

            float fullWidth = rect.width;

            // Giới hạn độ rộng phần field để chừa chỗ cho phần preview bên phải
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            float rectWidth = MAX_LABEL_WIDTH + MAX_FIELD_WIDTH;
            EditorGUIUtility.labelWidth = Mathf.Min(EditorGUIUtility.labelWidth, MAX_LABEL_WIDTH);
            rect.width = Mathf.Min(rect.width, rectWidth);

            // Vẽ 3 field: folder, folderIcon, overlayIcon
            DrawPropertyNoDepth(rect, property);

            // Phần còn lại bên phải dùng để vẽ ảnh preview
            rect.x += rect.width;
            rect.width = fullWidth - rect.width;

            // Lấy 2 texture của mục này để vẽ preview
            SerializedProperty folderTexture = property.FindPropertyRelative("folderIcon");
            SerializedProperty overlayTexture = property.FindPropertyRelative("overlayIcon");

            Object folderObject = folderTexture.objectReferenceValue;
            Object overlayObject = overlayTexture.objectReferenceValue;

            // Vẽ preview: icon chính + overlay ở góc
            FolderIconGUI.DrawFolderPreview(
                rect,
                folderObject as Texture,
                overlayObject as Texture
            );

            // Trả lại độ rộng nhãn như cũ để các phần UI khác không bị lệch
            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        /// <summary>
        /// Vẽ toàn bộ field con của một mục (folder, folderIcon, overlayIcon)
        /// theo chiều dọc, mỗi field một dòng.
        /// </summary>
        private void DrawPropertyNoDepth(Rect rect, SerializedProperty property)
        {
            // Vẽ viền bao quanh cụm field
            rect.width++;
            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(
                rect,
                Color.clear,
                new Color(0.15f, 0.15f, 0.15f, 1f)
            );
            Handles.EndGUI();

            rect.x++;
            rect.width -= 3;
            rect.y += PROPERTY_PADDING;
            rect.height = PROPERTY_HEIGHT;

            // Lặp qua từng field con (dùng Copy + Next để không làm hỏng con trỏ property gốc)
            SerializedProperty copy = property.Copy();
            bool enterChildren = true;

            while (copy.Next(enterChildren))
            {
                if (SerializedProperty.EqualContents(copy, property.GetEndProperty()))
                {
                    break; // Hết field con
                }

                EditorGUI.PropertyField(rect, copy, false);
                rect.y += PROPERTY_HEIGHT + PROPERTY_PADDING;

                enterChildren = false;
            }
        }

        /// <summary>
        /// Vẽ nền cho từng mục trong danh sách.
        /// Mục đang chọn (focus) sẽ có nền xanh.
        /// </summary>
        private void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.LabelField(rect, "", elementStyle);

            Color fill = isFocused ? FolderIconConstants.SelectedColor : Color.clear;

            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(rect, fill, new Color(0.15f, 0.15f, 0.15f, 1f));
            Handles.EndGUI();
        }

        /// <summary>
        /// Tính chiều cao cần thiết cho mỗi mục trong danh sách.
        /// Mỗi mục có đúng 3 field -> chiều cao = 3 dòng + khoảng cách.
        /// </summary>
        private float GetPropertyHeight(SerializedProperty property)
        {
            return (PROPERTY_HEIGHT + PROPERTY_PADDING) * 3 + PROPERTY_PADDING;
        }

        /// <summary>
        /// Hàm cầu nối cho ReorderableList (nó gọi theo index, ta chuyển thành property)
        /// </summary>
        private float GetPropertyHeight(int index)
        {
            return GetPropertyHeight(serializedIcons.GetArrayElementAtIndex(index));
        }

        #endregion

        #region Công cụ đổi màu texture

        /// <summary>
        /// Vẽ khu vực chọn texture gốc và xem trước texture đã đổi màu
        /// </summary>
        private void DrawTexturePreview()
        {
            EditorGUILayout.LabelField("Texture Colour Replacement", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            {
                // Dòng tiêu đề 2 cột
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Original Texture");
                EditorGUILayout.LabelField("Modified Texture");
                EditorGUILayout.EndHorizontal();

                // Dòng nội dung: ô chọn texture gốc + ảnh preview
                EditorGUILayout.BeginHorizontal();
                {
                    selectedTexture =
                        EditorGUILayout.ObjectField(
                            selectedTexture,
                            typeof(Texture2D),
                            false,
                            GUILayout.ExpandWidth(false),
                            GUILayout.ExpandHeight(true),
                            GUILayout.Width(64f)
                        ) as Texture2D;

                    EditorGUILayout.LabelField(previewContent, previewStyle, GUILayout.Height(64));
                }
                EditorGUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck())
            {
                // Người dùng chọn/xóa texture gốc
                if (selectedTexture == null)
                {
                    ClearPreviewData();
                    previewTexture = null;
                    return;
                }

                UpdatePreview();
            }

            EditorGUILayout.Space();
        }

        /// <summary>
        /// Vẽ phần nhập tên file, chọn thư mục và nút lưu texture thành .png
        /// </summary>
        private void DrawTextureSaving()
        {
            EditorGUILayout.LabelField("Save Created Texture", EditorStyles.boldLabel);

            // Nhập tên file
            GUILayout.BeginHorizontal();
            {
                textureName = EditorGUILayout.TextField("Texture Name", textureName);

                // Phần ".png" cố định, không cho sửa
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.TextField(".png", GUILayout.Width(40f));
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();

            // Nhập / chọn thư mục lưu
            GUILayout.BeginHorizontal();
            {
                savePath = EditorGUILayout.TextField("Save Path", savePath);

                if (GUILayout.Button("Select", GUILayout.MaxWidth(80f)))
                {
                    savePath = EditorUtility.OpenFolderPanel("Texture Save Path", "Assets", "");
                    GUIUtility.ExitGUI();
                }
            }
            GUILayout.EndHorizontal();

            // Nút lưu
            if (GUILayout.Button("Save Texture"))
            {
                string fullPath = $"{savePath}/{textureName}.png";
                SaveTextureAsPNG(previewTexture, fullPath);
            }
        }

        /// <summary>
        /// Thay màu toàn bộ pixel của preview texture bằng replacementColour
        /// (giữ nguyên độ trong suốt - alpha)
        /// </summary>
        private void SetPreviewColour()
        {
            for (int x = 0; x < previewTexture.width; x++)
            {
                for (int y = 0; y < previewTexture.height; y++)
                {
                    Color oldCol = previewTexture.GetPixel(x, y);
                    Color newCol = replacementColour;
                    newCol.a = oldCol.a; // Giữ alpha cũ để không làm mất độ trong suốt

                    previewTexture.SetPixel(x, y, newCol);
                }
            }

            previewTexture.Apply();
        }

        /// <summary>
        /// Ghi texture thành file .png và cấu hình lại TextureImporter
        /// để Unity hiểu đây là ảnh giao diện (GUI) có thể đọc pixel.
        /// </summary>
        private void SaveTextureAsPNG(Texture2D texture, string path)
        {
            // Bảo vệ: chỉ cho lưu bên trong thư mục Assets của dự án
            if (string.IsNullOrWhiteSpace(path) || !path.Contains("Assets"))
            {
                Debug.LogWarning("Cannot save texture to invalid path.");
                return;
            }

            // Ghi file ảnh ra đĩa
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            // Bảo Unity quét lại thư mục để nhận file mới
            AssetDatabase.Refresh();

            // Đổi đường dẫn tuyệt đối (C:\...\Assets\...) thành đường dẫn tương đối (Assets\...)
            int localPathIndex = path.IndexOf("Assets");
            path = path.Substring(localPathIndex, path.Length - localPathIndex);

            // Cấu hình importer: ảnh GUI + cho phép đọc pixel (cần để SetPreviewColour hoạt động)
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType = TextureImporterType.GUI;
            importer.isReadable = true;

            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Giải phóng vùng nhớ RenderTexture tạm (tránh rò rỉ bộ nhớ GPU)
        /// </summary>
        private void ClearPreviewData()
        {
            if (previewRender != null)
            {
                previewRender.Release();
            }
        }

        /// <summary>
        /// Tạo bản preview (giới hạn 256x256 cho nhẹ) và đổi màu theo màu đang chọn
        /// </summary>
        private void UpdatePreview()
        {
            ClearPreviewData();

            // Không cần texture quá lớn cho preview -> giới hạn 256px để chạy nhanh
            int width = Mathf.Min(256, selectedTexture.width);
            int height = Mathf.Min(256, selectedTexture.height);

            // RenderTexture là bước đệm: vẽ texture gốc vào đây rồi mới đọc pixel ra
            previewRender = new RenderTexture(width, height, 16);
            previewTexture = new Texture2D(previewRender.width, previewRender.height)
            {
                alphaIsTransparency = true,
            };
            previewContent.image = previewTexture;

            // Sao chép nội dung texture gốc vào vùng render tạm
            Graphics.Blit(selectedTexture, previewRender);

            // Đọc pixel từ RenderTexture ra Texture2D để có thể chỉnh từng pixel
            previewTexture.ReadPixels(
                new Rect(0, 0, previewRender.width, previewRender.height),
                0,
                0
            );
            SetPreviewColour();
        }

        #endregion
    }
}
