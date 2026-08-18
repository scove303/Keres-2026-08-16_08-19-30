using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tools
{
    public class CameraJumpPoints : MonoBehaviour
    {
        [System.Serializable]
        public class JumpPoint
        {
            public string name = "New Point";
            public Transform target;
            public KeyCode key = KeyCode.None;
            public bool ctrl;
            public bool alt;
            public bool shift;

            public string GetKeyString()
            {
                if (key == KeyCode.None)
                    return "None";
                string s = "";
                if (ctrl)
                    s += "Ctrl+";
                if (alt)
                    s += "Alt+";
                if (shift)
                    s += "Shift+";
                s += key.ToString();
                return s;
            }
        }

        [Tooltip(
            "List of camera bookmarks. Add a point, assign a target transform, and click the shortcut button to record a key binding."
        )]
        public List<JumpPoint> points = new List<JumpPoint>();
    }
}
