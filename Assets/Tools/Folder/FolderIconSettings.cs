using System;
using UnityEditor; // UnityEditor is for editing tools and scripts in the Unity Editor
using UnityEngine; // UnityEngine is the core namespace for Unity

namespace Assets.Tools.Folder
{
    [CreateAssetMenu(
        fileName = FolderIconConstants.MenuFileName, // Name of the asset file when created
        menuName = FolderIconConstants.MenuPath // Location in the Unity Create Asset Menu
    )]
    public class FolderIconSettings : ScriptableObject
    {
        [Serializable] // This attribute allows the FolderIcon class to be serialized and displayed in the Unity Inspector
        public class FolderIcon
        {
            public DefaultAsset folder; // Thư mục cần đổi icon (kiểu DefaultAsset đại diện cho một thư mục trong Unity)
            public Texture2D folderIcon; // Icon để thay đế icon mặc định của thử mục
            public Texture2D overlayIcon; // Icon phụ để chồng bên (badge)
        }

        public bool showOverlay = true;
        public bool showCustomFolders = true;
        public FolderIcon[] icons = new FolderIcon[0];
    }
}
