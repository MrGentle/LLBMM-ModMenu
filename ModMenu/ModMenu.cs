using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using LLScreen;
using LLGUI;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration;

namespace ModMenu
{
    [BepInPlugin(PluginInfos.PLUGIN_ID, PluginInfos.PLUGIN_NAME, PluginInfos.PLUGIN_VERSION)]
    [BepInProcess("LLBlaze.exe")]
    public class ModMenu : BaseUnityPlugin
    {
        public static ModMenu Instance { get; private set; } = null;
        public static Dictionary<BepInEx.PluginInfo, List<string>> registeredMods = new Dictionary<BepInEx.PluginInfo, List<string>>();
        public void Awake()
        {
            Instance = this;
            ModMenuStyle.InitStyle();
        }

        private readonly Array keyCodes = System.Enum.GetValues(typeof(KeyCode));

        private ScreenMenu mainmenu = null;
        private ScreenBase submenu = null;
        private AccessTools.FieldRef<object, ScreenBase[]> _currentScreens = AccessTools.FieldRefAccess<ScreenBase[]>(typeof(LLScreen.UIScreen), "currentScreens");
        private LLButton modSettingsButton = null;
        public List<LLButton> modButtons = new List<LLButton>();

        public Vector2 keybindScrollpos = new Vector2(0, 0);
        public Vector2 optionsScrollpos = new Vector2(0, 0);
        public Vector2 optionsTextpos = new Vector2(0, 0);

        private BepInEx.PluginInfo currentOpenMod;
        private BepInEx.PluginInfo previousOpenMod;

        private bool sliderChange = false;
        private ConfigDefinition definitionToRebind = null;
        private bool rebindingKey = false;
        private int switchInputModeTimer = 0;
        private bool inModOptions = false;
        public static bool InModOptions => Instance.inModOptions;
        private bool inModSubOptions = false;


        public static void RegisterMod(BepInEx.PluginInfo pluginInfo, List<string> modmenu_textinfo = null)
        {
            registeredMods.Add(pluginInfo, modmenu_textinfo);
        }

        private void Update()
        {
            if (this.sliderChange && Input.GetKeyUp(KeyCode.Mouse0))
            {
                this.currentOpenMod.Instance.Config.Save();
                this.currentOpenMod.Instance.Config.SaveOnConfigSet = true;
                this.sliderChange = false;
            }

            if (this.rebindingKey && Input.anyKeyDown)
            {
                foreach (KeyCode keyCode in keyCodes)
                {
                    if (Input.GetKey(keyCode))
                    {
                        ConfigFile modConfig = this.currentOpenMod.Instance.Config;
                        modConfig[this.definitionToRebind].BoxedValue = keyCode;
                        modConfig.Save();
                        this.rebindingKey = false;
                    }
                }
            }
            if (mainmenu == null)
                mainmenu = FindObjectOfType<ScreenMenu>();

            if (submenu == null)
            {
                submenu = _currentScreens.Invoke()[1];
            }
            else
            {
                if (submenu.screenType == ScreenType.MENU_OPTIONS && modSettingsButton == null)
                {
                    modSettingsButton = this.InitModSettingsButton();
                }
            }


            if (switchInputModeTimer != 30) switchInputModeTimer++;

            if (inModSubOptions && this.currentOpenMod != this.previousOpenMod) //If we are within the options of a spiesific mod.
            {
                this.previousOpenMod = this.currentOpenMod;
            }

            if (inModOptions)
            {
                if (LLHandlers.Controller.mouseKeyboard.GetButton(LLHandlers.InputAction.ESC))
                {
                    this.previousOpenMod = null;
                    UIScreen.Open(ScreenType.MENU_OPTIONS, 1, ScreenTransition.MOVE_RIGHT);
                    inModOptions = false;
                    inModSubOptions = false;
                }
            }
        }

        #region Handles
        public void HandleModSettingsClick(int playerNr)
        {
            ScreenBase screen = UIScreen.Open(ScreenType.OPTIONS, 1);
            inModOptions = true;
            GameObject.Find("btQuit").GetComponent<LLButton>().onClick = new LLClickable.ControlDelegate(this.HandleQuitClick);
            mainmenu.lbTitle.text = "MOD SETTINGS";

            var vcount = 0;
            var hcount = 0;
            foreach (BepInEx.PluginInfo plugin in registeredMods.Keys)
            {
                var obj = Instantiate(modSettingsButton, screen.transform);
                obj.SetText(plugin.Metadata.Name, 50);
                obj.transform.localScale = new Vector3(0.4f, 0.3f);
                obj.transform.position = new Vector3(-1.5f + (0.75f * hcount), 0.80f - (0.125f * vcount));
                obj.onClick = delegate (int pNr) { this.HandleModSubSettingsClick(plugin); };
                modButtons.Add(obj);
                if (vcount < 12) { vcount++; } else { hcount++; vcount = 0; }
            }
        }

        private void HandleModSubSettingsClick(BepInEx.PluginInfo plugin)
        {
            inModSubOptions = true;
            currentOpenMod = plugin;
            ScreenBase screen = UIScreen.Open(ScreenType.OPTIONS, 1);
            mainmenu.lbTitle.text = plugin.Metadata.Name.ToUpper() + " SETTINGS";
        }

        private void HandleQuitClick(int playerNr)
        {
            this.previousOpenMod = null;
            if (inModOptions == true)
            {
                UIScreen.Open(ScreenType.MENU_OPTIONS, 1);
            }
            else
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
            if (_currentScreens.Invoke()[1].screenType == ScreenType.MENU_OPTIONS)
            {
                mainmenu.lbTitle.text = "OPTIONS";
                inModOptions = false;
                inModSubOptions = false;
                LLHandlers.AudioHandler.PlayMenuSfx(LLHandlers.Sfx.MENU_BACK);
                LLHandlers.AudioHandler.PlayMenuSfx(LLHandlers.Sfx.MENU_CONFIRM);
            }
        }
        #endregion

        public LLButton InitModSettingsButton()
        {
            LLButton _modSettingsButton = null;
            List<LLButton> buttons = new List<LLButton>();
            ScreenMenuOptions SMO = FindObjectOfType<ScreenMenuOptions>();
            buttons.Add(SMO.btGame);
            buttons.Add(SMO.btInput);
            buttons.Add(SMO.btAudio);
            buttons.Add(SMO.btVideo);
            buttons.Add(SMO.btCredits);

            _modSettingsButton = Instantiate(buttons[4], buttons[4].transform, true);
            _modSettingsButton.name = "btMods";
            _modSettingsButton.SetText("mod settings");
            _modSettingsButton.onClick = new LLClickable.ControlDelegate(this.HandleModSettingsClick);

            buttons.Add(_modSettingsButton);

            for (var i = 0; i < buttons.Count(); i++)
            {
                if (buttons[i] == null) continue;
                Vector3 scale = buttons[i].transform.localScale;

                int modx = 2560;
                int mody = 1440;

                if (buttons[i] == _modSettingsButton)
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
                    buttons[i].transform.localPosition = new Vector3(-((modx / 100) * 8.375f), ((mody / 100) * 12.625f));
                    buttons[i].transform.localScale = new Vector3(scale.x * 0.85f, scale.y * 0.85f, scale.z);
                }
            }

            return _modSettingsButton;
        }

        #region GUIStuff

        private GUI.WindowFunction keybindWindowFunction = null;
        private GUI.WindowFunction optionsWindowFunction = null;
        private GUI.WindowFunction textWindowFunction = null;
        private void OnGUI()
        {
            if (keybindWindowFunction == null)
                keybindWindowFunction = new GUI.WindowFunction(OpenKeybindsWindow);
            if (optionsWindowFunction == null)
                optionsWindowFunction = new GUI.WindowFunction(OpenOptionsWindow);
            if (textWindowFunction == null)
                textWindowFunction = new GUI.WindowFunction(OpenTextWindow);

            var x1 = Screen.width / 6;
            var y1 = Screen.height / 10;
            var x2 = Screen.width - (Screen.width / 6) * 2;
            var y2 = Screen.height - (Screen.height / 6);

            if (inModSubOptions)
            {
                GUIContent guic = new GUIContent("   ModMenu " + this.Info.Metadata.Version + "   ");
                Vector2 calc = GUI.skin.box.CalcSize(guic);
                GUI.Box(new Rect(10, 10, calc.x + 20, calc.y), "ModMenu " + this.Info.Metadata.Version, ModMenuStyle.versionBox);

                GUI.Window(0, new Rect(x1, y1, x2, y2 / 3), keybindWindowFunction, "Keybindings", ModMenuStyle.windStyle);
                if (registeredMods[currentOpenMod] != null)
                {
                    GUI.Window(1, new Rect(x1, y1 + y2 / 3 + 10, x2, y2 / 3), optionsWindowFunction, "Options", ModMenuStyle.windStyle);
                    GUI.Window(2, new Rect(x1, y1 + ((y2 / 3 + 10) * 2), x2, y2 / 5), textWindowFunction, "Mod Information", ModMenuStyle.windStyle);
                }
                else
                {
                    GUI.Window(1, new Rect(x1, y1 + y2 / 3 + 10, x2, 2 * y2 / 3 ), optionsWindowFunction, "Options", ModMenuStyle.windStyle);
                }

                GUI.skin.window = null;
            }
            GUI.skin.label.fontSize = 15;
        }
        private void OpenKeybindsWindow(int wId)
        {
            GUILayout.Space(30);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            keybindScrollpos = GUILayout.BeginScrollView(keybindScrollpos, false, true);

            ConfigFile modConfig = currentOpenMod.Instance.Config;

            foreach (ConfigDefinition setting in modConfig.Keys.Where((ConfigDefinition setting) => modConfig[setting].SettingType == typeof(KeyCode)))
            {
                string formatted = UppercaseFirst(Regex.Replace(setting.Key, "([a-z])([A-Z])", "$1 $2"));
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(formatted + ":", ModMenuStyle.labStyle);
                GUILayout.Space(10);

                string displayText;

                if (this.rebindingKey && setting == this.definitionToRebind)
                    displayText = "WAITING FOR KEY";
                else
                    displayText = currentOpenMod.Instance.Config[setting].BoxedValue.ToString();

                if (GUILayout.Button("[" + displayText + "]", ModMenuStyle.button, GUILayout.MinWidth(100)))
                {
                    this.rebindingKey = true;
                    this.definitionToRebind = setting;
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
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
            ConfigFile modConfig = currentOpenMod.Instance.Config;

            foreach (ConfigDefinition setting in modConfig.Keys)
            {
                Type settingType = modConfig[setting].SettingType;
                if (settingType == typeof(bool))
                {
                    MakeBoolSettingGUI(modConfig, setting);
                }
                else if (settingType == typeof(int))
                {
                    if (modConfig[setting].Description?.AcceptableValues is AcceptableValueRange<int>
                    // NYI || modConfig[setting].Description.AcceptableValues is AcceptableValueRange<float>
                    )
                    {
                        MakeSliderSettingGUI(modConfig, setting);
                    }
                    else
                    {
                        MakeNumericSettingGUI(modConfig, setting);
                    }
                }
                else if (settingType == typeof(string))
                {
                    string settingValue = (string)modConfig[setting].BoxedValue;
                    string settingDesc = modConfig[setting].Description.Description;
                    object[] settingTags = modConfig[setting].Description.Tags;

                    if (settingTags.Contains("modmenu_filepicker") || settingDesc.ToLower() == "modmenu_filepicker")
                    {
                        // TODO NYI MakeFilePickerSettingGUI(modConfig, setting);
                        MakeStringSettingGUI(modConfig, setting);
                    }
                    else if (settingTags.Contains("modmenu_header") || settingDesc.ToLower() == "modmenu_header")
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.Box(settingValue, ModMenuStyle.headerBox);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                    else if (settingTags.Contains("modmenu_gap") || settingDesc.ToLower() == "modmenu_gap")
                    {
                        if (Int32.TryParse(settingValue, out int n))
                        {
                            GUILayout.Space(n);
                        }
                        else
                        {
                            GUILayout.Space(20);
                        }
                    }
                    else
                    {
                        MakeStringSettingGUI(modConfig, setting);
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void MakeBoolSettingGUI(ConfigFile modConfig, ConfigDefinition setting)
        {

            bool val = (bool)modConfig[setting].BoxedValue;
            string formatted = UppercaseFirst(Regex.Replace(setting.Key, "([a-z])([A-Z])", "$1 $2"));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(formatted + ":", ModMenuStyle.labStyle);
            GUILayout.Space(10);

            var str = "";
            if (val) str = "Enabled";
            else str = "Disabled";

            bool isPressed = GUILayout.Button(str, ModMenuStyle.button, GUILayout.MinWidth(100));
            if (isPressed)
            {
                modConfig[setting].BoxedValue = !val;
                modConfig.Save();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void MakeNumericSettingGUI(ConfigFile modConfig, ConfigDefinition setting)
        {
            int value = (int)modConfig[setting].BoxedValue;

            string formatted = UppercaseFirst(Regex.Replace(setting.Key, "([a-z])([A-Z])", "$1 $2"));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(formatted + ": ", ModMenuStyle.labStyle);
            GUILayout.Box(value.ToString(), ModMenuStyle.box);
            GUILayout.Space(10);

            bool isMinusPressed = GUILayout.Button("  -  ", ModMenuStyle.button);
            bool isPlusPressed = GUILayout.Button("  +  ", ModMenuStyle.button);


            GUILayout.Space(30);

            string intValue = GUILayout.TextField(value.ToString(), 10, ModMenuStyle._textFieldStyle, GUILayout.MinWidth(32));

            bool isFromTextPressed = GUILayout.Button("Set Value From Textbox", ModMenuStyle.button);

            if (isPlusPressed)
            {
                modConfig[setting].BoxedValue = value + 1;
                modConfig.Save();
            }
            else if (isMinusPressed)
            {
                modConfig[setting].BoxedValue = value - 1;
                modConfig.Save();
            }
            else if (Int32.TryParse(intValue, out int n))
            {
                if ((int)modConfig[setting].BoxedValue != n)
                {
                    modConfig[setting].BoxedValue = n;
                    modConfig.Save();
                }
                if (isFromTextPressed)
                {
                    modConfig[setting].BoxedValue = n;
                    modConfig.Save();
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void MakeSliderSettingGUI(ConfigFile modConfig, ConfigDefinition setting)
        {
            int storedSliderValue = (int)modConfig[setting].BoxedValue;
            AcceptableValueRange<int> range = modConfig[setting].Description.AcceptableValues as AcceptableValueRange<int>;
            string formatted = UppercaseFirst(Regex.Replace(setting.Key, "([a-z])([A-Z])", "$1 $2"));

            float sliderValue = storedSliderValue;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(formatted + ": ", ModMenuStyle.labStyle);
            sliderValue = GUILayout.HorizontalSlider(sliderValue, (float)range.MinValue, (float)range.MaxValue, ModMenuStyle._sliderBackgroundStyle, ModMenuStyle._sliderThumbStyle, GUILayout.Width(300));
            GUILayout.Box(System.Math.Round(Double.Parse(sliderValue.ToString())).ToString(), ModMenuStyle.box);

            int newSliderValue = (int)System.Math.Round(sliderValue);
            if (newSliderValue != storedSliderValue)
            {
                modConfig.SaveOnConfigSet = false;
                this.sliderChange = true;
                modConfig[setting].BoxedValue = newSliderValue;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void MakeStringSettingGUI(ConfigFile modConfig, ConfigDefinition setting)
        {
            string value = (string)modConfig[setting].BoxedValue;

            string formatted = UppercaseFirst(Regex.Replace(setting.Key, "([a-z])([A-Z])", "$1 $2"));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(formatted + ": ", ModMenuStyle.labStyle);
            GUILayout.Space(30);

            string textFieldString = GUILayout.TextField(value, 256, ModMenuStyle._textFieldStyle, GUILayout.MinWidth(120));


            GUIStyle test = ModMenuStyle.button;
            if ((string)modConfig[setting].BoxedValue != textFieldString)
            {
                test.fontStyle = FontStyle.Bold;
                modConfig.SaveOnConfigSet = false;
                modConfig[setting].BoxedValue = textFieldString;
            }
            bool isFromTextPressed = GUILayout.Button("Set Value", test);

            if (isFromTextPressed)
            {
                modConfig.Save();
                modConfig.SaveOnConfigSet = true;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void MakeFilePickerSettingGUI(ConfigFile modConfig, ConfigDefinition setting)
        {
            // TODO NYI
        }

        private void OpenTextWindow(int wId)
        {
            GUILayout.Space(30);
            GUILayout.BeginVertical();
            optionsTextpos = GUILayout.BeginScrollView(optionsTextpos, false, true);

            ConfigFile modConfig = currentOpenMod.Instance.Config;
            if (registeredMods.ContainsKey(currentOpenMod.Instance.Info) && registeredMods[currentOpenMod.Instance.Info] != null)
            {
                foreach (string textLine in registeredMods[currentOpenMod.Instance.Info])
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(textLine, ModMenuStyle.readStyle);
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        #endregion

        static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
