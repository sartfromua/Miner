using Shop.Upgrades;
using UnityEngine;

namespace Ores
{
    public static class WeightCalculator
    {
        public static float GetDynamicWeight(string oreId)
        {
            var db = GameDataManager.Instance.oreDataBase;
            var settings = db.spawnCurve;
            var oreData = db.GetOreById(oreId);
    
            var baseWeight = oreData ? oreData.chance : 0f;
            var upgradeLevel = GameDataManager.Instance.GetUpgradeLevel(UpgradeName.MoreOres);

            // 1. Пік
            var currentPeak = settings.basePeak + (upgradeLevel * settings.peakShiftPerLevel);

            // 2. Індекс та дистанція
            var oreIndex = db.allOres.FindIndex((item) => item.oreId == oreId);
            var distance = oreIndex - currentPeak;

            // 3. Сігма
            var sigma = (distance <= 0) ? settings.leftSpread : settings.rightSpread;
            var safeSigma = (sigma <= 0f) ? 0.001f : sigma;

            // 4. Множник Гауса
            // Розбиваємо формулу на частини для дебагу
            var exponent = -(distance * distance) / (2f * safeSigma * safeSigma);
            var gaussianMultiplier = Mathf.Exp(exponent);

            var finalWeight = baseWeight * gaussianMultiplier;

            // 5. Повний звіт в один Log (щоб не засмічувати консоль різними повідомленнями)
            Debug.Log($"<b>[Weight Debug: {oreId}]</b>\n" +
                      $"Lvl: {upgradeLevel} | Peak: {currentPeak:F2} | Index: {oreIndex}\n" +
                      $"Dist: {distance:F2} | Sigma: {safeSigma:F2} | Exponent: {exponent:F4}\n" +
                      $"Multiplier: {gaussianMultiplier:F6} | Base: {baseWeight} | <b>Final: {finalWeight:F6}</b>");

            return finalWeight;
        }
    }
}