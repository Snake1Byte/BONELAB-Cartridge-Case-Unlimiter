using UnityEngine;
using BoneLib.BoneMenu.Elements;
using BoneLib.BoneMenu;
using System;
using MelonLoader;
using System.Collections.Generic;
using SLZ.Props.Weapons;
using SLZ.Interaction;
using System.Linq;
using SLZ.Combat;

namespace CartridgeCaseDespawnUnlimiter
{
    internal class UI
    {
        private static FunctionElement toggle;

        public static void Initialize()
        {
            MenuCategory mainCategory = MenuManager.CreateCategory(SharedValues.ModName, SharedValues.MainMenuColor);
            toggle = mainCategory.CreateFunctionElement("Disable", Color.white, () => ToggleClicked());
            MenuCategory maxCartridgeCategory = mainCategory.CreateCategory("Cartridge Limit", Color.white);
            MenuCategory despawnTimerCategory = mainCategory.CreateCategory("Despawn Timer", Color.white);
            IntElement maxCartridgesCounter = maxCartridgeCategory.CreateIntElement("Max Cartridges", Color.white, SharedValues.MaxCartridges, 1, 0, int.MaxValue, item => ValueEdited<int>(ref SharedValues.MaxCartridges, item, null));
            maxCartridgeCategory.CreateFunctionElement("Add 10", Color.white, () => ValueEdited<int>(ref SharedValues.MaxCartridges, SharedValues.MaxCartridges += 10, maxCartridgesCounter));
            maxCartridgeCategory.CreateFunctionElement("Add 100", Color.white, () => ValueEdited<int>(ref SharedValues.MaxCartridges, SharedValues.MaxCartridges += 100, maxCartridgesCounter));
            maxCartridgeCategory.CreateFunctionElement("Add 1000", Color.white, () => ValueEdited<int>(ref SharedValues.MaxCartridges, SharedValues.MaxCartridges += 1000, maxCartridgesCounter));
            maxCartridgeCategory.CreateFunctionElement("Remove 10", Color.white, () => ValueEdited<int>(ref SharedValues.MaxCartridges, Math.Max(0, SharedValues.MaxCartridges - 10), maxCartridgesCounter));
            maxCartridgeCategory.CreateFunctionElement("Remove 100", Color.white, () => ValueEdited<int>(ref SharedValues.MaxCartridges, Math.Max(0, SharedValues.MaxCartridges - 100), maxCartridgesCounter));
            maxCartridgeCategory.CreateFunctionElement("Remove 1000", Color.white, () => ValueEdited<int>(ref SharedValues.MaxCartridges, Math.Max(0, SharedValues.MaxCartridges - 1000), maxCartridgesCounter));
            maxCartridgeCategory.CreateFunctionElement("Despawn All", SharedValues.MenuWarningColor, () => DespawnAllClicked());
            maxCartridgeCategory.CreateSubPanel("NOTE: If you wanna decrease the Cartridge", SharedValues.MenuWarningColor);
            maxCartridgeCategory.CreateSubPanel("Limit at any given time, you have to despawn all", SharedValues.MenuWarningColor);
            maxCartridgeCategory.CreateSubPanel("cartridges to see the effect.", SharedValues.MenuWarningColor);
            FloatElement delayTimerCounter = despawnTimerCategory.CreateFloatElement("Despawn Timer (Seconds)", Color.white, SharedValues.DespawnTimerInSeconds, 1, 0, 86400, item => ValueEdited<float>(ref SharedValues.DespawnTimerInSeconds, item, null));
            despawnTimerCategory.CreateFunctionElement("Add 1m", Color.white, () => ValueEdited<float>(ref SharedValues.DespawnTimerInSeconds, SharedValues.DespawnTimerInSeconds += 60, delayTimerCounter));
            despawnTimerCategory.CreateFunctionElement("Add 10m", Color.white, () => ValueEdited<float>(ref SharedValues.DespawnTimerInSeconds, SharedValues.DespawnTimerInSeconds += 600, delayTimerCounter));
            despawnTimerCategory.CreateFunctionElement("Add 1h", Color.white, () => ValueEdited<float>(ref SharedValues.DespawnTimerInSeconds, SharedValues.DespawnTimerInSeconds += 3600, delayTimerCounter));
            despawnTimerCategory.CreateFunctionElement("Remove 1m", Color.white, () => ValueEdited<float>(ref SharedValues.DespawnTimerInSeconds, Math.Max(0, SharedValues.DespawnTimerInSeconds - 60), delayTimerCounter));
            despawnTimerCategory.CreateFunctionElement("Remove 10m", Color.white, () => ValueEdited<float>(ref SharedValues.DespawnTimerInSeconds, Math.Max(0, SharedValues.DespawnTimerInSeconds - 600), delayTimerCounter));
            despawnTimerCategory.CreateFunctionElement("Remove 1h", Color.white, () => ValueEdited<float>(ref SharedValues.DespawnTimerInSeconds, Math.Max(0, SharedValues.DespawnTimerInSeconds - 3600), delayTimerCounter));
        }

        private static void ToggleClicked()
        {
            SharedValues.ModEnabled = !SharedValues.ModEnabled;
            toggle.SetName(SharedValues.ModEnabled ? "Disable" : "Enable");

            lock (SharedValues.SyncLock)
            {
                // Remove destroyed Guns first
                List<Gun> toDelete = new List<Gun>();
                foreach (var gun in SharedValues.GrabbedGuns.Keys)
                {
                    if (gun == null)  // destroyed GameObject. It's not actually "null", just compares to "null"
                    {
                        toDelete.Add(gun);
                    }
                }
                toDelete.ForEach(gun => SharedValues.GrabbedGuns.Remove(gun));


                foreach (var gun in SharedValues.GrabbedGuns.Keys)
                {
                    if (SharedValues.ModEnabled)
                    {
                        gun.defaultCartridge.cartridgeCaseSpawnable.policyData.maxSize = SharedValues.MaxCartridges;
                        System.Array.ForEach(Resources.FindObjectsOfTypeAll<FirearmCartridge>().ToArray(), item => item.despawnDelaySeconds = SharedValues.DespawnTimerInSeconds);
                    }
                    else
                    {
                        // Restore previous policyData.maxSize values
                        gun.defaultCartridge.cartridgeCaseSpawnable.policyData.maxSize = SharedValues.GrabbedGuns[gun];
                        System.Array.ForEach(Resources.FindObjectsOfTypeAll<FirearmCartridge>().ToArray(), item => item.despawnDelaySeconds = SharedValues.DefaultDespawnTimer);
                    }
                }
                if (SharedValues.Debug)
                {
                    foreach (var gun in SharedValues.GrabbedGuns.Keys)
                    {
                        Melon<Main>.Logger.Msg("---------------------------------------------------------------------------");
                        Melon<Main>.Logger.Msg($"Gun {gun.name} has spawnPolicData {gun.defaultCartridge.cartridgeCaseSpawnable.policyData.maxSize} after {(SharedValues.ModEnabled ? "enabling" : "disabling")} this mod.");
                    }
                }
            }
        }

        private static void ValueEdited<T>(ref T toEdit, T value, GenericElement<T> menu)
        {
            toEdit = value;
            if (menu != null)
            {
                menu.SetValue(value);
            }

            Array.ForEach<InventorySlot>(Resources.FindObjectsOfTypeAll<InventorySlot>().ToArray(), item =>
            {
                if (item == null) return;
                if (item.itemGameObject == null) return;
                MethodPatches.ObjectGrabbed(item.itemGameObject);
            });
            if (SharedValues.Debug)
            {
                foreach (var gun in SharedValues.GrabbedGuns.Keys)
                {
                    Melon<Main>.Logger.Msg("---------------------------------------------------------------------------");
                    Melon<Main>.Logger.Msg($"Gun {gun.name} has spawnPolicData {gun.defaultCartridge.cartridgeCaseSpawnable.policyData.maxSize} after changing a value in the menu.");
                }
            }
        }

        private static void DespawnAllClicked()
        {
            Array.ForEach(Resources.FindObjectsOfTypeAll<FirearmCartridge>().ToArray(), item => item.Despawn());
        }
    }
}
