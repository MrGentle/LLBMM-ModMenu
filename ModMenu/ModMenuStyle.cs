using System.Collections.Generic;
using UnityEngine;

namespace ModMenu
{
    public static class ModMenuStyle
    {

        static Dictionary<string, Texture2D> texColors = new Dictionary<string, Texture2D>();

        public static void InitStyle()
        {
            texColors.Clear();
            texColors.Add("Yellow", ColorToTexture2D(new Color(1f, 0.968f, 0.3f)));
            texColors.Add("LightYellow", ColorToTexture2D(new Color(1f, 1f, 0.5f)));
            texColors.Add("DarkGray", ColorToTexture2D(new Color(0.145f, 0.145f, 0.145f)));
            texColors.Add("LightGray", ColorToTexture2D(new Color(0.5f, 0.5f, 0.5f)));
            texColors.Add("Black", ColorToTexture2D(new Color32(12, 12, 12, 255)));
            texColors.Add("White", ColorToTexture2D(new Color32(255, 255, 255, 255)));
        }


        public static GUIStyle mainStyle
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle()
                {
                    padding = new RectOffset(4, 4, 4, 4),
                };
                gUIStyle.normal.background = texColors["Black"];
                return gUIStyle;
            }
        }

        public static GUIStyle labStyle
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                };
                gUIStyle.normal.textColor = new Color(1f, 0.968f, 0.3f);
                return gUIStyle;
            }
        }

        public static GUIStyle windStyle
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle()
                {
                    padding = new RectOffset(4, 4, 4, 4),
                    alignment = TextAnchor.UpperCenter,
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                };
                gUIStyle.normal.background = texColors["Black"];
                gUIStyle.normal.textColor = new Color(1f, 0.968f, 0.3f);
                return gUIStyle;
            }
        }

        public static GUIStyle border = new GUIStyle() { padding = new RectOffset(10, 10, 24, 10), fontSize = 136, };

        public static GUIStyle button
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(4, 4, 4, 4),
                };

                gUIStyle.normal.textColor = Color.black;
                gUIStyle.hover.textColor = Color.black;
                gUIStyle.active.textColor = Color.white;

                gUIStyle.normal.background = texColors["Yellow"];
                gUIStyle.hover.background = texColors["LightYellow"];

                return gUIStyle;
            }
        }

        public static GUIStyle box
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(4, 4, 4, 4),
                };

                gUIStyle.normal.textColor = Color.black;
                gUIStyle.hover.textColor = Color.black;
                gUIStyle.active.textColor = Color.white;

                gUIStyle.normal.background = texColors["Yellow"];
                gUIStyle.hover.background = texColors["Yellow"];

                return gUIStyle;
            }
        }

        public static GUIStyle headerBox
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(4, 4, 4, 4),
                };

                gUIStyle.normal.textColor = Color.black;
                gUIStyle.hover.textColor = Color.black;
                gUIStyle.active.textColor = Color.white;

                gUIStyle.normal.background = texColors["Yellow"];
                gUIStyle.hover.background = texColors["Yellow"];

                return gUIStyle;
            }
        }

        public static GUIStyle versionBox
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(4, 4, 4, 4),
                };

                gUIStyle.normal.textColor = new Color(1f, 0.968f, 0.3f);
                gUIStyle.hover.textColor = new Color(1f, 0.968f, 0.3f);
                gUIStyle.active.textColor = new Color(1f, 0.968f, 0.3f);

                gUIStyle.normal.background = texColors["Black"];
                gUIStyle.hover.background = texColors["Black"];

                return gUIStyle;
            }
        }

        public static GUIStyle _sliderThumbStyle
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle()
                {
                    fontSize = 24,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                };
                gUIStyle.stretchHeight = true;
                gUIStyle.fixedWidth = 40;
                gUIStyle.normal.background = texColors["Black"];
                gUIStyle.hover.background = texColors["Black"];
                gUIStyle.active.background = texColors["LightGray"];
                return gUIStyle;
            }

        }

        public static GUIStyle _sliderBackgroundStyle
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle()
                {
                    stretchWidth = true,
                    padding = new RectOffset(2, 2, 2, 2),
                };
                gUIStyle.margin = new RectOffset(0, 0, 7, 0);
                gUIStyle.normal.background = texColors["Yellow"];
                gUIStyle.hover.background = texColors["Yellow"];
                gUIStyle.active.background = texColors["Yellow"];
                gUIStyle.focused.background = texColors["Yellow"];
                return gUIStyle;
            }
        }

        public static GUIStyle _textFieldStyle
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle()
                {
                    fontSize = 16,
                    padding = new RectOffset(4, 4, 4, 4),
                };
                gUIStyle.margin = new RectOffset(0, 0, 5, 0);
                gUIStyle.normal.textColor = new Color(1f, 0.968f, 0.3f);
                gUIStyle.normal.background = texColors["Black"];

                return gUIStyle;
            }
        }

        public static GUIStyle readStyle
        {
            get
            {
                GUIStyle gUIStyle = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = 16,
                };
                gUIStyle.margin = new RectOffset(25, 25, 0, 0);
                gUIStyle.wordWrap = true;
                gUIStyle.normal.textColor = Color.white;
                return gUIStyle;
            }
        }


        static Texture2D ColorToTexture2D(Color32 color)
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture2D.SetPixel(0, 0, color);
            texture2D.Apply();
            return texture2D;
        }
    }
}
