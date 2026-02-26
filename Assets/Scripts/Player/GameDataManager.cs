using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using Ores;
using Shop.Upgrades;
using UnityEngine.Serialization; // Needed if you want to use LINQ shortcuts, otherwise List.Find is fine

// All functions that changes Player data
// "Controller"
public partial class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    public PlayerData playerData = new PlayerData();
    public OreDatabase oreDataBase; // Reference to your ScriptableObject
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

    public static float GetOreDurability(string oreId)
    {
        if (Instance == null || Instance.oreDataBase == null) return 1;

        // Corrected syntax from your snippet
        var setting = Instance.oreDataBase.allOres.Find(x => x.oreId == oreId);

        return setting != null ? setting.durability : 1f; 
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

    public int GetUpgradePrice(UpgradeName upgradeName)
    {
        var upgradeData = upgradesDataBase.GetUpgradeInfoByName(upgradeName);
        var level = GetUpgradeLevel(upgradeName);
        
        return upgradeData.price * (1 + level);
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

    public int GetDamage()
    {
        var totalDamage = 1;
        totalDamage += GetUpgradeLevel(UpgradeName.Pickaxe);
        return totalDamage;
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
        return (float)(1 + multPerLevel * level);
    }

    public void AddMoney(int amount)
    {
        playerData.money += amount;

        Debug.Log($"Позорище, читерит в кликере... Держи свои грязные {amount}$");

        // 3. Сообщаем всем, что данные изменились (UI обновится сам)
        OnDataUpdated?.Invoke();
    }
    
    public void AddOre(string oreId, int amount=1)
    {
        amount += GetDoubleHarvestProc();
        if (!playerData.OresInventory.TryAdd(oreId, amount))
            playerData.OresInventory[oreId] += amount;
        playerData.blocksBroken += amount;
        
        OnDataUpdated?.Invoke();
    }
}