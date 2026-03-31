using System;
using System.Collections.Generic;
using EquipmentCraft;
using Newtonsoft.Json;

[Serializable]
public class PlayerData
{
    public int score = 0;
    public int money = 0;
    public int damage = 0;
    public int blocksBroken = 0;
    public string playerName;
    // Ключ — ID руды, значение — количество
    public Dictionary<string, int> OresInventory = new Dictionary<string, int>();
    public Dictionary<string, int> RefinedInventory = new Dictionary<string, int>();
    // Ключ — ID апгрейда, значение — текущий уровень
    public Dictionary<string, int> Upgrades = new Dictionary<string, int>();
    
    public EquipmentCraftState equipmentCraftState = new EquipmentCraftState();

    /// <summary>Инвентарь предметов экипировки, полученных через крафт.</summary>
    public List<EquipmentItem> equipmentInventory = new List<EquipmentItem>();

    /// <summary>Экипированные предметы по типам (только по 1 предмету каждого типа).</summary>
    public Dictionary<EquipmentType, EquipmentItem> equippedItems = new Dictionary<EquipmentType, EquipmentItem>();

    // === СИСТЕМА ОФФЛАЙН-ПЛАВКИ ===
    [Serializable]
    public class FurnaceSlotData
    {
        public string slotId = "";           // уникальный ID слота (например "Furnace")
        public long startTimeUnix = 0;       // 0 = крафт не запущен
    }

    public List<FurnaceSlotData> craftSlots = new List<FurnaceSlotData>();
    
    public PlayerData()
    {
        score = 0;
        money = 0;
        damage = 1;
        playerName = "NewbieMiner";
    }

    public override string ToString()
    {
        return playerName + " \nMoney: " + money + 
               " \nOres: " + JsonConvert.SerializeObject(OresInventory, Formatting.Indented) + 
               " \nUpgrades: " + JsonConvert.SerializeObject(Upgrades, Formatting.Indented);
    }
}