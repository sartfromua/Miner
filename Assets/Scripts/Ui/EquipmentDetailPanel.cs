using EquipmentCraft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    /// <summary>
    /// Панель детального описания предмета экипировки с кнопками управления.
    /// Показывается при клике на предмет в инвентаре.
    /// </summary>
    public class EquipmentDetailPanel : MonoBehaviour
    {
        [Header("Prefab")]
        [Tooltip("Префаб FramedObjectPrefab для отображения предмета")]
        public GameObject framedObjectPrefab;

        [Tooltip("Контейнер для размещения префаба")]
        public Transform itemContainer;

        [Header("Text Fields")]
        [Tooltip("Текстовое поле для типа предмета")]
        public TextMeshProUGUI itemTypeText;

        [Tooltip("Текстовое поле для статов предмета")]
        public TextMeshProUGUI itemStatsText;

        [Header("Buttons")]
        [Tooltip("Кнопка экипировки/снятия")]
        public Button equipButton;

        [Tooltip("Текст на кнопке экипировки")]
        public TextMeshProUGUI equipButtonText;

        [Tooltip("Кнопка закрытия панели")]
        public Button closeButton;

        [Header("References")]
        [Tooltip("База данных иконок типов экипировки")]
        public EquipmentIconDatabase iconDatabase;

        private EquipmentItem currentItem;
        private GameObject currentFrameInstance;
        private EquipmentFrameUI currentFrameUI;
        private bool isInitialized = false;

        private void Initialize()
        {
            if (isInitialized)
                return;

            if (equipButton)
                equipButton.onClick.AddListener(OnEquipButtonClicked);

            if (closeButton)
                closeButton.onClick.AddListener(OnCloseButtonClicked);

            isInitialized = true;
            Debug.Log("[EquipmentDetailPanel] Initialized");
        }

        /// <summary>
        /// Показывает панель с информацией о предмете.
        /// </summary>
        public void Show(EquipmentItem item)
        {
            if (item == null)
            {
                Debug.LogWarning("[EquipmentDetailPanel] Попытка показать панель для null предмета");
                return;
            }

            // Ініціалізуємо при першому виклику
            Initialize();

            Debug.Log($"[EquipmentDetailPanel] Show вызван для предмета: {item.itemName}");
            Debug.Log($"[EquipmentDetailPanel] Текущий статус активности панели: {gameObject.activeSelf}");
            Debug.Log($"[EquipmentDetailPanel] Родитель панели: {(transform.parent != null ? transform.parent.name : "null")}");

            currentItem = item;

            // Создаём или обновляем префаб предмета
            SetupItemFrame(item);

            // Тип предмета
            if (itemTypeText)
                itemTypeText.text = item.TypeName;

            // Статы в формате:
            // Rarity: common
            // Stat1: 10
            // Stat2: 20
            if (itemStatsText)
            {
                var statsString = $"Rarity: {item.RarityName}\n";

                if (item.stats.Count > 0)
                {
                    foreach (var stat in item.stats)
                    {
                        statsString += $"{stat.displayName}: {stat.value}\n";
                    }
                }
                else
                {
                    statsString += "No stats";
                }

                itemStatsText.text = statsString.TrimEnd();
            }

            // Обновляем текст кнопки экипировки
            UpdateEquipButton();

            gameObject.SetActive(true);
            Debug.Log($"[EquipmentDetailPanel] SetActive(true) виконано. Новий статус: {gameObject.activeSelf}");
        }

        /// <summary>
        /// Создаёт или обновляет экземпляр FramedObjectPrefab в контейнере.
        /// </summary>
        private void SetupItemFrame(EquipmentItem item)
        {
            if (!framedObjectPrefab || !itemContainer)
            {
                Debug.LogWarning("[EquipmentDetailPanel] framedObjectPrefab или itemContainer не назначены!");
                return;
            }

            // Уничтожаем старый экземпляр, если есть
            if (currentFrameInstance != null)
            {
                Destroy(currentFrameInstance);
            }

            // Создаём новый экземпляр префаба
            currentFrameInstance = Instantiate(framedObjectPrefab, itemContainer);
            currentFrameUI = currentFrameInstance.GetComponent<EquipmentFrameUI>();

            if (currentFrameUI == null)
            {
                Debug.LogWarning("[EquipmentDetailPanel] Префаб не содержит компонент EquipmentFrameUI!");
                return;
            }

            // Передаём базу иконок
            if (iconDatabase)
            {
                currentFrameUI.iconDatabase = iconDatabase;
            }

            // Заполняем данными предмета
            currentFrameUI.Setup(item);

            // Отключаем кнопку на фрейме (чтобы не было конфликтов с кликами)
            var button = currentFrameInstance.GetComponent<Button>();
            if (button)
            {
                button.interactable = false;
            }
        }

        /// <summary>
        /// Скрывает панель.
        /// </summary>
        public void Hide()
        {
            currentItem = null;

            // Уничтожаем экземпляр префаба при закрытии
            if (currentFrameInstance != null)
            {
                Destroy(currentFrameInstance);
                currentFrameInstance = null;
                currentFrameUI = null;
            }

            gameObject.SetActive(false);
        }

        private void OnEquipButtonClicked()
        {
            if (currentItem == null || !GameDataManager.Instance)
                return;

            // Проверяем, экипирован ли этот предмет
            var equippedItem = GameDataManager.Instance.GetEquippedItem(currentItem.type);
            bool isThisItemEquipped = equippedItem == currentItem;

            if (isThisItemEquipped)
            {
                // Снимаем предмет
                bool success = GameDataManager.Instance.UnequipItem(currentItem.type);

                if (success)
                {
                    Debug.Log($"[EquipmentDetailPanel] Предмет снят: {currentItem.itemName}");
                    Hide(); // Закрываем панель после снятия
                }
                else
                {
                    Debug.LogWarning("[EquipmentDetailPanel] Не удалось снять предмет");
                }
            }
            else
            {
                // Экипируем предмет
                bool success = GameDataManager.Instance.EquipItem(currentItem);

                if (success)
                {
                    Debug.Log($"[EquipmentDetailPanel] Предмет экипирован: {currentItem.itemName}");
                    Hide(); // Закрываем панель после экипировки
                }
                else
                {
                    Debug.LogWarning("[EquipmentDetailPanel] Не удалось экипировать предмет");
                }
            }
        }

        private void OnCloseButtonClicked()
        {
            Hide();
        }

        private void UpdateEquipButton()
        {
            if (!equipButton || !equipButtonText || !GameDataManager.Instance)
                return;

            // Проверяем, экипирован ли уже предмет этого типа
            bool isTypeEquipped = GameDataManager.Instance.IsEquipped(currentItem.type);

            if (isTypeEquipped)
            {
                var equippedItem = GameDataManager.Instance.GetEquippedItem(currentItem.type);
                if (equippedItem == currentItem)
                {
                    // Этот предмет уже экипирован (не должно происходить в инвентаре)
                    equipButtonText.text = "Unequip";
                }
                else
                {
                    // Другой предмет этого типа экипирован - будет заменён
                    equipButtonText.text = "Replace & Equip";
                }
            }
            else
            {
                equipButtonText.text = "Equip";
            }
        }
    }
}
