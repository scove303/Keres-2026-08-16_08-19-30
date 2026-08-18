using System;
using UnityEditor; // UnityEditor is for editing tools and scripts in the Unity Editor
using UnityEngine; // UnityEngine is the core namespace for Unity

namespace Assets.Tools.Folder
{
    [InitializeOnLoad] // This attribute ensures that the static constructor is called when the Unity Editor loads
    static class FolderIconReplacer
    {
        private static UnityEngine.Object[] allFolderIcons;
        private static FolderIconSettings settings;
        public static bool showFolder;
        public static bool showOverlay;

        static void FolderIcon()
        {
            CheckPreferences();

            // EditorApplication.projectWindowItemOnGUI -= ReplaceFolders;
            // EditorApplication.projectWindowItemOnGUI += ReplaceFolders;
        }

        private static void CheckPreferences()
        {
            showFolder = EditorPrefs.GetBool("FolderIcon.showFolder", true);
            showOverlay = EditorPrefs.GetBool("FolderIcon.showOverlay", true);
        }

        private static T[] GetAllInstances<T>()
            where T : UnityEngine.Object
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
    }
}
