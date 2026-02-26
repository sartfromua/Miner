using UnityEngine;

namespace Shop.SellingOres
{
    public class SellingListGenerator : MonoBehaviour
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
            // TODO: fix
            // 1. Очистка старого списка
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            if (!GameDataManager.Instance) return;

            // 2. Получаем все виды руд из базы
            var allOres = GameDataManager.Instance.oreDataBase.allOres;
            var inventory = GameDataManager.Instance.playerData.OresInventory;

            // 3. Создаем кнопки
            foreach (var ore in allOres)
            {
                // Узнаем, сколько этой руды у игрока (если нет в словаре, будет 0)
                var amount = 0;
                if (inventory.TryGetValue(ore.oreId, out var value))
                {
                    amount = value;
                }

                // Создаем объект
                var newItem = Instantiate(itemPrefab, contentParent);
            
                // Настраиваем
                var controller = newItem.GetComponent<SellingItemUI>();
                if (controller)
                {
                    controller.Setup(ore, amount);
                }
            }
        }
    }
}