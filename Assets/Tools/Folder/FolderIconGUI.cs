using UnityEditor;
using UnityEngine;

namespace Assets.Tools.Folder
{
    /// <summary>
    /// Helper vẽ các thành phần GUI dùng chung cho Folder Icon system
    /// </summary>
    public static class FolderIconGUI
    {
        /// <summary>
        /// Vẽ preview của một ô icon: icon chính ở giữa, overlay ở góc dưới phải.
        /// Nếu không có icon chính, hiển thị icon thư mục mặc định của Editor.
        /// </summary>
        public static void DrawFolderPreview(
            Rect rect,
            Texture folderTexture,
            Texture overlayTexture
        )
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (folderTexture != null)
            {
                GUI.DrawTexture(rect, folderTexture);
            }
            else
            {
                Texture defaultFolder =
                    EditorGUIUtility.IconContent("Folder Icon").image
                    ?? EditorGUIUtility.IconContent("Folder").image;
                GUI.DrawTexture(rect, defaultFolder);
            }

            if (overlayTexture != null)
            {
                float overlaySize = Mathf.Min(rect.width, rect.height) * 0.5f;
                Rect overlayRect = new Rect(
                    rect.xMax - overlaySize,
                    rect.yMax - overlaySize,
                    overlaySize,
                    overlaySize
                );
                GUI.DrawTexture(overlayRect, overlayTexture);
            }
        }
    }
}
