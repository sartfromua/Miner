using System;
using System.Collections.Generic;
using EquipmentCraft;
using UnityEngine;

public partial class GameDataManager
{
    #region ====== REFINED INVENTORY ======

    public int GetRefinedOreAmount(string oreId)
    {
        return playerData.RefinedInventory.GetValueOrDefault(oreId, 0);
    }

    public void AddRefinedOre(string oreId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(oreId) || amount <= 0) return;

        if (!playerData.RefinedInventory.TryAdd(oreId, amount))
            playerData.RefinedInventory[oreId] += amount;

        OnDataUpdated?.Invoke();
    }

    public bool RemoveRefinedOre(string oreId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(oreId) || amount <= 0) return false;
        if (!playerData.RefinedInventory.TryGetValue(oreId, out var currentAmount)) return false;
        if (currentAmount < amount) return false;

        playerData.RefinedInventory[oreId] -= amount;

        if (playerData.RefinedInventory[oreId] <= 0)
            playerData.RefinedInventory.Remove(oreId);

        OnDataUpdated?.Invoke();
        return true;
    }

    #endregion

    #region ====== КРАФТ ЭКИПИРОВКИ ======

    [Header("Equipment Craft")]
    [SerializeField] private EquipmentCraftConfig equipmentCraftConfig;
    [SerializeField] private EquipmentItemDatabase equipmentItemDatabase;

    public EquipmentCraftState EquipmentCraftState()
    {
        return playerData.equipmentCraftState;
    }

    // works only with amount=1
    public bool PutRefinedOreToCraft(int amount = 1)
    {
        var currentOreId = playerData.equipmentCraftState.GetNextOreId();
        if (GetRefinedOreAmount(currentOreId) < amount) return false;
        playerData.equipmentCraftState.AddOreToCraft(amount);
        RemoveRefinedOre(currentOreId, amount);
        return true;
    }

    public bool TakeRefinedOreFromCraft(int amount = 1)
    {
        var oreId = playerData.equipmentCraftState.RemoveOreFromCraft(amount);
        if (oreId == null) return false;
        AddRefinedOre(oreId, amount);
        return true;
    }

    public bool CanStartRefinedCraft()
    {
        return playerData.equipmentCraftState.storedAmount > 0;
    }

    public bool StartRefinedCraft(int amountRequiredPerStep)
    {
        if (!CanStartRefinedCraft())
            return false;

        var state = EquipmentCraftState();
        state.craftStartTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        state.craftDurationSeconds = equipmentCraftConfig.baseCraftDurationSeconds;

        OnDataUpdated?.Invoke();
        return true;
    }

    public float GetRefinedCraftProgress(float durationSeconds)
    {
        if (durationSeconds <= 0f) return 0f;

        var state = EquipmentCraftState();
        if (!state.IsCraftRunning) return 0f;

        var currentUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var elapsed = currentUnix - state.craftStartTimeUnix;

        return Mathf.Clamp01((float)elapsed / durationSeconds);
    }

    public bool IsRefinedCraftReady()
    {
        var durationSeconds = EquipmentCraftState().craftDurationSeconds;
        if (durationSeconds <= 0f) return false;

        var state = EquipmentCraftState();
        if (!state.IsCraftRunning) return false;

        var currentUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var elapsed = currentUnix - state.craftStartTimeUnix;

        return elapsed >= durationSeconds;
    }

    /// <summary>
    /// Завершает крафт и генерирует предмет экипировки.
    /// Возвращает созданный EquipmentItem или null при ошибке.
    /// </summary>
    public EquipmentItem FinishEquipmentCraft()
    {
        if (!IsRefinedCraftReady())
        {
            Debug.LogWarning("[EquipmentCraftManager] Крафт ещё не завершён.");
            return null;
        }

        var state = EquipmentCraftState();
        int resourcesSpent = state.storedAmount;
        float rarityMultiplier = state.craftRarityMultiplier;

        // Добавляем бонус от экипировки (statId: 2 = Craft Chance Multiplier)
        // Формула: 1 + бонус (например, 0.1, 0.2, 0.3 и т.д.)
        float equipmentCraftBonus = GetEquipmentStat("2");
        rarityMultiplier *= (1f + equipmentCraftBonus);

        // Генерируем предмет
        var item = EquipmentCraftGenerator.Generate(
            resourcesSpent,
            rarityMultiplier,
            equipmentCraftConfig,
            equipmentItemDatabase);

        if (item == null) return null;

        // Сохраняем в инвентарь игрока
        playerData.equipmentInventory.Add(item);

        // Сбрасываем состояние крафта
        state.craftStartTimeUnix = 0;
        state.storedAmount = 0;

        OnDataUpdated?.Invoke();
        return item;
    }

    /// <summary>
    /// Устанавливает множитель редкости крафта (например, через апгрейд или бонус).
    /// </summary>
    public void SetCraftRarityMultiplier(float multiplier)
    {
        playerData.equipmentCraftState.craftRarityMultiplier = Mathf.Max(1f, multiplier);
        OnDataUpdated?.Invoke();
    }

    #endregion
}
