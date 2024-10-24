using BepInEx;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using static VTSIntegration.VTSConfig;
using static VTSIntegration.VTSApi;
using static VTSIntegration.LogBookControls;

namespace VTSIntegration
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class VTSIntegration : BaseUnityPlugin
    {
        public const string PluginGUID = PluginName;
        public const string PluginAuthor = "doomy64";
        public const string PluginName = "VTSIntegration";
        public const string PluginVersion = "1.0.0";

        public static List<ItemIndex> lastInventory = new List<ItemIndex>();
        public static EquipmentIndex lastEquipment = EquipmentIndex.None;
        private static bool updateInv = false;
        
        public void Awake()
        {
            Log.Init(Logger);
            RoR2.CharacterBody.onBodyInventoryChangedGlobal += CharacterBody_onBodyInventoryChanged;
            RoR2.Run.onRunStartGlobal += OnRunStart;
            VTSConfig.Init(this);
            VTSApi.Init();
            LogBookControls.Init();
        }

        private void CharacterBody_onBodyInventoryChanged(CharacterBody body)
        {
            if (!VTSApi.connected)
                return;
            
            if (body == null || LocalUserManager.GetFirstLocalUser().cachedBody != body)
                return;

            UpdateInventory(body);
        }

        private void UpdateInventory(CharacterBody body = null)
        {
            if (body == null)
                body = LocalUserManager.GetFirstLocalUser().cachedBody;
            
            if (body == null)
                return;

            List<ItemIndex> inventory = body.inventory.itemAcquisitionOrder;
            List<ItemIndex> added = inventory.Except(lastInventory).ToList();
            List<ItemIndex> removed = lastInventory.Except(inventory).ToList();
            added.ForEach(OnItemPickup);
            removed.ForEach(OnItemRemoved);
            lastInventory.Clear();
            lastInventory.AddRange(inventory);
            EquipmentIndex equipment = body.inventory.currentEquipmentIndex;
            if (equipment != lastEquipment)
            {
                if (lastEquipment != EquipmentIndex.None)
                    OnEquipmentRemoved(lastEquipment);

                if (equipment != EquipmentIndex.None)
                    OnEquipmentPickup(equipment);
            }
            lastEquipment = equipment;
        }

        public static void OnItemPickup(ItemIndex item)
        {
            if (item == ItemIndex.None) 
                return;

            ItemDef def = ItemCatalog.GetItemDef(item);
            if (blockedItems.Contains(def.name))
                return;

            LoadItem(def.name, def.pickupIconSprite.texture);
        }
        public static void OnItemRemoved(ItemIndex item)
        {
            ItemDef def = ItemCatalog.GetItemDef(item);
            UnloadItem(def.name);
            itemsWaitingForSpace.Remove(def.name);
        }

        private void Update()
        {
            itemsWaitingToAdd.ForEach((s) =>
            {
                EquipmentIndex equipment = EquipmentCatalog.FindEquipmentIndex(s);
                ItemIndex item = ItemCatalog.FindItemIndex(s);
                if (equipment != EquipmentIndex.None)
                    OnEquipmentPickup(equipment);
                else
                    OnItemPickup(item);
            });
            itemsWaitingToAdd.Clear();

            if (updateInv && LocalUserManager.GetFirstLocalUser().cachedBody != null)
            {
                UpdateInventory();
                updateInv = false;
            }

            HandleLogbookInteractions();
        }

        

        public void OnRunStart(Run run)
        {
            VTSApi.itemsWaitingForSpace.Clear();
            lastInventory.ForEach(OnItemRemoved);
            lastInventory.Clear();
            OnEquipmentRemoved(lastEquipment);
            lastEquipment = EquipmentIndex.None;
            updateInv = true;
        }

        public static void OnEquipmentPickup(EquipmentIndex equipment)
        {
            if (equipment == EquipmentIndex.None)
                return;

            EquipmentDef def = EquipmentCatalog.GetEquipmentDef(equipment);
            if (blockedItems.Contains(def.name))
                return;

            LoadItem(def.name, def.pickupIconSprite.texture);
        }

        public static void OnEquipmentRemoved(EquipmentIndex equipment)
        {
            if (equipment == EquipmentIndex.None)
                return;
            
            EquipmentDef def = EquipmentCatalog.GetEquipmentDef(equipment);
            UnloadItem(def.name);
            VTSApi.itemsWaitingForSpace.Remove(def.name);
        }
    }

}
