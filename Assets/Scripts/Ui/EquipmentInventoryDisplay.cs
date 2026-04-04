using System.Collections.Generic;
using UnityEngine;

namespace Ui
{
    /// <summary>
    /// Управляет отображением инвентаря экипировки в виде фиксированного грида.
    /// Создает фиксированное количество фреймов (например 32 = 4 ряда × 8 колонок).
    /// Подписывается на события изменения данных для автообновления.
    /// </summary>
    public class EquipmentInventoryDisplay : MonoBehaviour
    {
        [Header("Grid Settings")]
        [Tooltip("Префаб Frame для отображения предметов")]
        public GameObject framePrefab;

        [Tooltip("Контейнер с Grid Layout Group компонентом")]
        public Transform gridContainer;

        [Header("Icon Database")]
        [Tooltip("База данных иконок типов экипировки")]
        public EquipmentCraft.EquipmentIconDatabase iconDatabase;

        [Header("Detail Panel")]
        [Tooltip("Панель детального просмотра предмета")]
        public EquipmentDetailPanel detailPanel;

        [Header("Auto-calculate from AutoGridLayoutSizer")]
        [Tooltip("Автоматически рассчитывать количество слотов из AutoGridLayoutSizer (если есть)")]
        public bool autoCalculateSlots = true;

        [Tooltip("Ручное количество слотов (используется если autoCalculateSlots = false)")]
        public int manualTotalSlots = 32;

        private List<EquipmentFrameUI> frameSlots = new List<EquipmentFrameUI>();

        private void OnEnable()
        {
            // Инициализируем грид при включении
            InitializeGrid();

            // Обновляем отображение
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
            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.OnDataUpdated -= UpdateDisplay;
                GameDataManager.Instance.OnDataLoaded -= UpdateDisplay;
            }
        }

        /// <summary>
        /// Создает фиксированное количество фреймов в контейнере.
        /// Вызывается только один раз при инициализации.
        /// </summary>
        private void InitializeGrid()
        {
            // Если грид уже инициализирован, пропускаем
            if (frameSlots.Count > 0)
                return;

            if (!framePrefab || !gridContainer)
            {
                Debug.LogError("[EquipmentInventoryDisplay] framePrefab или gridContainer не назначены!");
                return;
            }

            // Определяем количество слотов
            var totalSlots = GetTotalSlots();

            // Создаем фиксированное количество слотов
            for (var i = 0; i < totalSlots; i++)
            {
                var frameObject = Instantiate(framePrefab, gridContainer);
                var frameUI = frameObject.GetComponent<EquipmentFrameUI>();

                if (frameUI == null)
                {
                    Debug.LogWarning($"[EquipmentInventoryDisplay] Префаб не содержит компонент EquipmentFrameUI! Добавляю автоматически.");
                    frameUI = frameObject.AddComponent<EquipmentFrameUI>();
                }

                // Передаём ссылку на iconDatabase и detailPanel в каждый фрейм
                if (frameUI)
                {
                    if (iconDatabase)
                        frameUI.iconDatabase = iconDatabase;

                    if (detailPanel)
                        frameUI.detailPanel = detailPanel;

                    // Позначаємо фрейм як інвентарний — в деталях з'явиться кнопка Sell
                    frameUI.isInventoryItem = true;
                }

                frameSlots.Add(frameUI);
            }
        }

        /// <summary>
        /// Возвращает общее количество слотов (автоматически или вручную).
        /// </summary>
        private int GetTotalSlots()
        {
            if (autoCalculateSlots)
            {
                var autoSizer = gridContainer.GetComponent<AutoGridLayoutSizer>();
                if (autoSizer != null)
                {
                    return autoSizer.columns * autoSizer.rows;
                }
                else
                {
                    Debug.LogWarning("[EquipmentInventoryDisplay] AutoGridLayoutSizer не найден на gridContainer! Использую ручное значение.");
                }
            }

            return manualTotalSlots;
        }

        /// <summary>
        /// Обновляет отображение всех слотов на основе текущего инвентаря.
        /// </summary>
        private void UpdateDisplay()
        {
            if (frameSlots.Count == 0)
            {
                Debug.LogWarning("[EquipmentInventoryDisplay] Грид не инициализирован!");
                return;
            }

            if (!GameDataManager.Instance || GameDataManager.Instance.playerData == null)
            {
                ClearAllSlots();
                return;
            }

            var equipmentInventory = GameDataManager.Instance.playerData.equipmentInventory;

            // Заполняем слоты предметами из инвентаря
            for (var i = 0; i < frameSlots.Count; i++)
            {
                if (i < equipmentInventory.Count)
                {
                    // Слот заполнен предметом
                    frameSlots[i].Setup(equipmentInventory[i]);
                }
                else
                {
                    // Слот пустой
                    frameSlots[i].Clear();
                }
            }
        }

        /// <summary>
        /// Очищает все слоты (используется при отсутствии данных).
        /// </summary>
        private void ClearAllSlots()
        {
            foreach (var slot in frameSlots)
            {
                slot.Clear();
            }
        }

        /// <summary>
        /// Полностью пересоздает грид (полезно для тестирования или изменения размера).
        /// </summary>
        public void RebuildGrid()
        {
            // Удаляем старые фреймы
            foreach (var slot in frameSlots)
            {
                if (slot)
                    Destroy(slot.gameObject);
            }

            frameSlots.Clear();

            // Создаем заново
            InitializeGrid();
            UpdateDisplay();
        }
    }
}
