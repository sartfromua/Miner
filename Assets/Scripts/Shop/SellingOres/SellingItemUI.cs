using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop.SellingOres
{
    public class SellingItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image iconImage;
        public TextMeshProUGUI amountText;
        public TextMeshProUGUI priceText;
    
        [Header("Buttons")]
        public Button sellOneButton;
        public Button sellAllButton;

        private OreData _data;
        private int _currentAmount;

        public void Setup(OreData data, int currentAmount)
        {
            _data = data;
            _currentAmount = currentAmount;
            var price = GameDataManager.Instance.GetOrePrice(data.oreId); 

            // 1. Заполняем визуал
            iconImage.sprite = data.icon;
            // nameText.text = data.oreId; // Или data.displayName, если есть
            priceText.text = $"Price: {price}";
            amountText.text = $"x{currentAmount}";

            // 2. Настраиваем кнопки
            // Сначала удаляем старые подписки, чтобы не дублировались
            sellOneButton.onClick.RemoveAllListeners();
            sellAllButton.onClick.RemoveAllListeners();

            // Если руды нет (0), блокируем кнопки
            if (currentAmount <= 0)
            {
                sellOneButton.interactable = false;
                sellAllButton.interactable = false;
            }
            else
            {
                sellOneButton.interactable = true;
                sellAllButton.interactable = true;

                // Логика кнопок
                sellOneButton.onClick.AddListener(() => SellOre(1));
                sellAllButton.onClick.AddListener(() => SellOre(_currentAmount));
            }
        }

        private void SellOre(int amountToSell)
        {
            if (GameDataManager.Instance)
            {
                // Вызываем метод продажи в менеджере
                GameDataManager.Instance.SellOre(_data.oreId, amountToSell, _data.price);
            }
        }
    }
}