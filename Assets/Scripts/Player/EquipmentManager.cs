using System.Collections.Generic;
using EquipmentCraft;
using UnityEngine;

public partial class GameDataManager
{
    #region ====== EQUIPMENT MANAGEMENT ======

    /// <summary>
    /// Экипирует предмет из инвентаря. Если предмет этого типа уже экипирован, снимает его в инвентарь.
    /// </summary>
    /// <param name="item">Предмет для экипировки (должен быть в equipmentInventory)</param>
    /// <returns>true если успешно экипировано, false если предмет не найден в инвентаре</returns>
    public bool EquipItem(EquipmentItem item)
    {
        if (item == null || playerData == null)
        {
            Debug.LogWarning("[EquipmentManager] Item или PlayerData равен null");
            return false;
        }

        // Проверяем, что предмет есть в инвентаре
        if (!playerData.equipmentInventory.Contains(item))
        {
            Debug.LogWarning("[EquipmentManager] Предмет не найден в инвентаре");
            return false;
        }

        // Если уже экипирован предмет этого типа, снимаем его
        if (playerData.equippedItems.TryGetValue(item.type, out var currentEquipped))
        {
            // Возвращаем старый предмет в инвентарь
            playerData.equipmentInventory.Add(currentEquipped);
            Debug.Log($"[EquipmentManager] Снят предмет: {currentEquipped.itemName}");
        }

        // Убираем новый предмет из инвентаря
        playerData.equipmentInventory.Remove(item);

        // Экипируем новый предмет
        playerData.equippedItems[item.type] = item;

        Debug.Log($"[EquipmentManager] Экипирован предмет: {item.itemName} ({item.TypeName})");

        OnDataUpdated?.Invoke();
        return true;
    }

    /// <summary>
    /// Снимает экипированный предмет и возвращает его в инвентарь.
    /// </summary>
    /// <param name="type">Тип предмета для снятия</param>
    /// <returns>true если успешно снято, false если ничего не было экипировано</returns>
    public bool UnequipItem(EquipmentType type)
    {
        if (playerData == null)
        {
            Debug.LogWarning("[EquipmentManager] PlayerData равен null");
            return false;
        }

        if (!playerData.equippedItems.TryGetValue(type, out var item))
        {
            Debug.LogWarning($"[EquipmentManager] Нет экипированного предмета типа {type}");
            return false;
        }

        // Убираем из экипированных
        playerData.equippedItems.Remove(type);

        // Возвращаем в инвентарь
        playerData.equipmentInventory.Add(item);

        Debug.Log($"[EquipmentManager] Снят предмет: {item.itemName}");

        OnDataUpdated?.Invoke();
        return true;
    }

    /// <summary>
    /// Проверяет, экипирован ли предмет этого типа.
    /// </summary>
    public bool IsEquipped(EquipmentType type)
    {
        return playerData?.equippedItems.ContainsKey(type) ?? false;
    }

    /// <summary>
    /// Получает экипированный предмет указанного типа.
    /// </summary>
    public EquipmentItem GetEquippedItem(EquipmentType type)
    {
        return playerData?.equippedItems.TryGetValue(type, out var item) == true ? item : null;
    }

    /// <summary>
    /// Получает список всех экипированных предметов.
    /// </summary>
    public List<EquipmentItem> GetAllEquippedItems()
    {
        var result = new List<EquipmentItem>();
        if (playerData?.equippedItems != null)
        {
            foreach (var kvp in playerData.equippedItems)
            {
                result.Add(kvp.Value);
            }
        }
        return result;
    }

    #endregion
}
