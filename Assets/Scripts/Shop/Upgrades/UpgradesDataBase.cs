using System.Collections.Generic;
using UnityEngine;

namespace Shop.Upgrades
{
    [CreateAssetMenu(fileName = "UpgradesDataBase", menuName = "Data/Upgrades Database")]
    public class UpgradesDataBase : ScriptableObject
    {
        public List<UpgradeData> allUpgrades;

        public UpgradeData GetUpgradeInfoByName(UpgradeName upgradeName)
        {
            return allUpgrades.Find(x => x.name == upgradeName);
        }
    }
}