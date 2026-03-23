using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.InventorySlideMenu
{
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image iconImage;
        public TextMeshProUGUI amountText;

        // Этот метод мы вызовем из менеджера
        public void Setup(Sprite icon, int amount)
        {
            iconImage.sprite = icon;
            amountText.text = amount.ToString();
        
            // Опционально: если иконок нет, ставим заглушку
            iconImage.enabled = icon;
        }
    }
}