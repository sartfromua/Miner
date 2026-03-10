using System.Collections.Generic;
using Ores;
using Ores.Refined;
using UnityEngine;

namespace Furnace
{
    public class FurnaceListGenerator : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private GameObject furnaceSlotPrefab;      // префаб с FurnaceSlotController
        [SerializeField] private Transform contentParent;       // VerticalLayoutGroup или его Content

        [Header("Базы данных")]
        [SerializeField] private RefinedDatabase refinedDatabase;   // ScriptableObject с рецептами
        [SerializeField] private OreDatabase oreDatabase;           // для иконок входных руд (опционально)

        private readonly Dictionary<string, FurnaceSlotController> activeStations 
            = new Dictionary<string, FurnaceSlotController>();

        private void OnEnable()
        {
            GenerateStations();

            if (GameDataManager.Instance == null) return;
            GameDataManager.Instance.OnDataUpdated += RefreshAllStations;
            GameDataManager.Instance.OnDataLoaded += GenerateStations;
        }

        private void OnDisable()
        {
            if (GameDataManager.Instance == null) return;
            GameDataManager.Instance.OnDataUpdated -= RefreshAllStations;
            GameDataManager.Instance.OnDataLoaded -= GenerateStations;
        }

        private void GenerateStations()
        {
            // 1. Очистка старых объектов
            ClearContent();

            if (!refinedDatabase || refinedDatabase.allRefined == null)
            {
                Debug.LogWarning("RefinedDatabase не назначен или пуст");
                return;
            }

            activeStations.Clear();

            // 2. Создаём по одному префабу на каждый рецепт
            foreach (var refined in refinedDatabase.allRefined)
            {
                if (!refined) continue;

                var newStation = Instantiate(furnaceSlotPrefab, contentParent);

                // Получаем компонент и настраиваем
                var controller = newStation.GetComponent<FurnaceSlotController>();
                if (!controller)
                {
                    Debug.LogWarning($"На префабе {furnaceSlotPrefab.name} отсутствует CraftSlotController");
                    Destroy(newStation);
                    continue;
                }

                
                controller.Setup(refined, oreDatabase.allOres.Find(x => x.oreId == refined.oreId));

                activeStations[controller.slotId] = controller;

                // Можно сразу обновить визуальное состояние
                controller.RefreshUI();
            }

            Debug.Log($"Сгенерировано {activeStations.Count} станков плавки");
        }

        private void RefreshAllStations()
        {
            foreach (var station in activeStations.Values)
            {
                station.RefreshUI();
            }
        }

        private void ClearContent()
        {
            for (var i = contentParent.childCount - 1; i >= 0; i--)
            {
                Destroy(contentParent.GetChild(i).gameObject);
            }
        }

        // Опционально — если хочешь брать иконку входной руды из OreDatabase
        private Sprite GetInputIcon(string oreId)
        {
            if (oreDatabase == null) return null;
            var ore = oreDatabase.GetOreById(oreId);
            return ore?.icon;   // предполагаем, что в OreData есть поле public Sprite icon;
        }
    }
}