using UnityEditor;
using UnityEngine;

namespace Assets.Tools.Folder
{
    [InitializeOnLoad]
    public static class FolderIconReplacer
    {
        static FolderIconReplacer()
        {
            // Nạp dữ liệu Cache ban đầu vào Dictionary O(1)
            FolderIconRegistry.RefreshCache();

            // Đăng ký callback vẽ giao diện Project Window
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;

            // Xếp lại thứ tự vẽ: callback của ta phải chạy ĐẦU TIÊN (vẽ ở lớp dưới cùng),
            // để các plugin khác (vFolders) vẽ ĐÈ LÊN trên icon của mình.
            // Phải đợi delayCall vì lúc này các plugin khác chưa chắc đã đăng ký xong.
            EditorApplication.delayCall += MoveCallbackToDrawFirst;
        }

        /// <summary>
        /// Đẩy callback của FolderIcon lên vị trí ĐẦU danh sách gọi.
        /// Quy tắc: ai vẽ trước thì nằm DƯỚI, ai vẽ sau thì nằm TRÊN.
        /// vFolders cố tình chèn callback của nó vào đầu danh sách để vẽ đè,
        /// nên ta cũng phải chen vào đầu thì nó mới vẽ đè lên được icon của ta.
        /// </summary>
        private static void MoveCallbackToDrawFirst()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
            EditorApplication.projectWindowItemOnGUI = OnProjectWindowItemGUI + EditorApplication.projectWindowItemOnGUI;
        }

        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            if (string.IsNullOrEmpty(guid))
                return;

            // 1. Kiểm tra cờ bật/tắt Global từ ActiveSettings
            var settings = FolderIconRegistry.ActiveSettings;
            if (settings == null)
                return;

            // Nếu cả 2 tùy chọn hiển thị đều tắt thì ngưng vẽ
            if (!settings.showCustomFolders && !settings.showOverlay)
                return;

            // 2. Tra cứu O(1) từ Dictionary theo GUID
            if (FolderIconRegistry.TryGetIcon(guid, out var iconData))
            {
                // 3. Chuyển sang Renderer để tính Rect, xóa nền và vẽ Icon
                FolderIconRenderer.DrawFolderIcon(selectionRect, iconData, settings);
            }
        }
    }
}
