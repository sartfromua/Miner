using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop.Upgrades
{
    public class UpgradeUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI priceText;
    
        [Header("Buttons")]
        public Button buyButton;

        private UpgradeData _data;
        private UpgradeName _dataName;
        
        public void Awake()
        {
            GameDataManager.Instance.OnDataUpdated += UpdateVisual;
        }

        private void UpdateVisual()
        {
            nameText.text = _data.name;
            var price = GameDataManager.Instance.GetUpgradePrice(_dataName); 
            priceText.text = price.ToString();
            priceText.color = GameDataManager.Instance.playerData.money >= price ? Color.green : Color.red;
        }

        public void Setup(UpgradeData data, int currentLevel)
        {
            _data = data;
            _dataName = UpgradeName.GetNameFromString(data.name);
            if (_dataName == null) throw new Exception("Wrong upgrade name");

            // 1. Заполняем визуал
            UpdateVisual();

            // 2. Настраиваем кнопки
            // Сначала удаляем старые подписки, чтобы не дублировались
            buyButton.onClick.RemoveAllListeners();

            // Если руды нет (0), блокируем кнопки
            if (currentLevel >= data.maxLevel)
            {
                buyButton.interactable = false;
            }
            else
            {
                buyButton.interactable = true;

                // Логика кнопок
                buyButton.onClick.AddListener(BuyUpgrade);
            }
        }

        private void BuyUpgrade()
        {
            if (GameDataManager.Instance)
            {
                // Вызываем метод продажи в менеджере
                GameDataManager.Instance.BuyUpgrade(UpgradeName.GetNameFromString(_dataName));
            }
        }
    }
}