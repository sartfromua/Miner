using System.Collections.Generic;
using UnityEngine;

namespace Ores
{
    [System.Serializable] 
    public struct SpawnCurveSettings
    {
        public float leftSpread;
        public float rightSpread;
        public float peakShiftPerLevel;
        public float basePeak;
    }


    [CreateAssetMenu(fileName = "OreDatabase", menuName = "Data/Ore Database")]
    public class OreDatabase : ScriptableObject
    {
        public List<OreData> allOres;
    
        [Header("Налаштування ймовірностей (Крива)")]
        public SpawnCurveSettings spawnCurve;

        public OreData GetOreById(string id)
        {
            return allOres.Find(x => x.oreId == id);
        }
    }
}