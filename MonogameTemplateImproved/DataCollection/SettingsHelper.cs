using System;
using System.IO;
using System.Web.Script.Serialization;

public class SettingsHelper
{
    public SettingsHelper()
    {
    }

    public static GameSettings ReadSettings(string path)
    {
        GameSettings settings;
        string json = String.Empty;

        if (!File.Exists(path))
        {
            GameSettings initialSetting = new GameSettings();

            initialSetting.DatabaseName = "Test";
            initialSetting.ServerName = "YourServerAndInstanceNameHere";
            initialSetting.UserName = "UsrName_Here_Must_Be_SQL_Authentication";
            initialSetting.Password = "YourSecurePasswordHere";

            SetDefaultWorld(ref initialSetting);

            WriteSettings(path, initialSetting);
        }

        json = File.ReadAllText(path);
        settings = new JavaScriptSerializer().Deserialize<GameSettings>(json);

        return settings;
    }
    public static void WriteSettings(string path, GameSettings settings)
    {
        string json = new JavaScriptSerializer().Serialize(settings);
        File.WriteAllText(path, JsonHelper.FormatJson(json));
    }
    public static void SetDefaultWorld(ref GameSettings settingsIn)
    {
        settingsIn.WorldSize = 10000;
    }
}