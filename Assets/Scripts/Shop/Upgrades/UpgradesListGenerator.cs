using Shop.SellingOres;
using UnityEngine;

namespace Shop.Upgrades
{
    public class UpgradesListGenerator : MonoBehaviour
    {
        public GameObject itemPrefab; // Префаб с SellingItemUI
        public Transform contentParent;

        private void OnEnable()
        {
            Generate();
            // Подписываемся на обновление данных, чтобы при продаже список обновлялся
            if (GameDataManager.Instance != null)
                GameDataManager.Instance.OnDataUpdated += Generate;
        }

        private void OnDisable()
        {
            if (GameDataManager.Instance != null)
                GameDataManager.Instance.OnDataUpdated -= Generate;
        }

        private void Generate()
        {
            // 1. Очистка старого списка
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            if (!GameDataManager.Instance) return;

            // 2. Получаем все upgrade из базы
            var upgrades = GameDataManager.Instance.upgradesDataBase.allUpgrades;

            // 3. Создаем кнопки
            foreach (var upgrade in upgrades)
            {
                var level = GameDataManager.Instance.GetUpgradeLevel(UpgradeName.GetNameFromString(upgrade.name));
                Debug.Log("Setting up upgrade button: " + upgrade.name);
                if (level >= upgrade.maxLevel) continue;

                // Создаем объект
                var newItem = Instantiate(itemPrefab, contentParent);
            
                // Настраиваем
                var controller = newItem.GetComponent<UpgradeUI>();
                if (controller)
                {
                    controller.Setup(upgrade, level);
                }
            }
        }
    }
}