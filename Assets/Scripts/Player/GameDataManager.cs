using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using Ores;
using Ores.Refined;
using Shop.Upgrades;
using UnityEngine.Serialization; // Needed if you want to use LINQ shortcuts, otherwise List.Find is fine

// All functions that changes Player data
// "Controller"
public partial class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    public PlayerData playerData = new PlayerData();
    public OreDatabase oreDataBase; // Reference to your ScriptableObject
    public RefinedDatabase refinedDataBase; // Reference to your ScriptableObject
    public UpgradesDataBase upgradesDataBase; // Reference to your ScriptableObject

    // Event for UI updates
    public System.Action OnDataUpdated;
    public System.Action OnDataLoaded;
    public System.Action OnUpgradesUpdated;

    private bool _needsSaving = false; // Флаг: "Данные изменились, надо сохранить"
    [SerializeField] private float autoSaveInterval = 5f; // Сохраняем не чаще чем раз в 5 сек

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Когда данные меняются, просто поднимаем флаг
        OnDataUpdated += () => _needsSaving = true;
        OnDataLoaded += () => Debug.Log("User logged in");

        // Запускаем таймер автосохранения
        StartCoroutine(AutoSaveRoutine());
    }

    // Корутина, которая работает бесконечно
    private System.Collections.IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            // Ждем 5 секунд
            yield return new WaitForSeconds(autoSaveInterval);

            // Если флаг поднят - сохраняем
            if (_needsSaving)
            {
                Debug.Log("AutoSave: Отправка данных на PlayFab...");
                PlayFabService.SavePlayerData(playerData);
                _needsSaving = false; // Сбрасываем флаг
            }
        }
    }

    // Обязательно сохраняем при закрытии игры/сворачивании на телефон
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && _needsSaving)
        {
            PlayFabService.SavePlayerData(playerData);
        }
    }

    private void OnApplicationQuit()
    {
        if (_needsSaving)
        {
            PlayFabService.SavePlayerData(playerData);
        }
    }

    public void UpdatePlayerData(PlayerData newProgress)
    {
        playerData = newProgress;
        OnDataUpdated?.Invoke();
        Debug.Log("User updated data" + JsonConvert.SerializeObject(playerData));
    }

    public void UpdateOresData(Dictionary<string, OreServerData> newOres)
    {
        // Проходим по всем рудам, которые есть у нас в базе (ScriptableObject)
        foreach (var ore in oreDataBase.allOres)
        {
            // Проверяем, прислал ли сервер настройки для этой руды (по ID)
            if (!newOres.TryGetValue(ore.oreId, out var serverSettings)) continue;
            // Применяем новые значения
            ore.chance = serverSettings.chance;
            ore.durability = serverSettings.durability;
            ore.price = serverSettings.price;
        }

        Debug.Log("Данные руд обновлены с сервера!");
        OnDataUpdated?.Invoke(); // Уведомляем игру, что цифры изменились
    }

    #region ====== АПГРЕЙДЫ ======

    public void UpdateUpgradesData(Dictionary<string, UpgradeServerData> newUpgrades)
    {
        foreach (var upgrade in upgradesDataBase.allUpgrades)
        {
            if (!newUpgrades.TryGetValue(upgrade.name, out var serverSettings)) continue;
            // Применяем новые значения
            upgrade.price = serverSettings.price;
            upgrade.maxLevel = serverSettings.maxLevel;
        }

        Debug.Log("Данные улучшений обновлены с сервера!");
        OnDataUpdated?.Invoke(); // Уведомляем игру, что цифры изменились
        OnUpgradesUpdated?.Invoke();
    }

    public int GetUpgradePrice(UpgradeName upgradeName)
    {
        var upgradeData = upgradesDataBase.GetUpgradeInfoByName(upgradeName);
        var level = GetUpgradeLevel(upgradeName);

        int basePrice = upgradeData.price * (1 + level);

        // Применяем скидку от экипировки (statId: 6 = Shop discount)
        // Формула: базовая цена / (1 + бонус скидки)
        // Например: 100 / (1 + 0.1) = 90
        float discountBonus = GetEquipmentStat("6");
        int finalPrice = (int)(basePrice / (1f + discountBonus));

        return Mathf.Max(1, finalPrice); // Минимальная цена 1
    }

    public void BuyUpgrade(UpgradeName upgradeName)
    {
        var upgradeData = upgradesDataBase.GetUpgradeInfoByName(upgradeName);
        if (!upgradeData) return;
        var price = GetUpgradePrice(upgradeName);
        // 1. Проверка денег
        if (playerData.money >= price)
        {
            var currentLevel = GetUpgradeLevel(upgradeName);
            if (currentLevel >= upgradeData.maxLevel) return;
            // 2. Снимаем деньги
            playerData.money -= price;

            // 3. Уровни апгрейдов
            if (!playerData.Upgrades.TryAdd(upgradeName, 1))
                playerData.Upgrades[upgradeName] += 1;

            GameDataManager.Instance.AddScore(25);


            // 4. Сообщаем всем, что данные изменились (UI обновится сам)
            OnDataUpdated?.Invoke();
            OnUpgradesUpdated?.Invoke();

            Debug.Log($"Куплен апгрейд: {upgradeName}. Остаток: {playerData.money}");
        }
        else
        {
            Debug.Log("Недостаточно денег!");
        }
    }

    public int GetUpgradeLevel(UpgradeName upgradeName)
    {
        playerData.Upgrades.TryGetValue(upgradeName.Value, out var currentLevel);
        return currentLevel;
    }

    #endregion

    #region ====== КОНСТАНТЫ ПОСЛЕ АПГРЕЙДОВ ======

    public int GetBrokenBlocks()
    {
        return playerData.blocksBroken;
    }
    
    public int GetMoney()
    {
        return playerData.money;
    }
    
    public int GetScore()
    {
        return playerData.score;
    }
    
    public int GetDamage()
    {
        var totalDamage = 1;
        totalDamage += GetUpgradeLevel(UpgradeName.Pickaxe);

        // Добавляем бонус от экипировки (statId: 4 = Pickaxe damage)
        totalDamage += (int)GetEquipmentStat("4");

        return totalDamage;
    }

    /// <summary>
    /// Получает урон от автоматической кирки (урон в секунду).
    /// </summary>
    public float GetAutoPickaxeDamage()
    {
        // Урон от апгрейда AutoPickaxe
        float upgradeDamage = GetUpgradeLevel(UpgradeName.AutoPickaxe);

        // Урон от экипировки (statId: 1 = Auto pickaxe)
        float equipmentDamage = GetEquipmentStat("1");

        return upgradeDamage + equipmentDamage;
    }

    /// <summary>
    /// Получает уровень автоплавки (количество руд, которые автоматически плавятся).
    /// </summary>
    public int GetAutoSmeltLevel()
    {
        return GetUpgradeLevel(UpgradeName.AutoSmelt);
    }

    /// <summary>
    /// Получает список ID руд, которые автоматически плавятся (по порядку из базы данных).
    /// </summary>
    public List<string> GetAutoSmeltOreIds()
    {
        int level = GetAutoSmeltLevel();
        if (level <= 0 || oreDataBase == null || oreDataBase.allOres == null)
            return new List<string>();

        var autoSmeltOres = new List<string>();
        int count = Mathf.Min(level, oreDataBase.allOres.Count);

        for (int i = 0; i < count; i++)
        {
            autoSmeltOres.Add(oreDataBase.allOres[i].oreId);
        }

        return autoSmeltOres;
    }

    /// <summary>
    /// Проверяет, включена ли автоплавка для указанной руды.
    /// </summary>
    public bool IsAutoSmeltEnabled(string oreId)
    {
        return GetAutoSmeltOreIds().Contains(oreId);
    }

    public static float GetOreDurability(string oreId)
    {
        if (!Instance || !Instance.oreDataBase) return 1;

        // Corrected syntax from your snippet
        var setting = Instance.oreDataBase.allOres.Find(x => x.oreId == oreId);

        return setting ? setting.durability : 1f;
    }

    private int GetDoubleHarvestProc()
    {
        const double chancePerLevel = 0.2; // 20% за рівень
        var level = GetUpgradeLevel(UpgradeName.DoubleHarvest);

        var totalValue = level * chancePerLevel;

        var guaranteed = (int)Math.Floor(totalValue);

        var chance = totalValue - guaranteed;

        var bonus = 0;
        if (UnityEngine.Random.value < (float)chance)
        {
            bonus = 1;
        }

        return guaranteed + bonus;
    }

    private float GetSellingOreMultiplier()
    {
        const double multPerLevel = 0.05;
        var level = GetUpgradeLevel(UpgradeName.SellingOresMult);
        float upgradeMultiplier = (float)(1 + multPerLevel * level);

        // Добавляем бонус от экипировки (statId: 5 = Ores Selling price multiplier)
        // Формула: (1 + бонус от апгрейда) * (1 + бонус от экипировки)
        float equipmentBonus = GetEquipmentStat("5");
        return upgradeMultiplier * (1f + equipmentBonus);
    }

    #endregion

    #region ====== ВЗАИМОДЕЙСТВИЕ С ДАННЫМИ ИГРОКА ======

    public void AddScore(int score)
    {
        playerData.score += score;
        OnDataUpdated?.Invoke();
    }

    public int GetOrePrice(string oreId)
    {
        var ore = oreDataBase.allOres.Find(x => x.oreId == oreId);
        return (int)(ore.price * GetSellingOreMultiplier());
    }

    public void SellOre(string oreId, int amount, int pricePerUnit)
    {
        if (!playerData.OresInventory.TryGetValue(oreId, out var currentAmount)) return;

        if (currentAmount < amount) return;
        // 1. Отнимаем руду
        playerData.OresInventory[oreId] -= amount;

        playerData.money += (int)(amount * pricePerUnit * GetSellingOreMultiplier());

        Debug.Log($"Продано {amount} шт. {oreId}. Заработано {amount * pricePerUnit} монет.");

        // 3. Сообщаем всем, что данные изменились (UI обновится сам)
        OnDataUpdated?.Invoke();
    }

    public void AddMoney(int amount)
    {
        playerData.money += amount;

        Debug.Log($"Позорище, читерит в кликере... Держи свои грязные {amount}$");

        // 3. Сообщаем всем, что данные изменились (UI обновится сам)
        OnDataUpdated?.Invoke();
    }

    public void AddOre(string oreId, int amount = 1)
    {
        amount += GetDoubleHarvestProc();
        if (!playerData.OresInventory.TryAdd(oreId, amount))
            playerData.OresInventory[oreId] += amount;
        playerData.blocksBroken += amount;

        OnDataUpdated?.Invoke();
    }

    #endregion

    #region ====== ОФФЛАЙН КРАФТ ======

    /// <summary>
    /// Получает скорректированную длительность крафта печи с учетом бонуса от экипировки.
    /// </summary>
    /// <param name="baseDuration">Базовая длительность в секундах</param>
    /// <returns>Скорректированная длительность (уменьшенная при наличии бонуса)</returns>
    private float GetAdjustedFurnaceDuration(float baseDuration)
    {
        // Получаем бонус от экипировки (statId: 3 = Furnace speed)
        // Формула: базовая длительность / (1 + бонус)
        // Например: 100 сек / (1 + 0.2) = 83.33 сек
        float speedBonus = GetEquipmentStat("3");
        return baseDuration / (1f + speedBonus);
    }

    public PlayerData.FurnaceSlotData GetFurnaceSlot(string slotId)
    {
        var slot = playerData.craftSlots.Find(s => s.slotId == slotId);
        if (slot != null) return slot;
        slot = new PlayerData.FurnaceSlotData { slotId = slotId };
        playerData.craftSlots.Add(slot);
        return slot;
    }

    public bool StartFurnaceCraft(string slotId, string inputOreId, int inputAmount, float durationSeconds)
    {
        var slot = GetFurnaceSlot(slotId);
        if (slot.startTimeUnix > 0) return false; // уже идёт крафт

        if (!playerData.OresInventory.TryGetValue(inputOreId, out var count) || count < inputAmount)
        {
            Debug.LogWarning($"Недостаточно {inputOreId} для крафта!");
            return false;
        }

        playerData.OresInventory[inputOreId] -= inputAmount;

        slot.startTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        ;
        OnDataUpdated?.Invoke();
        return true;
    }

    public float GetFurnaceCraftProgress(string slotId, float durationSeconds)
    {
        var slot = GetFurnaceSlot(slotId);
        if (slot.startTimeUnix == 0) return 0f;

        // Применяем бонус скорости печи
        float adjustedDuration = GetAdjustedFurnaceDuration(durationSeconds);

        var currentUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var elapsed = currentUnix - slot.startTimeUnix;
        return Mathf.Clamp01((float)elapsed / adjustedDuration);
    }

    public bool IsFurnaceCraftReady(string slotId, float durationSeconds)
    {
        var slot = GetFurnaceSlot(slotId);
        if (slot.startTimeUnix == 0) return false;

        // Применяем бонус скорости печи
        float adjustedDuration = GetAdjustedFurnaceDuration(durationSeconds);

        var elapsed = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds - slot.startTimeUnix;
        return elapsed >= (long)adjustedDuration;
    }

    public bool ClaimFurnaceCraft(string slotId, string outputOreId, int outputAmount)
    {
        var slot = GetFurnaceSlot(slotId);
        if (slot.startTimeUnix == 0) return false;

        // Здесь duration не нужен, т.к. проверка уже в IsCraftReady
        if (!playerData.RefinedInventory.TryAdd(outputOreId, outputAmount))
            playerData.RefinedInventory[outputOreId] += outputAmount;
        GameDataManager.Instance.AddScore(5);

        slot.startTimeUnix = 0; // сбрасываем
        OnDataUpdated?.Invoke();
        return true;
    }

    #endregion
}