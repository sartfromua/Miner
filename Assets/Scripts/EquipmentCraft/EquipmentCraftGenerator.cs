using System.Collections.Generic;
using UnityEngine;

namespace EquipmentCraft
{
    /// <summary>
    /// Генерирует EquipmentItem на основе вложенных ресурсов и множителя редкости крафта.
    /// </summary>
    public static class EquipmentCraftGenerator
    {
        private const int MaxStats = 4;

        /// <summary>
        /// Генерирует предмет экипировки.
        /// </summary>
        /// <param name="resourcesSpent">Количество ресурсов, потраченных на крафт.</param>
        /// <param name="craftRarityMultiplier">Множитель редкости крафта (1.0 = без бонуса, 2.0 = удваивает редкость).</param>
        /// <param name="config">Конфигурация крафта с порогами и множителями статов.</param>
        /// <param name="database">База доступных характеристик.</param>
        /// <returns>Сгенерированный предмет или null при отсутствии статов в базе.</returns>
        public static EquipmentItem Generate(
            int resourcesSpent,
            float craftRarityMultiplier,
            EquipmentCraftConfig config,
            EquipmentItemDatabase database)
        {
            if (config == null || database == null || database.allStats == null || database.allStats.Count == 0)
            {
                Debug.LogWarning("[EquipmentCraftGenerator] Config или Database не назначены / пусты.");
                return null;
            }

            // 1. Базовая редкость по ресурсам
            var baseRarity = config.GetBaseRarity(resourcesSpent);

            // 2. Применяем множитель редкости крафта и округляем
            var effectiveRarityF = baseRarity * Mathf.Max(1f, craftRarityMultiplier);
            var rarity = Mathf.Clamp(Mathf.RoundToInt(effectiveRarityF), 1, 5);

            // 3. Множитель стата для полученной редкости
            var statMultiplier = config.GetStatMultiplier(rarity);

            // 4. Случайно выбираем от 1 до maxStatsCount статов (без повторений)
            var statCount = Mathf.Clamp(
                Random.Range(1, config.maxStatsCount + 1),
                1,
                Mathf.Min(config.maxStatsCount, database.allStats.Count));

            var selectedStats = PickRandomStats(database, statCount);

            // 5. Случайно выбираем тип экипировки
            var equipmentType = (EquipmentType)Random.Range(0, System.Enum.GetValues(typeof(EquipmentType)).Length);

            // 6. Собираем предмет
            var item = new EquipmentItem
            {
                itemName = $"{GetRarityPrefix(rarity)} {GetTypeName(equipmentType)}",
                rarity   = rarity,
                type     = equipmentType,
                stats    = new System.Collections.Generic.List<EquipmentStat>()
            };

            foreach (var def in selectedStats)
            {
                item.stats.Add(new EquipmentStat
                {
                    statId      = def.statId,
                    displayName = def.displayName,
                    value       = Mathf.Round(def.baseValue * statMultiplier * 100f) / 100f
                });
            }

            Debug.Log($"[EquipmentCraftGenerator] Создан предмет: {item.itemName} | " +
                      $"Редкость: {rarity} ({item.RarityName}) | " +
                      $"Ресурсов: {resourcesSpent} | Множитель крафта: {craftRarityMultiplier:F2} | " +
                      $"Статов: {item.stats.Count}");

            return item;
        }

        // ── Helpers ─────────────────────────────────────────────────────────────────

        private static List<EquipmentStatDefinition> PickRandomStats(EquipmentItemDatabase database, int count)
        {
            // Копируем список и перемешиваем (Fisher-Yates)
            var pool = new List<EquipmentStatDefinition>(database.allStats);
            for (var i = pool.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            var result = new List<EquipmentStatDefinition>();
            for (var i = 0; i < count && i < pool.Count; i++)
                result.Add(pool[i]);

            return result;
        }

        private static string GetRarityPrefix(int rarity) => rarity switch
        {
            1 => "Common",
            2 => "Uncommon",
            3 => "Rare",
            4 => "Epic",
            5 => "Legendary",
            _ => "Unknown"
        };

        private static string GetTypeName(EquipmentType type) => type switch
        {
            EquipmentType.Body => "Body Armor",
            EquipmentType.Head => "Helmet",
            EquipmentType.Legs => "Leg Armor",
            EquipmentType.Feet => "Boots",
            EquipmentType.Pickaxe => "Pickaxe",
            _ => "Equipment"
        };
    }
}
