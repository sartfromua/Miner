using EquipmentCraft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    /// <summary>
    /// Управляет отображением одного предмета экипировки внутри Frame.
    /// Прикрепляется к префабу FramedObjectPrefab.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class EquipmentFrameUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Transform объекта 'Content' внутри фрейма, куда будет помещаться контент")]
        public Transform contentContainer;

        [Tooltip("Image рамки фрейма (FrameImage), чтобы менять цвет обводки")]
        public Image frameImage;

        [Tooltip("База данных иконок типов экипировки")]
        public EquipmentIconDatabase iconDatabase;

        [Tooltip("Панель детального просмотра предмета")]
        public EquipmentDetailPanel detailPanel;

        [Header("UI Elements")]
        [Tooltip("Image для отображения иконки предмета")]
        public Image iconImage;

        [Tooltip("Text для названия предмета (optional)")]
        public TextMeshProUGUI itemNameText;

        [Tooltip("Text для редкости (optional)")]
        public TextMeshProUGUI rarityText;

        [Tooltip("Text для списка статов (optional)")]
        public TextMeshProUGUI statsText;

        [Header("Rarity Colors")]
        public Color commonColor = Color.gray;
        public Color uncommonColor = Color.green;
        public Color rareColor = Color.blue;
        public Color epicColor = new Color(0.58f, 0.29f, 0.78f); // Purple
        public Color legendaryColor = new Color(1f, 0.5f, 0f); // Orange

        private EquipmentItem currentItem;
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button)
            {
                button.onClick.AddListener(OnClicked);
            }
        }

        /// <summary>
        /// Заполняет фрейм данными предмета экипировки.
        /// </summary>
        public void Setup(EquipmentItem item)
        {
            currentItem = item;

            if (item == null)
            {
                Clear();
                return;
            }

            // Устанавливаем иконку типа экипировки
            if (iconImage && iconDatabase)
            {
                var icon = iconDatabase.GetIcon(item.type);
                if (icon)
                {
                    iconImage.sprite = icon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            // Устанавливаем цвет обводки рамки по редкости
            if (frameImage)
            {
                frameImage.color = GetRarityColor(item.rarity);
            }

            // Устанавливаем название
            if (itemNameText)
                itemNameText.text = item.itemName;

            // Устанавливаем редкость с цветом
            if (rarityText)
            {
                rarityText.text = item.RarityName;
                rarityText.color = GetRarityColor(item.rarity);
            }

            // Формируем текст статов
            if (statsText)
            {
                if (item.stats.Count > 0)
                {
                    var statsString = "";
                    foreach (var stat in item.stats)
                    {
                        statsString += $"{stat.displayName}: +{stat.value:F1}\n";
                    }
                    statsText.text = statsString.TrimEnd();
                }
                else
                {
                    statsText.text = "No stats";
                }
            }
        }

        /// <summary>
        /// Очищает фрейм (для пустых слотов).
        /// </summary>
        public void Clear()
        {
            currentItem = null;

            if (iconImage)
                iconImage.enabled = false;

            if (frameImage)
                frameImage.color = Color.white; // Сбрасываем цвет рамки

            if (itemNameText)
                itemNameText.text = "";

            if (rarityText)
                rarityText.text = "";

            if (statsText)
                statsText.text = "";
        }

        /// <summary>
        /// Возвращает цвет для указанной редкости.
        /// </summary>
        private Color GetRarityColor(int rarity)
        {
            return rarity switch
            {
                1 => commonColor,
                2 => uncommonColor,
                3 => rareColor,
                4 => epicColor,
                5 => legendaryColor,
                _ => Color.white
            };
        }

        /// <summary>
        /// Обработчик клика по фрейму. Открывает панель детального просмотра.
        /// </summary>
        private void OnClicked()
        {
            // Открываем панель только если есть предмет
            if (currentItem != null && detailPanel != null)
            {
                detailPanel.Show(currentItem);
            }
        }
    }
}
