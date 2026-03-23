using System;
using UnityEngine;

namespace EquipmentCraft
{
    /// <summary>
    /// ScriptableObject — конфигурация крафта экипировки.
    /// Определяет пороги ресурсов для каждого уровня редкости и множители статов.
    /// Создать в меню: Data / Equipment / Craft Config
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentCraftConfig", menuName = "Data/Equipment/Craft Config")]
    public class EquipmentCraftConfig : ScriptableObject
    {
        [Serializable]
        public class RarityThreshold
        {
            [Tooltip("Минимальное кол-во ресурсов для этого уровня редкости")]
            public int minResources;

            [Tooltip("Уровень редкости 1–5")]
            [Range(1, 5)]
            public int rarityLevel;
        }

        [Header("Пороги ресурсов → Редкость")]
        [Tooltip("Список порогов. Сортируется по minResources. Берётся наивысшая подходящая редкость.")]
        public RarityThreshold[] rarityThresholds = new RarityThreshold[]
        {
            new RarityThreshold { minResources = 0,   rarityLevel = 1 },
            new RarityThreshold { minResources = 10,  rarityLevel = 2 },
            new RarityThreshold { minResources = 30,  rarityLevel = 3 },
            new RarityThreshold { minResources = 60,  rarityLevel = 4 },
            new RarityThreshold { minResources = 100, rarityLevel = 5 },
        };

        [Header("Множители статов по редкости")]
        [Tooltip("Индекс 0 = редкость 1 (Common), индекс 4 = редкость 5 (Legendary).\n" +
                 "Финальный стат = baseValue * statMultipliers[rarity - 1]")]
        public float[] statMultipliers = new float[] { 1.0f, 1.5f, 2.25f, 3.5f, 5.0f };

        [Header("Время крафта")]
        [Tooltip("Базовое время крафта в секундах. Итоговое = base / craftDurationModifier")]
        public int baseCraftDurationSeconds = 5 * 60;

        [Header("Генерация статов")]
        [Tooltip("Максимальное кол-во статов на одном предмете")]
        [Range(1, 4)]
        public int maxStatsCount = 4;

        /// <summary>
        /// Возвращает базовый уровень редкости (1–5) по количеству вложенных ресурсов.
        /// </summary>
        public int GetBaseRarity(int resourcesSpent)
        {
            int result = 1;
            foreach (var threshold in rarityThresholds)
            {
                if (resourcesSpent >= threshold.minResources && threshold.rarityLevel > result)
                    result = threshold.rarityLevel;
            }
            return Mathf.Clamp(result, 1, 5);
        }

        /// <summary>
        /// Возвращает множитель стата для указанной редкости.
        /// </summary>
        public float GetStatMultiplier(int rarity)
        {
            int index = Mathf.Clamp(rarity - 1, 0, statMultipliers.Length - 1);
            return statMultipliers[index];
        }
    }
}
