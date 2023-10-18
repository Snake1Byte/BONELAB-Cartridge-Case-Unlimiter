using SLZ.Props.Weapons;
using System.Collections.Generic;
using UnityEngine;

namespace CartridgeCaseDespawnUnlimiter
{
    internal class SharedValues
    {
        // Build info
        public const string ModName = "Cartridge Unlimiter";
        public const string Version = "1.0.0";
        public const string Author = "Snake1Byte";

        // Game variables
        public static int MaxCartridges = 1000;
        public static float DespawnTimerInSeconds = 300;
        public static float DefaultDespawnTimer = 30;

        // BoneMenu
        public static string MainMenuColor = "#ffffb0";
        public static string MenuWarningColor = "#ff6e6e";
        public static string MenuEnableColor = "#642b2b";

        // Logic
        public static Dictionary<Gun, int> GrabbedGuns = new Dictionary<Gun, int>();
        public static readonly object SyncLock = new object();
        public static bool ModEnabled = true;
        public static bool Debug = false;
    }
}
