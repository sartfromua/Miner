using UnityEngine;

namespace Shop.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrade", menuName = "Data/Upgrades")]
    public class UpgradeData : ScriptableObject
    {
        public new string name; // iron_ore
        public int price;
        public int maxLevel;
    }
}