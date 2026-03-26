using System.Collections.Generic;
using EquipmentCraft;
using TMPro;
using UnityEngine;

namespace Ui
{
    /// <summary>
    /// Управляет отображением экипированных предметов в фиксированных слотах по типам.
    /// Показывает 5 фреймов для каждого типа экипировки (Body, Head, Legs, Feet, Pickaxe).
    /// </summary>
    public class EquippedItemsDisplay : MonoBehaviour
    {
        [Header("Equipment Slots")]
        [Tooltip("Слот для Body Armor")]
        public EquipmentFrameUI bodySlot;

        [Tooltip("Слот для Helmet")]
        public EquipmentFrameUI headSlot;

        [Tooltip("Слот для Leg Armor")]
        public EquipmentFrameUI legsSlot;

        [Tooltip("Слот для Boots")]
        public EquipmentFrameUI feetSlot;

        [Tooltip("Слот для Pickaxe")]
        public EquipmentFrameUI pickaxeSlot;

        [Header("References")]
        [Tooltip("База данных иконок типов экипировки")]
        public EquipmentIconDatabase iconDatabase;

        [Tooltip("Панель детального просмотра предмета")]
        public EquipmentDetailPanel detailPanel;

        [Header("Stats Display")]
        [Tooltip("Текстовое поле для отображения суммарных бонусов экипировки")]
        public TextMeshProUGUI totalStatsText;

        private Dictionary<EquipmentType, EquipmentFrameUI> slotsByType;

        private void Awake()
        {
            // Создаём словарь для быстрого доступа к слотам по типу
            slotsByType = new Dictionary<EquipmentType, EquipmentFrameUI>
            {
                { EquipmentType.Body, bodySlot },
                { EquipmentType.Head, headSlot },
                { EquipmentType.Legs, legsSlot },
                { EquipmentType.Feet, feetSlot },
                { EquipmentType.Pickaxe, pickaxeSlot }
            };

            // Настраиваем каждый слот
            SetupSlots();
        }

        private void OnEnable()
        {
            // Обновляем отображение при включении
            UpdateDisplay();

            // Подписываемся на события изменения данных
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.OnDataUpdated += UpdateDisplay;
                GameDataManager.Instance.OnDataLoaded += UpdateDisplay;
            }
        }

        private void OnDisable()
        {
            // Отписываемся от событий
            if (GameDataManager.Instance == null) return;
            GameDataManager.Instance.OnDataUpdated -= UpdateDisplay;
            GameDataManager.Instance.OnDataLoaded -= UpdateDisplay;
        }

        /// <summary>
        /// Настраивает все слоты (передаёт ссылки на iconDatabase и detailPanel).
        /// </summary>
        private void SetupSlots()
        {
            foreach (var kvp in slotsByType)
            {
                var slot = kvp.Value;
                if (slot == null)
                {
                    Debug.LogWarning($"[EquippedItemsDisplay] Слот для типа {kvp.Key} не назначен!");
                    continue;
                }

                // Передаём ссылки
                if (iconDatabase)
                    slot.iconDatabase = iconDatabase;

                if (detailPanel)
                    slot.detailPanel = detailPanel;
            }
        }

        /// <summary>
        /// Обновляет отображение всех слотов на основе экипированных предметов.
        /// </summary>
        private void UpdateDisplay()
        {
            if (!GameDataManager.Instance || GameDataManager.Instance.playerData == null)
            {
                ClearAllSlots();
                UpdateTotalStatsDisplay(null);
                return;
            }

            var equippedItems = GameDataManager.Instance.playerData.equippedItems;

            // Обновляем каждый слот
            foreach (var kvp in slotsByType)
            {
                var equipmentType = kvp.Key;
                var slot = kvp.Value;

                if (slot == null)
                    continue;

                // Проверяем, есть ли экипированный предмет этого типа
                if (equippedItems.TryGetValue(equipmentType, out var item))
                {
                    // Слот заполнен
                    slot.Setup(item);
                }
                else
                {
                    // Слот пустой
                    slot.Clear();
                }
            }

            // Обновляем отображение суммарных статов
            UpdateTotalStatsDisplay(GameDataManager.Instance.GetTotalEquipmentStats());
        }

        /// <summary>
        /// Очищает все слоты (используется при отсутствии данных).
        /// </summary>
        private void ClearAllSlots()
        {
            foreach (var kvp in slotsByType)
            {
                if (kvp.Value != null)
                    kvp.Value.Clear();
            }
        }

        /// <summary>
        /// Обновляет текстовое поле с суммарными бонусами экипировки.
        /// </summary>
        /// <param name="stats">Словарь со статами (statId -> значение). Null если нет данных.</param>
        private void UpdateTotalStatsDisplay(Dictionary<string, float> stats)
        {
            if (totalStatsText == null)
                return;

            if (stats == null || stats.Count == 0)
            {
                totalStatsText.text = "Stats:\n - No equipment";
                return;
            }

            // Формируем текст по шаблону
            var text = "Stats:";

            // Порядок отображения статов (по statId)
            var statOrder = new[] { "1", "2", "3", "4", "5", "6" };
            var statNames = new Dictionary<string, string>
            {
                { "1", "Auto pickaxe" },
                { "2", "Craft Chance Multiplier" },
                { "3", "Furnace speed" },
                { "4", "Pickaxe damage" },
                { "5", "Ores Selling price multiplier" },
                { "6", "Shop discount" }
            };

            foreach (var statId in statOrder)
            {
                if (stats.TryGetValue(statId, out var value))
                {
                    var statName = statNames.TryGetValue(statId, out var name) ? name : $"Stat{statId}";
                    text += $"\n - {statName}: {value:F1}";
                }
                else
                {
                    var statName = statNames.TryGetValue(statId, out var name) ? name : $"Stat{statId}";
                    text += $"\n - {statName}: 0";
                }
            }

            totalStatsText.text = text;
        }
    }
}
