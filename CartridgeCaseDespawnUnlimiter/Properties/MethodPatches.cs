using SLZ.Combat;
using SLZ.Interaction;
using SLZ.Props.Weapons;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using MelonLoader;

namespace CartridgeCaseDespawnUnlimiter
{
    internal class MethodPatches
    {
        
        public static void Initialize()
        {
        }

        public static void ObjectGrabbed(GameObject go)
        {
            var array = go.GetComponentsInChildren<Gun>();
            foreach (var item in array)
            {
                if (item == null) continue;
                if (item.defaultCartridge == null) continue;
                if (item.defaultCartridge.cartridgeCaseSpawnable == null) continue;
                if (item.defaultCartridge.cartridgeCaseSpawnable.policyData == null) continue;
                lock (SharedValues.SyncLock)
                {
                    AddToDictionary(item);
                }
                if (SharedValues.ModEnabled)
                {
                    item.defaultCartridge.cartridgeCaseSpawnable.policyData.maxSize = SharedValues.MaxCartridges;
                }
                // if we
                else if (SharedValues.GrabbedGuns.ContainsKey(item))
                {
                    item.defaultCartridge.cartridgeCaseSpawnable.policyData.maxSize = SharedValues.GrabbedGuns[item];

                }
                if (SharedValues.Debug)
                {
                    Melon<Main>.Logger.Msg("---------------------------------------------------------------------------");
                    Melon<Main>.Logger.Msg($"After grabbing it, Gun component: {item.name} has its spawnPolicData changed to {item.defaultCartridge.cartridgeCaseSpawnable.policyData.maxSize}.");
                }
            }

            foreach (var item in array)
            {
                if (item == null) continue;
                if (item.defaultMagazine == null) continue;
                if (item.defaultMagazine.spawnable == null) continue;
                if (item.defaultMagazine.spawnable.policyData == null) continue;
                item.defaultMagazine.spawnable.policyData.maxSize = SharedValues.MaxCartridges;
            }
            if (SharedValues.ModEnabled)
            {
                System.Array.ForEach(Resources.FindObjectsOfTypeAll<FirearmCartridge>().ToArray(), item => item.despawnDelaySeconds = SharedValues.DespawnTimerInSeconds);
            }
            else
            {
                System.Array.ForEach(Resources.FindObjectsOfTypeAll<FirearmCartridge>().ToArray(), item => item.despawnDelaySeconds = SharedValues.DefaultDespawnTimer);
            }
        }

        private static void AddToDictionary(Gun item)
        {
            bool alreadyAdded = false;
            foreach (var gun in SharedValues.GrabbedGuns.Keys)
            {
                if (item.GetInstanceID() == gun.GetInstanceID())
                {
                    alreadyAdded = true;
                    break;
                }
            }
            if (!alreadyAdded)
            {
                SharedValues.GrabbedGuns.Add(item, item.defaultCartridge.cartridgeCaseSpawnable.policyData.maxSize);
            }
        }

        [HarmonyPatch(typeof(InventorySlot), "Insert")]
        public static class PatchInventorySlotInsert
        {
            [HarmonyPostfix]
            private static void Postfix(ref GameObject go)
            {
                ObjectGrabbed(go);
            }
        }
    }
}
