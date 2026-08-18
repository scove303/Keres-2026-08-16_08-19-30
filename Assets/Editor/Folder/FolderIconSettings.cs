using System;
using UnityEditor;  // UnityEditor is for editing tools and scripts in the Unity Editor
using UnityEngine;  // UnityEngine is the core namespace for Unity

namespace Assets.Editor.Folder
{
    [CreateAssetMenu(fileName = "Folder Icon Settings", menuName = "UnityGUI/FolderIconSettings", order = 1)]
    public class FolderIconSettings : ScriptableObject
    {
        [Serializable] public class FolderIcon
        {
            public DefaultAsset folder;
            public Texture2D folderIcon;
            public Texture2D overlayIcon;
        }

        public bool showOverlay = true;
        public bool showCustomFolders = true;
        public FolderIcon[] icons = new FolderIcon[0];
    }
}