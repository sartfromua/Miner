using System;

namespace EquipmentCraft
{
    [Serializable]
    public class EquipmentCraftState
    {
        public int amountPerOre = 10;
        public int storedAmount = 0;
        public long craftStartTimeUnix = 0;
        public int craftDurationSeconds = 5*60;

        public bool IsCraftRunning => craftStartTimeUnix > 0;

        /// <summary>
        /// Множитель редкости крафта. 1.0 = без бонуса.
        /// Значение > 1 повышает итоговую редкость предмета.
        /// </summary>
        public float craftRarityMultiplier = 1f;

        public string GetNextOreId()
        {
            var idx = (int)(storedAmount / amountPerOre);
            if (idx >= GameDataManager.Instance.oreDataBase.allOres.Count) return null;
            var oreId = GameDataManager.Instance.oreDataBase.allOres[idx].oreId;
            return oreId;
        }

        public void AddOreToCraft(int amount=1)
        {
            if (GetNextOreId() != null) storedAmount += amount;
        }
        
        public string RemoveOreFromCraft(int amount=1)
        {
            if (storedAmount >= amount) storedAmount -= amount;
            return GetNextOreId();
        }
    }
}