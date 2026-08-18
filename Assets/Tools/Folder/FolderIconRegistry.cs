using System.Collections.Generic;
using UnityEditor;

namespace Assets.Tools.Folder
{
    public static class FolderIconRegistry
    {
        // Dictionary tra cứu O(1) với Key là GUID của Folder
        private static readonly Dictionary<string, FolderIconSettings.FolderIcon> folderIconMap =
            new Dictionary<string, FolderIconSettings.FolderIcon>();

        // Lưu giữ reference đến file Settings hiện tại để đọc các cờ global (showOverlay, showCustomFolders)
        public static FolderIconSettings ActiveSettings { get; private set; }

        public static void RefreshCache()
        {
            folderIconMap.Clear();
            ActiveSettings = null;

            // Tìm tất cả các file cấu hình FolderIconSettings trong dự án
            string[] guids = AssetDatabase.FindAssets("t:FolderIconSettings");

            if (guids.Length == 0)
                return;

            // Lấy file cấu hình đầu tiên tìm thấy
            string settingsPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            ActiveSettings = AssetDatabase.LoadAssetAtPath<FolderIconSettings>(settingsPath);

            if (ActiveSettings == null || ActiveSettings.icons == null)
                return;

            // Duyệt qua từng ô cấu hình icon
            foreach (var iconData in ActiveSettings.icons)
            {
                if (iconData == null || iconData.folder == null)
                    continue;

                // Lấy đường dẫn và đổi sang GUID duy nhất của chính folder đó
                string folderPath = AssetDatabase.GetAssetPath(iconData.folder);
                string folderGuid = AssetDatabase.AssetPathToGUID(folderPath);

                if (string.IsNullOrEmpty(folderGuid))
                    continue;

                // Lưu vào Dictionary theo GUID thay vì tên folder
                if (!folderIconMap.ContainsKey(folderGuid))
                {
                    folderIconMap.Add(folderGuid, iconData);
                }
            }
        }

        /// <summary>
        /// Hàm tra cứu nhanh O(1) dùng cho GUI Loop
        /// </summary>
        public static bool TryGetIcon(string folderGuid, out FolderIconSettings.FolderIcon iconData)
        {
            // Tự động nạp cache nếu Dictionary đang trống (ví dụ sau khi Recompile)
            if (folderIconMap.Count == 0 && ActiveSettings == null)
            {
                RefreshCache();
            }

            return folderIconMap.TryGetValue(folderGuid, out iconData);
        }
    }
}
