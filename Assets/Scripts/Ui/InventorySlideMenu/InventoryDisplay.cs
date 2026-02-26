using UnityEngine;
using System.Collections.Generic;

public class InventoryDisplay : MonoBehaviour
{
    [Header("References")]
    public GameObject slotPrefab;
    public Transform container;

    // --- ПОДПИСКА НА СОБЫТИЯ ---

    private void OnEnable()
    {
        // 1. Сразу отображаем актуальные данные при открытии
        UpdateDisplay();

        // 2. Подписываемся: "Если данные изменятся, вызови UpdateDisplay снова"
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnDataUpdated += UpdateDisplay;
        }
    }

    private void OnDisable()
    {
        // 3. ОБЯЗАТЕЛЬНО отписываемся, иначе будет ошибка, 
        // если данные обновятся, а окно инвентаря выключено или удалено.
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnDataUpdated -= UpdateDisplay;
        }
    }

    // --- ВАШ МЕТОД ОБНОВЛЕНИЯ ---

    public void UpdateDisplay()
    {
        // Удаляем старые слоты
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        if (!GameDataManager.Instance || !GameDataManager.Instance.oreDataBase)
            return;

        // Берем список всех руд (порядок из ScriptableObject)
        var allOres = GameDataManager.Instance.oreDataBase.allOres;
        
        // Берем инвентарь игрока
        var inventory = GameDataManager.Instance.playerData.OresInventory;

        foreach (var ore in allOres)
        {
            var amount = 0;

            // Если руда есть в сохранении - берем число, иначе 0
            if (inventory.TryGetValue(ore.oreId, out var count))
            {
                amount = count;
            }

            // Создаем слот
            var newSlot = Instantiate(slotPrefab, container);
            
            var slotScript = newSlot.GetComponent<InventorySlotUI>();
            if (slotScript)
            {
                slotScript.Setup(ore.icon, amount);
            }
        }
    }
}