using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Tools
{
    public static class ScovySignature
    {
        public static VisualElement CreateSignatureBox()
        {
            VisualElement signatureBox = new VisualElement();
            signatureBox.style.marginTop = 25;
            signatureBox.style.paddingTop = 15;
            signatureBox.style.paddingBottom = 10;
            signatureBox.style.borderTopWidth = 1;
            signatureBox.style.borderTopColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
            signatureBox.style.alignItems = Align.Center;

            Label creatorLabel = new Label("Created by Scovy");
            creatorLabel.style.fontSize = 14;
            creatorLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            creatorLabel.style.color = new StyleColor(new Color(0.4f, 0.8f, 1f));
            signatureBox.Add(creatorLabel);

            // Load ascii art from catgirl.txt
            string asciiText = "  /\\_/\\  \n ( o.o ) \n  > ^ <  "; // Fallback

            // Search for the text asset anywhere in the project
            string[] guids = AssetDatabase.FindAssets("catgirl t:TextAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                TextAsset txtAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (txtAsset != null)
                {
                    asciiText = txtAsset.text;
                }
            }
            else
            {
                // Fallback attempt to read directly if Unity hasn't indexed it yet
                string directPath = Path.Combine(Application.dataPath, "Editors/catgirl.txt");
                if (File.Exists(directPath))
                {
                    asciiText = File.ReadAllText(directPath);
                }
            }

            // Prevent Unity from stripping leading spaces by replacing them with non-breaking spaces
            asciiText = asciiText.Replace(" ", "\u00A0");

            Label asciiArt = new Label(asciiText);
            asciiArt.style.whiteSpace = WhiteSpace.Pre;
            asciiArt.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f));
            asciiArt.style.marginTop = 5;
            asciiArt.style.unityTextAlign = TextAnchor.MiddleCenter;

            // Increased font size based on feedback
            asciiArt.style.fontSize = 10;

            // Attempt to load an OS system font that supports Braille characters (like MS Gothic or Segoe UI Symbol)
            Font osFont = Font.CreateDynamicFontFromOSFont(
                new string[] { "MS Gothic", "Segoe UI Symbol", "Consolas", "Arial" },
                10
            );
            if (osFont != null)
            {
                // In newer Unity versions unityFontDefinition is preferred
                asciiArt.style.unityFontDefinition = new StyleFontDefinition(osFont);
            }

            // Also add a scrollview wrapper just in case it's still too large
            ScrollView scrollWrapper = new ScrollView();
            scrollWrapper.style.maxHeight = 250; // Increased height to accommodate larger font
            scrollWrapper.style.marginTop = 5;
            scrollWrapper.contentContainer.style.alignItems = Align.Center;
            scrollWrapper.Add(asciiArt);

            signatureBox.Add(scrollWrapper);

            return signatureBox;
        }
    }
}
