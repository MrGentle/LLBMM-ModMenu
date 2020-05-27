using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LLScreen;
using LLGUI;
using LLHandlers;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace LLModMenu
{
    public class ModMenu : MonoBehaviour
    {
        private static ModMenu instance = null;
        public static ModMenu Instance { get { return instance; } }
        public static void Initialize() { GameObject gameObject = new GameObject("ModMenu"); ModMenu modLoader = gameObject.AddComponent<ModMenu>(); DontDestroyOnLoad(gameObject); instance = modLoader; ModMenuStyle.InitStyle();  }

        public LLButton button = null;
        public List<string> mods = new List<string>();
        public List<LLButton> modButtons = new List<LLButton>();
        public bool inModOptions = false;
        public bool inModSubOptions = false;
        public ScreenMenu mainmenu = null;
        public ScreenBase submenu = null;
        public Dictionary<string, string> configKeys = new Dictionary<string, string>();
        public Dictionary<string, string> configBools = new Dictionary<string, string>();
        public Dictionary<string, string> configInts = new Dictionary<string, string>();
        public List<string> intList = new List<string>();
        public Dictionary<string, string> configHeaders = new Dictionary<string, string>();
        public Dictionary<string, string> configSliders = new Dictionary<string, string>();
        public List<float> sliderList = new List<float>();
        public Dictionary<string, string> configText = new Dictionary<string, string>();
        public List<string> optionsQueue = new List<string>();

        public string currentOpenMod = "";
        public string newKey = "";
        public Vector2 keybindScrollpos = new Vector2(0,0);
        public Vector2 optionsScrollpos = new Vector2(0, 0);
        public Vector2 optionsTextpos = new Vector2(0, 0);

        public ModMenuIntegration MMI = null;

        public int switchInputModeTimer = 0;

        private string modVersion = "v1.1.0";
        private string iniLocation = Path.GetDirectoryName(Application.dataPath) + "\\ModSettings";



        private void Update()
        {
            if (!Directory.Exists(iniLocation)) Directory.CreateDirectory(iniLocation);
            if (mainmenu == null) { mainmenu = FindObjectOfType<ScreenMenu>(); }
            if (submenu == null) { submenu = UIScreen.currentScreens[1]; }
            else
            {

                List<LLButton> buttons = new List<LLButton>();
                if (submenu.screenType == ScreenType.MENU_OPTIONS && button == null)
                {
                    ScreenMenuOptions SMO = FindObjectOfType<ScreenMenuOptions>();
                    buttons.Add(SMO.btGame);
                    buttons.Add(SMO.btInput);
                    buttons.Add(SMO.btAudio);
                    buttons.Add(SMO.btVideo);
                    buttons.Add(SMO.btCredits);

                    button = Instantiate(buttons[4], buttons[4].transform, true);
                    button.name = "btMods";
                    button.SetText("mod settings");
                    button.onClick = new LLClickable.ControlDelegate(this.ModSettingsClick);

                    buttons.Add(button);

                    for(var i = 0; i < buttons.Count(); i++)
                    {
                        Vector3 scale = buttons[i].transform.localScale;

                        int modx = 2560;
                        int mody = 1440;

                        if (buttons[i] == button)
                        {
                            buttons[i].transform.localPosition = new Vector3(buttons[i].transform.localPosition.x - ((modx / 100) * 1), buttons[i].transform.localPosition.y - ((mody / 100) * 9.5f));
                        }
                        else if (buttons[i] != SMO.btGame)
                        {
                            buttons[i].transform.localPosition = new Vector3(SMO.btGame.transform.localPosition.x - (((modx / 100) * 0.55f) * i), SMO.btGame.transform.localPosition.y - (((mody / 100) * 5.3f) * i));
                            buttons[i].transform.localScale = new Vector3(scale.x * 0.85f, scale.y * 0.85f, scale.z);
                        }
                        else
                        {
                            buttons[i].transform.localPosition = new Vector3(-((modx / 100)*8.375f), ((mody / 100) * 12.625f));
                            buttons[i].transform.localScale = new Vector3(scale.x * 0.85f, scale.y * 0.85f, scale.z);
                        }
                    }
                }
            }


            if (switchInputModeTimer != 30) switchInputModeTimer++;

            if (inModSubOptions) //If we are within the options of a spiesific mod.
            {
                Component comp = GameObject.Find(currentOpenMod).GetComponent(currentOpenMod); //Get the script for that mod.
                comp.SendMessage("ReadIni"); //Tell it to run the "ReadIni" method.
            }

            if (Controller.mouseKeyboard.GetButton(InputAction.ESC))
            {
                if (inModOptions)
                {
                    UIScreen.Open(ScreenType.MENU_OPTIONS, 1, ScreenTransition.MOVE_RIGHT);
                    inModOptions = false;
                    inModSubOptions = false;
                }
            }
        }

        private void ModSettingsClick(int playerNr)
        {
            ScreenBase screen = UIScreen.Open(ScreenType.OPTIONS, 1);
            inModOptions = true;
            GameObject.Find("btQuit").GetComponent<LLButton>().onClick = new LLClickable.ControlDelegate(QuitClick);
            mainmenu.lbTitle.text = "MOD SETTINGS";

            var vcount = 0;
            var hcount = 0;

            foreach (string mod in mods)
            {
                var obj = Instantiate(button, screen.transform);
                obj.SetText(mod, 50);
                obj.transform.localScale = new Vector3(0.4f, 0.3f);
                obj.transform.position = new Vector3(-1.5f + (0.75f * hcount), 0.80f - (0.125f * vcount));
                obj.onClick = delegate (int pNr) { ModSubSettingsClick(mod); };
                modButtons.Add(obj);
                if (vcount < 12) { vcount++; } else { hcount++; vcount = 0; }
            }
        }

        private void QuitClick(int playerNr)
        {
            if (inModOptions == true)
            {
                UIScreen.Open(ScreenType.MENU_OPTIONS, 1);
            } else
            {
                if (submenu != null)
                {
                    if (submenu.screenType == ScreenType.MENU_MAIN)
                    {
                        DNPFJHMAIBP.GKBNNFEAJGO(Msg.QUIT, playerNr, -1);
                    }
                    else
                    {
                        DNPFJHMAIBP.GKBNNFEAJGO(Msg.BACK, playerNr, -1);
                    }
                }
            }
            if (UIScreen.currentScreens[1].screenType == ScreenType.MENU_OPTIONS)
            {
                mainmenu.lbTitle.text = "OPTIONS";
                inModOptions = false;
                inModSubOptions = false;
                AudioHandler.PlayMenuSfx(Sfx.MENU_BACK);
                AudioHandler.PlayMenuSfx(Sfx.MENU_CONFIRM);
            }
        }

        #region GUIStuff
        private void OnGUI()
        {
            var x1 = Screen.width / 6;
            var y1 = Screen.height / 10;
            var x2 = Screen.width - (Screen.width/6)*2;
            var y2 = Screen.height - (Screen.height / 6);


            if (inModSubOptions)
            {
                GUIContent guic = new GUIContent("   ModMenu " + modVersion + "   ");
                Vector2 calc = GUI.skin.box.CalcSize(guic);
                GUI.Box(new Rect(10, 10, calc.x, calc.y), "ModMenu " + modVersion, ModMenuStyle.versionBox);
                GUI.Window(0, new Rect(x1, y1, x2, y2 / 3), new GUI.WindowFunction(OpenKeybindsWindow), "Keybindings", ModMenuStyle.windStyle);
                GUI.Window(1, new Rect(x1, y1 + y2 / 3 + 10, x2, y2 / 3), new GUI.WindowFunction(OpenOptionsWindow), "Options", ModMenuStyle.windStyle);
                GUI.Window(2, new Rect(x1, y1 + ((y2 / 3 + 10)*2), x2, y2 / 5), new GUI.WindowFunction(OpenTextWindow), "Mod Information", ModMenuStyle.windStyle);
                GUI.skin.window = null;
            }
            GUI.skin.label.fontSize = 15;
        }

        private void ModSubSettingsClick(string modName)
        {
            inModSubOptions = true;
            currentOpenMod = modName;
            ScreenBase screen = UIScreen.Open(ScreenType.OPTIONS, 1);
            mainmenu.lbTitle.text = modName.ToUpper() + " SETTINGS";
            configKeys.Clear();
            configBools.Clear();
            configInts.Clear();
            intList.Clear();
            configSliders.Clear();
            sliderList.Clear();
            configHeaders.Clear();
            configText.Clear();

            string[] lines = File.ReadAllLines(iniLocation + @"\" + modName + ".ini");
            foreach (string line in lines)
            {
                if (line.StartsWith("(key)"))
                {
                    string[] split = line.Split('=');
                    configKeys.Add(split[0], split[1]);
                }
                else if (line.StartsWith("(bool)"))
                {
                    string[] split = line.Split('=');
                    configBools.Add(split[0], split[1]);
                }
                else if (line.StartsWith("(int)"))
                {
                    string[] split = line.Split('=');
                    configInts.Add(split[0], split[1]);
                    intList.Add(split[1]);
                }
                else if (line.StartsWith("(slider)"))
                {
                    string[] split = line.Split('=');
                    configSliders.Add(split[0], split[1]);

                    string[] valMinMax = split[1].Split('|');
                    sliderList.Add(float.Parse(valMinMax[0]));
                }
                else if (line.StartsWith("(header)"))
                {
                    string[] split = line.Split('=');
                    configHeaders.Add(split[0], split[1]);
                }
                else if (line.StartsWith("(text)"))
                {
                    string[] split = line.Split('=');
                    configText.Add(split[0], split[1]);
                }
            }
        }


        private void OpenKeybindsWindow(int wId)
        {
            GUILayout.Space(30);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            keybindScrollpos = GUILayout.BeginScrollView(keybindScrollpos, false, true);
            var keyList = new List<string>();
            if (configKeys.Count > 0)
            {
                foreach (KeyValuePair<string, string> keyval in configKeys) keyList.Add(keyval.Key);

                foreach (string key in keyList)
                {
                    string format = key.Remove(0, 5);
                    string formatted = UppercaseFirst(Regex.Replace(format, "([a-z])([A-Z])", "$1 $2"));
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(formatted + ":", ModMenuStyle.labStyle);
                    GUILayout.Space(10);
                    if (GUILayout.Button("[" + configKeys[key] + "]", ModMenuStyle.button, GUILayout.MinWidth(100)))
                    {
                        StartCoroutine(BindKey(key));
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void OpenOptionsWindow(int wId)
        {
            GUILayout.Space(30);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            optionsScrollpos = GUILayout.BeginScrollView(optionsScrollpos, false, true);

            optionsQueue = GetOptionsQueue(currentOpenMod);
            int bools = 0;
            int ints = 0;
            int sliders = 0;
            int headers = 0;

            if (optionsQueue != null)
            {
                foreach (string option in optionsQueue)
                {
                    if (option == "(bool)")
                    {
                        var key = configBools.Keys.ElementAt(bools);
                        var val = configBools.Values.ElementAt(bools);
                        string format = key.Remove(0, 6);
                        string formatted = UppercaseFirst(Regex.Replace(format, "([a-z])([A-Z])", "$1 $2"));
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(formatted + ":", ModMenuStyle.labStyle);
                        GUILayout.Space(10);
                        var str = "";
                        if (val == "true") str = "Enabled";
                        else str = "Disabled";

                        if (GUILayout.Button(str, ModMenuStyle.button, GUILayout.MinWidth(100)))
                        {
                            IniFile modIni = new IniFile(iniLocation + @"\" + currentOpenMod + ".ini");
                            if (val == "true")
                            {
                                modIni.Write(key, "false");
                                configBools[key] = "false";
                            }
                            else
                            {
                                modIni.Write(key, "true");
                                configBools[key] = "true";
                            }
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        bools++;
                    }
                    else if (option == "(int)")
                    {
                        var key = configInts.Keys.ElementAt(ints);
                        string format = key.Remove(0, 5);
                        string formatted = UppercaseFirst(Regex.Replace(format, "([a-z])([A-Z])", "$1 $2"));
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        GUILayout.Label(formatted + ": ", ModMenuStyle.labStyle);
                        GUILayout.Box(configInts[key], ModMenuStyle.box);
                        GUILayout.Space(10);
                        if (GUILayout.Button("  -  ", ModMenuStyle.button))
                        {
                            IniFile modIni = new IniFile(iniLocation + @"\" + currentOpenMod + ".ini");
                            var j = Convert.ToInt32(configInts[key]);
                            j--;
                            modIni.Write(key, j.ToString());
                            configInts[key] = j.ToString();
                        }

                        if (GUILayout.Button("  +  ", ModMenuStyle.button))
                        {
                            IniFile modIni = new IniFile(iniLocation + @"\" + currentOpenMod + ".ini");
                            var j = Convert.ToInt32(configInts[key]);
                            j++;
                            modIni.Write(key, j.ToString());
                            configInts[key] = j.ToString();
                        }

                        GUILayout.Space(30);

                        intList[ints] = GUILayout.TextField(intList[ints].ToString(), 10, ModMenuStyle._textFieldStyle, GUILayout.MinWidth(32));

                        if (GUILayout.Button("Set Value From Textbox", ModMenuStyle.button))
                        {
                            IniFile modIni = new IniFile(iniLocation + @"\" + currentOpenMod + ".ini");
                            if (Int32.TryParse(intList[ints], out int n))
                            {
                                modIni.Write(key, intList[ints]);
                                configInts[key] = intList[ints];
                            }
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        ints++;
                    }
                    else if (option == "(slider)")
                    {
                        IniFile modIni = new IniFile(iniLocation + @"\" + currentOpenMod + ".ini");

                        var key = configSliders.Keys.ElementAt(sliders);
                        string format = key.Remove(0, 8);
                        string formatted = UppercaseFirst(Regex.Replace(format, "([a-z])([A-Z])", "$1 $2"));

                        string[] valMinMax = configSliders[key].Split('|');

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        GUILayout.Label(formatted + ": ", ModMenuStyle.labStyle);
                        sliderList[sliders] = GUILayout.HorizontalSlider(sliderList[sliders], float.Parse(valMinMax[1]), float.Parse(valMinMax[2]), ModMenuStyle._sliderBackgroundStyle, ModMenuStyle._sliderThumbStyle, GUILayout.Width(300));
                        GUILayout.Box(System.Math.Round(Double.Parse(sliderList[sliders].ToString())).ToString(), ModMenuStyle.box);

                        modIni.Write(key, System.Math.Round(Double.Parse(sliderList[sliders].ToString())).ToString() + "|" + valMinMax[1] + "|" + valMinMax[2]);

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        sliders++;
                    }
                    else if (option == "(header)")
                    {
                        var key = configHeaders.Keys.ElementAt(headers);
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.Box(configHeaders[key], ModMenuStyle.headerBox);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        headers++;
                    }
                    else if (option == "(gap)")
                    {
                        GUILayout.Space(20);
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void OpenTextWindow(int wId)
        {
            GUILayout.Space(30);
            GUILayout.BeginVertical();
            optionsTextpos = GUILayout.BeginScrollView(optionsTextpos, false, true);

            if (configText.Count > 0)
            {
                foreach (KeyValuePair<string, string> keyval in configText)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(keyval.Value, ModMenuStyle.readStyle);
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        public void WriteIni(string modName, List<string> writeQueue, Dictionary<string, string> keyBinds, Dictionary<string, string> bools, Dictionary<string, string> ints, Dictionary<string, string> sliders, Dictionary<string, string> headers, Dictionary<string, string> gaps, Dictionary<string, string> text)
        {

            if (File.Exists(iniLocation + @"\" + modName + ".ini"))
            {
                string[] lines = File.ReadAllLines(iniLocation + @"\" + modName + ".ini");
                foreach (string key in writeQueue)
                {
                    try
                    {
                        if (!lines[writeQueue.IndexOf(key) + 1].Contains(key))
                        {
                            Debug.Log("ModMenu: " + iniLocation + @"\" + modName + ".ini has been remade because it did not match what was expected");
                            File.Delete(iniLocation + @"\" + modName + ".ini");
                            break;
                        };
                    } catch
                    {
                        Debug.Log("ModMenu: " + iniLocation + @"\" + modName + ".ini has been remade because it did not match what was expected");
                        File.Delete(iniLocation + @"\" + modName + ".ini");
                        break;
                    }
                }
            }
            IniFile modIni = new IniFile(iniLocation + @"\" + modName + ".ini");


            if (writeQueue.Count() > 0)
            {
                foreach (string key in writeQueue)
                {
                    if (key.StartsWith("(bool)"))
                    {
                        if (!modIni.KeyExists(key)) modIni.Write(key, bools[key]);
                    }
                    else if (key.StartsWith("(int)"))
                    {
                        if (!modIni.KeyExists(key)) modIni.Write(key, ints[key]);
                    }
                    else if (key.StartsWith("(slider)"))
                    {
                        if (!modIni.KeyExists(key)) modIni.Write(key, sliders[key]);
                    }
                    else if (key.StartsWith("(header)"))
                    {
                        if (!modIni.KeyExists(key)) modIni.Write(key, headers[key]);
                    }
                    else if (key.StartsWith("(gap)"))
                    {
                        if (!modIni.KeyExists(key)) modIni.Write(key, gaps[key]);
                    }
                    else if (key.StartsWith("(key)"))
                    {
                        if (!modIni.KeyExists(key)) modIni.Write(key, keyBinds[key]);
                    }
                    else if (key.StartsWith("(text)"))
                    {
                        if (!modIni.KeyExists(key)) modIni.Write(key, text[key]);
                    }
                }
            }
        }

        public List<string> GetOptionsQueue(string modName)
        {
            List<string> ret = new List<string>();
            string[] lines = File.ReadAllLines(iniLocation + @"\" + modName + ".ini");
            if (lines.Length > 0)
            {
                foreach (string line in lines)
                {
                    if (line.StartsWith("(bool)"))
                    {
                        ret.Add("(bool)");
                    }
                    else if (line.StartsWith("(int)"))
                    {
                        ret.Add("(int)");
                    }
                    else if (line.StartsWith("(slider)"))
                    {
                        ret.Add("(slider)");
                    }
                    else if (line.StartsWith("(header)"))
                    {
                        ret.Add("(header)");
                    }
                    else if (line.StartsWith("(gap)"))
                    {
                        ret.Add("(gap)");
                    }
                }
            }
            return ret;
        }

        IEnumerator BindKey(string key)
        {
            bool pressed = false;
            configKeys[key] = "WAITING FOR KEY";
            IniFile modIni = new IniFile(iniLocation + @"\" + currentOpenMod + ".ini");
            while (!pressed)
            {
                foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKey(vKey))
                    {
                        newKey = vKey.ToString();
                        configKeys[key] = newKey;
                        pressed = true;
                        modIni.Write(key, newKey);
                        break;
                    }
                }
                yield return null;
            }
        }

        static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }
        #endregion
    }
}
