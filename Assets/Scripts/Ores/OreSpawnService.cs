using System;
using System.Collections;
using System.Collections.Generic; // Нужно для корутин
using UnityEngine;
using System.Linq;
using Ores;
using Shop.Upgrades;
using Random = UnityEngine.Random;

public class OreSpawnService : MonoBehaviour
{
    public OreObject sceneOreObject;
    private Dictionary<string, float> _oreChances;
    [SerializeField] private List<string> _oreChancesDebug;
    public System.Action OnOresReset;
    
    
    private void Awake()
    {
        GameDataManager.Instance.OnDataLoaded += ResetOreChances;
        GameDataManager.Instance.OnUpgradesUpdated += ResetOreChances;
        // Запускаем первую руду при логине игры
        OnOresReset += SpawnRandomOre;
        OnOresReset += TestRandomOre;
    }

    private void SpawnRandomOre()
    {
        var selectedOre = GetWeightedRandomOre();
        
        if (!selectedOre) 
        {
            Debug.LogError("Не удалось выбрать руду!");
            return;
        }
        
        // Передаем в Setup сам объект руды И метод, который нужно вызвать после смерти.
        // Здесь мы используем лямбду () => ..., чтобы добавить задержку перед спавном новой
        sceneOreObject.Setup(selectedOre, () => 
        {
            StartCoroutine(RespawnRoutine());
        });
    }

    // Небольшая задержка перед появлением новой руды (для красоты)
    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(0.2f); 
        SpawnRandomOre();
    }

    private void ResetOreChances()
    {
        _oreChances = new Dictionary<string, float>();
        foreach (var ore in GameDataManager.Instance.oreDataBase.allOres)
        {
            var chance = WeightCalculator.GetDynamicWeight(ore.oreId);
            Debug.Log($"ResetOreChances: {ore.oreId} - {chance} ");
            _oreChances[ore.oreId] = chance;
        }
        _oreChancesDebug = _oreChances.Select(kvp => $"{kvp.Key}: {kvp.Value:F2}").ToList();
        OnOresReset?.Invoke();
    }

    private OreData GetWeightedRandomOre()
    {
        if (_oreChances == null) return GameDataManager.Instance.oreDataBase.allOres.FirstOrDefault();

        var totalWeight = _oreChances.Values.Sum();
        var randomValue = Random.Range(0f, totalWeight);

        foreach (var ore in GameDataManager.Instance.oreDataBase.allOres)
        {
            if (!_oreChances.TryGetValue(ore.oreId, out var weight)) continue;
            // Якщо випадкове число потрапило в "прошарок" цієї руди
            if (randomValue < weight)
                return ore;

            // Зменшуємо випадкове число на вагу поточної руди, 
            // переходячи до наступного сегмента
            randomValue -= weight;
        }

        return GameDataManager.Instance.oreDataBase.allOres.FirstOrDefault();
    }

    private void TestRandomOre()
    {
        if (!GameDataManager.Instance || !GameDataManager.Instance.oreDataBase) return;

        var stats = new Dictionary<string, int>();
        var db = GameDataManager.Instance.oreDataBase;

        // Ініціалізація словника
        foreach (var ore in db.allOres)
        {
            stats[ore.oreId] = 0;
        }

        // 2. Генерація спроб
        const int testCount = 1000; // Збільшив до 1000 для кращої статистики
        for (var i = 0; i < testCount; i++)
        {
            var droppedOre = GetWeightedRandomOre();
            if (droppedOre) stats[droppedOre.oreId]++;
        }

        // 3. Формування звіту з параметрами
        var curve = db.spawnCurve; // Наші налаштування графіка
        var level = GameDataManager.Instance.GetUpgradeLevel(UpgradeName.MoreOres);

        
        // Рахуємо фактичний пік для звіту
        var currentPeak = curve.basePeak + (level * curve.peakShiftPerLevel);

        var report = $"<b>[ORE TEST REPORT]</b>\n";
        report += $"<color=cyan>Параметри:</color> Рівень MoreOres: <b>{level}</b> | Пік (Tier): <b>{currentPeak:F2}</b>\n";
        report += $"<color=cyan>Крива:</color> Left: {curve.leftSpread} | Right: {curve.rightSpread} | Shift: {curve.peakShiftPerLevel}\n";
        report += "---------------------------------------\n";

        foreach (var entry in stats)
        {
            // Отримуємо розраховану вагу зі словника (якщо її там немає — 0)
            _oreChances.TryGetValue(entry.Key, out var weight);
    
            var percentage = (entry.Value / (float)testCount) * 100f;

            // Виводимо: ID | Вага | Кількість | Відсоток
            report += $"- <b>{entry.Key}</b>: Вага: <i>{weight:F2}</i> | Випало: {entry.Value} шт. ({percentage:F1}%)\n";
        }

        Debug.Log(report);
    }    
}