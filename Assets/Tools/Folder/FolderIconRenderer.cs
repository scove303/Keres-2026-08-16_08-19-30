using UnityEditor;
using UnityEngine;

namespace Assets.Tools.Folder
{
    public static class FolderIconRenderer
    {
        // Màu nền chuẩn Unity Editor theo Theme
        private static readonly Color DarkThemeBg = new Color(0.22f, 0.22f, 0.22f, 0f);
        private static readonly Color LightThemeBg = new Color(0.78f, 0.78f, 0.78f, 1f);

        public static void DrawFolderIcon(
            Rect selectionRect,
            FolderIconSettings.FolderIcon iconData,
            FolderIconSettings settings
        )
        {
            bool isTreeView = selectionRect.width > selectionRect.height;
            Rect iconRect;

            if (isTreeView)
            {
                // ----------------------------------------------------
                // Case 1: TreeView (Danh sách cây / Panel bên trái)
                // ----------------------------------------------------
                // Kích thước chuẩn của icon nhỏ trong TreeView là 16x16
                float iconSize = 16f;

                // Kiểm tra xem có đang ở SideView (bên trái) hay ListView (bên phải)
                bool isSideView = IsSideView(selectionRect);
                float xOffset = isSideView ? 0f : 0.5f; // Khôi phục Offset +2.5f từ code cũ nếu ở ListView

                iconRect = new Rect(
                    selectionRect.x + xOffset,
                    selectionRect.y + (selectionRect.height - iconSize) / 2f,
                    iconSize,
                    iconSize
                );
            }
            else
            {
                // ----------------------------------------------------
                // Case 2: GridView (Dạng các ô vuông lớn bên phải)
                // ----------------------------------------------------
                // Trừ 14px chiều cao dành cho Nhãn chữ (Name) bên dưới
                float size = selectionRect.width;
                float height = selectionRect.height - 14f;

                iconRect = new Rect(selectionRect.x, selectionRect.y, size, height);
            }

            // ----------------------------------------------------
            // FIX LỖI 1: Xóa/Che hoàn toàn Icon mặc định của Unity
            // ----------------------------------------------------
            Color bgColor = EditorGUIUtility.isProSkin ? DarkThemeBg : LightThemeBg;
            EditorGUI.DrawRect(iconRect, bgColor);

            // ----------------------------------------------------
            // FIX LỖI 2 & 3: Vẽ Icon với ScaleMode.ScaleToFit (Chống Stretch)
            // ----------------------------------------------------
            // 1. Vẽ Custom Folder Icon nền
            if (settings.showCustomFolders && iconData.folderIcon != null)
            {
                GUI.DrawTexture(iconRect, iconData.folderIcon, ScaleMode.ScaleToFit);
            }

            // 2. Vẽ Overlay Icon đè lên trên
            if (settings.showOverlay && iconData.overlayIcon != null)
            {
                GUI.DrawTexture(iconRect, iconData.overlayIcon, ScaleMode.ScaleToFit);
            }
        }

        // Nhận diện cột SideView bên trái
        private static bool IsSideView(Rect rect)
        {
            return rect.x < 16f;
        }
    }
}
