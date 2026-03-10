using System.Collections.Generic;
using UnityEngine;

namespace Ores.Refined
{
    [CreateAssetMenu(fileName = "RefinedDatabase", menuName = "Data/Refined Database")]
    public class RefinedDatabase : ScriptableObject
    {
        public List<RefinedData> allRefined;

        public RefinedData GetOreById(string id)
        {
            return allRefined.Find(x => x.oreId == id);
        }
    }
}