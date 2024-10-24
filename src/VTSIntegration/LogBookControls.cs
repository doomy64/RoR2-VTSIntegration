using RoR2;
using System.Collections.Generic;
using RoR2.ExpansionManagement;
using static VTSIntegration.VTSConfig;
using static VTSIntegration.VTSApi;
using static VTSIntegration.VTSIntegration;
using UnityEngine.EventSystems;
using UnityEngine;

namespace VTSIntegration
{
    internal class LogBookControls
    {
        public static bool detourEntry = false;
        public static ItemIndex lastEntryItem;
        public static EquipmentIndex lastEntryEquipment;

        public static void Init()
        {
            On.RoR2.UI.LogBook.LogBookController.ViewEntry += LogBookController_ViewEntry;
            On.RoR2.UI.LogBook.LogBookController.CanSelectItemEntry += LogBookController_CanSelectItemEntry;
            On.RoR2.UI.LogBook.LogBookController.CanSelectEquipmentEntry += LogBookController_CanSelectEquipmentEntry;
            On.RoR2.UI.LogBook.LogBookController.GetPickupStatus += LogBookController_GetPickupStatus;
        }

        private static RoR2.UI.LogBook.EntryStatus LogBookController_GetPickupStatus(On.RoR2.UI.LogBook.LogBookController.orig_GetPickupStatus orig, ref RoR2.UI.LogBook.Entry entry, UserProfile viewerProfile)
        {
            object extra = entry.extraData;
            if (extra != null && extra.GetType() == typeof(PickupIndex))
            {
                PickupIndex pickup = (PickupIndex)extra;
                PickupDef def = PickupCatalog.GetPickupDef(pickup);
                ItemIndex item = def.itemIndex;
                ItemDef itemDef = ItemCatalog.GetItemDef(item);
                EquipmentIndex equipment = def.equipmentIndex;
                EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipment);
                string name = (item != ItemIndex.None && itemDef != null) ? itemDef.name : ((equipment != EquipmentIndex.None && equipmentDef != null) ? equipmentDef.name : "null");
                if (forceEntries.Contains(name))
                    return RoR2.UI.LogBook.EntryStatus.Available;
            }

            return orig(ref entry, viewerProfile);
        }

        private static bool LogBookController_CanSelectItemEntry(On.RoR2.UI.LogBook.LogBookController.orig_CanSelectItemEntry orig, ItemDef itemDef, Dictionary<ExpansionDef, bool> expansionAvailability)
        {
            if (itemDef != null && forceEntries.Contains(itemDef.name))
                return true;

            return orig(itemDef, expansionAvailability);
        }

        private static bool LogBookController_CanSelectEquipmentEntry(On.RoR2.UI.LogBook.LogBookController.orig_CanSelectEquipmentEntry orig, EquipmentDef equipmentDef, Dictionary<ExpansionDef, bool> expansionAvailability)
        {
            if (equipmentDef != null && forceEntries.Contains(equipmentDef.name))
                return true;

            return orig(equipmentDef, expansionAvailability);
        }

        private static void LogBookController_ViewEntry(On.RoR2.UI.LogBook.LogBookController.orig_ViewEntry orig, RoR2.UI.LogBook.LogBookController self, RoR2.UI.LogBook.Entry entry)
        {
            if (detourEntry)
            {
                object extra = entry.extraData;
                if (extra != null && extra.GetType() == typeof(PickupIndex))
                {
                    PickupIndex pickup = (PickupIndex)extra;
                    PickupDef def = PickupCatalog.GetPickupDef(pickup);
                    lastEntryItem = def.itemIndex;
                    lastEntryEquipment = def.equipmentIndex;
                }
            }
            else
            {
                orig(self, entry);
            }
        }

        public static void GetIndiciesFromButton(RoR2.UI.HGButton button)
        {
            detourEntry = true;
            button.onClick.Invoke();
            detourEntry = false;
        }

        public static void HandleLogbookInteractions()
        {
            //TODO Surely a better way of doing this?
            bool isF2 = Input.GetKeyDown(KeyCode.F2); //Send/take this item to/from VTS as though it were just picked up/removed
            bool isF3 = Input.GetKeyDown(KeyCode.F3); //Toggle whether or not this item should appear in front of the model
            //TODO Indicator of when this feature is active
            bool isF4 = Input.GetKeyDown(KeyCode.F4); //Setup an item to be replaced by a custom image, by loading an item in VTS after pressing this button
            bool isCtrl = Input.GetKey(KeyCode.LeftControl);
            bool isShift = Input.GetKey(KeyCode.LeftShift);
            float scroll = Input.mouseScrollDelta.y;  //Grow/shrink/rotate an item (ONLY while loaded in VTS)
            if ((isF2 || isF3 || isF4 || scroll != 0.0f) && EventSystem.current.IsPointerOverGameObject())
            {
                GameObject curObject = EventSystem.current.currentSelectedGameObject;
                RoR2.UI.HGButton button = curObject.GetComponent<RoR2.UI.HGButton>();
                if (button != null && button.name.StartsWith("ItemEntryIcon"))
                {
                    GetIndiciesFromButton(button);
                    ItemIndex item = lastEntryItem;
                    EquipmentIndex equipment = lastEntryEquipment;
                    ItemDef itemDef = ItemCatalog.GetItemDef(item);
                    EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipment);
                    bool isEquip = equipmentDef != null;
                    string name = isEquip ? equipmentDef.name : itemDef.name;
                    ItemConfig config = itemConfig[name];
                    if (isF2)
                    {
                        blockedItems.Remove(name);
                        if (isEquip)
                        {
                            if (lastEquipment == equipment)
                            {
                                OnEquipmentRemoved(equipment);
                                lastEquipment = EquipmentIndex.None;
                            }
                            else
                            {
                                OnEquipmentPickup(equipment);
                                if (lastEquipment != EquipmentIndex.None)
                                    OnEquipmentRemoved(lastEquipment);
                                lastEquipment = equipment;
                            }
                        }
                        else
                        {
                            if (lastInventory.Contains(item))
                            {
                                OnItemRemoved(item);
                                lastInventory.Remove(item);
                            }
                            else
                            {
                                OnItemPickup(item);
                                lastInventory.Add(item);
                            }
                        }
                    }
                    else if (isF3)
                    {

                        backgroundItems.Set(name, !backgroundItems.Contains(name));
                        if (OrderAvailable(name))
                            MoveItem(name, new Vector2(-1000.0f, -1000.0f), config.rotation.Value, config.size.Value, ClaimOrder(name));

                    }
                    else if (isF4)
                    {
                        if (isCtrl)
                        {
                            config.imageOverride.Value = "null";
                            VTSApi.cachedItems.Remove(name);
                            if (VTSApi.loadedItems.ContainsKey(name))
                            {
                                UnloadItem(name);
                                LoadItem(name, isEquip ? equipmentDef.pickupIconSprite.texture : itemDef.pickupIconSprite.texture);
                            }
                            itemToReplace = string.Empty;
                        }
                        else
                        {
                            if (itemToReplace.Equals(name))
                                itemToReplace = string.Empty;
                            else
                                itemToReplace = name;
                        }
                    }
                    //Don't allow changing these if the item isn't loaded
                    else if (loadedItems.ContainsKey(name))
                    {
                        int mult = isShift ? 5 : 1;
                        if (isCtrl)
                        {
                            //Rotation
                            config.rotation.Value += (scroll < 0.0f ? -1 : 1)*mult;
                            config.rotation.Value %= 360;
                        }
                        else
                        {
                            float size = config.size.Value;
                            //Scale
                            size += (scroll < 0.0f ? -0.01f : 0.01f)*mult;
                            size = Mathf.Max(0.0f, size);
                            size = Mathf.Min(1.0f, size);
                            config.size.Value = size;
                        }
                        MoveItem(name, new Vector2(-1000.0f, -1000.0f), config.rotation.Value, config.size.Value);
                    }
                }
            }
        }
    }
}
