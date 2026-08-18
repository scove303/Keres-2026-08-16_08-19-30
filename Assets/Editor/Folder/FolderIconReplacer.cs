using System;
using UnityEditor; // UnityEditor is for editing tools and scripts in the Unity Editor
using UnityEngine; // UnityEngine is the core namespace for Unity

namespace Assets.Editor.Folder
{
    [InitializeOnLoad] // This attribute ensures that the static constructor is called when the Unity Editor loads
    static class FolderIconReplacer
    {
        private static UnityEngine.Object[] allFolderIcons;
        private static FolderIconSettings settings;
        public static bool showFolder;
        public static bool showOverlay;

        static FolderIcon()
        {
            CheckPreferences();

            EditorApplication.projectWindowItemOnGUI -= ReplaceFolders;
            EditorApplication.projectWindowItemOnGUI += ReplaceFolders;
        }

        private static void CheckPreferences()
        {
            showFolder = EditorPrefs.GetBool("FolderIcon.showFolder", true);
            showOverlay = EditorPrefs.GetBool("FolderIcon.showOverlay", true);
        }
 
        private static T[] GetAllInstances<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            T[] instances = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                instances[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return instances;
        }

        private static void ReplaceFolders(string guid, Rect selectionRect)
        {
            if (settings == null)
            {
                allFolderIcons = GetAllInstances<FolderIconSettings>();

                if (allFolderIcons.Length > 0)
                {
                    settings = allFolderIcons[0] as FolderIconSettings;
                }
                else
                {
                    FolderIconSettings setting =
                        ScriptableObject.CreateInstance<FolderIconSettings>();
                    AssetDatabase.CreateAsset(setting, "Assets/Presets/FolderIconSettings.asset");

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    settings =
                        AssetDatabase.LoadAssetAtPath(
                            "Assets/Presets/FolderIconSettings.asset",
                            typeof(FolderIconSettings)
                        ) as FolderIconSettings;
                }
            }
            if (settings == null)
            {
                return;
            }

            if (!settings.showCustomFolders && !settings.showOverlay)
            {
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guid);
            UnityEngine.Object folderAsset = AssetDatabase.LoadAssetAtPath(path, typeof(DefaultAsset));

            if (folderAsset == null)
            {
                return;
            }

            for (int i = 0; i < settings.icons.Length; i++)
            {
                FolderIconSettings.FolderIcon icon = settings.icons[i];

                if (icon.folder != folderAsset)
                {
                    continue;
                }

                DrawTextures(selectionRect, icon, folderAsset, guid);
            }
        }

        private static void DrawTextures(Rect rect, FolderIconSettings.FolderIcon icon, Object folderAsset, string guid)
        {
            bool isTreeView = rect.width > rect.height;
            bool isSideView = FolderIconGUI.IsSideView(rect);

            if (isTreeView)
            {
                rect.with
            }
        }
    }
}
