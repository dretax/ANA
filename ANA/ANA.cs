using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Fougerite;
using Fougerite.Events;

namespace ANA
{
    /*
     * 
     * ANA was converted to C# due to IP's failure to sometimes handle NON-ASCII characters.
     * 
     */
    public class ANA : Fougerite.Module
    {
        public IniParser Settings;
        public Random random = new Random();
        public int KickInsteadOfRenaming = 0;
        public int NameLength = 17;
        public int DontRenameMods = 1;
        public int DontRenameAdmins = 1;
        public string NotificationMessage = "Only English Characters in name, see chat!";
        public string NotificationMessage2 = "We only allow these characters: a-z,0-9!+()[]<>/@#. _-";
        public string RegexMatcher = @"[^a-zA-Z0-9_!+?()<>/@#,. \[\]\\-]";

        public List<int> RandNames = new List<int>();
        public List<string> TakenNames = new List<string>();
        public List<string> Restricted = new List<string>();

        public override string Name
        {
            get { return "ANA"; }
        }

        public override string Author
        {
            get { return "DreTaX"; }
        }

        public override string Description
        {
            get { return "ANA"; }
        }

        public override Version Version
        {
            get { return new Version("1.1"); }
        }

        public override void Initialize()
        {
            if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                Settings.AddSetting("Settings", "DontRenameAdmins", "1");
                Settings.AddSetting("Settings", "DontRenameMods", "1");
                Settings.AddSetting("Settings", "NameLength", "17");
                Settings.AddSetting("Settings", "KickInsteadOfRenaming", "0");
                Settings.AddSetting("Settings", "NotificationMessage", "Only English Characters in name, see chat!");
                Settings.AddSetting("Settings", "NotificationMessage2", "We only allow these characters: a-z,0-9!+()[]<>/@#. _-");
                Settings.AddSetting("Settings", "RegexMatcher", @"[^a-zA-Z0-9_!+?()<>/@#,. \[\]\\-]");
                Settings.AddSetting("Restricted", "1", "DerpTeamNoob");
                Settings.AddSetting("Restricted", "2", "Changeme");
                Settings.Save();
            }
            else
            {
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
            }
            ReloadConfig();
            Fougerite.Hooks.OnCommand += OnCommand;
            Fougerite.Hooks.OnPlayerConnected += OnPlayerConnected; // Maybe try playerapproval?
            Fougerite.Hooks.OnPlayerDisconnected += OnPlayerDisconnected;
        }

        public override void DeInitialize()
        {
            Fougerite.Hooks.OnCommand -= OnCommand;
            Fougerite.Hooks.OnPlayerConnected -= OnPlayerConnected;
            Fougerite.Hooks.OnPlayerDisconnected -= OnPlayerDisconnected;
        }

        private bool ReloadConfig()
        {
            if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                Settings.AddSetting("Settings", "DontRenameAdmins", "1");
                Settings.AddSetting("Settings", "DontRenameMods", "1");
                Settings.AddSetting("Settings", "NameLength", "17");
                Settings.AddSetting("Settings", "KickInsteadOfRenaming", "0");
                Settings.AddSetting("Settings", "NotificationMessage", "Only English Characters in name, see chat!");
                Settings.AddSetting("Settings", "NotificationMessage2", "We only allow these characters: a-z,0-9!+()[]<>/@#. _-");
                Settings.AddSetting("Settings", "RegexMatcher", @"[^a-zA-Z0-9_!+?()<>/@#,. \[\]\\-]");
                Settings.AddSetting("Restricted", "1", "DerpTeamNoob");
                Settings.AddSetting("Restricted", "2", "Changeme");
                Settings.Save();
            }
            Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
            try
            {
                DontRenameMods = int.Parse(Settings.GetSetting("Settings", "DontRenameMods"));
                DontRenameAdmins = int.Parse(Settings.GetSetting("Settings", "DontRenameAdmins"));
                NameLength = int.Parse(Settings.GetSetting("Settings", "NameLength"));
                KickInsteadOfRenaming = int.Parse(Settings.GetSetting("Settings", "KickInsteadOfRenaming"));
                NotificationMessage = Settings.GetSetting("Settings", "NotificationMessage");
                NotificationMessage2 = Settings.GetSetting("Settings", "NotificationMessage2");
                RegexMatcher = @Settings.GetSetting("Settings", "RegexMatcher");
                string[] ls = Settings.EnumSection("Restricted");
                foreach (var x in ls)
                {
                    Restricted.Add(Settings.GetSetting("Restricted", x).ToLower());
                }
            }
            catch (Exception ex)
            {
                Fougerite.Logger.LogError("[ANA] Failed to read config, possible wrong value somewhere! Ex: " + ex);
                return false;
            }
            return true;
        }

        public int GetNum()
        {
            for (int i = 0; i <= 1000; i++)
            {
                if (!RandNames.Contains(i))
                {
                    return i;
                }
            }
            return random.Next(1001, 999999999); // Should never happen.
        }

        public void OnPlayerDisconnected(Fougerite.Player player)
        {
            if (player.Name.Contains("Stranger"))
            {
                int justNumbers;
                bool b = int.TryParse(new string(player.Name.Where(char.IsDigit).ToArray()), out justNumbers);
                if (b && RandNames.Contains(justNumbers))
                {
                    RandNames.Remove(justNumbers);
                }
            }
            if (TakenNames.Contains(player.Name))
            {
                TakenNames.Remove(player.Name);
            }
        }

        public void OnPlayerConnected(Fougerite.Player player)
        {
            if (player.Admin && DontRenameAdmins == 1)
            {
                return;
            }
            if (player.Moderator && DontRenameMods == 1)
            {
                return;
            }
            string name = player.Name;
            if (KickInsteadOfRenaming == 1)
            {
                bool xd = Regex.IsMatch(name, RegexMatcher);
                if (xd || string.IsNullOrEmpty(name) || name.Length <= 1)
                {
                    player.Notice("", NotificationMessage, 15f);
                    player.MessageFrom("ANA", NotificationMessage2);
                    player.Disconnect();
                }
                return;
            }
            name = name.Substring(0, Math.Min(name.Length, NameLength));
            name = Regex.Replace(name, RegexMatcher, string.Empty);
            if (string.IsNullOrEmpty(name) || name.Length <= 1 || string.Join(" ", TakenNames.ToArray()).ToLower().Contains(name.ToLower()) || string.Join(" ", Restricted.ToArray()).Contains(name.ToLower()))
            {
                name = "Stranger";
                int randnumber = GetNum();
                name = name + randnumber;
                RandNames.Add(randnumber);
            }
            TakenNames.Add(name);
            player.Name = name;
        }

        public void OnCommand(Fougerite.Player player, string cmd, string[] args)
        {
            if (cmd == "anareload")
            {
                if (player.Admin)
                {
                    bool b = ReloadConfig();
                    player.MessageFrom("ANA", b ? "Config Reloaded!" : "Failed to reload config!");
                }
            }
        }
    }
}
