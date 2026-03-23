using System;
using System.Collections.Generic;

namespace EquipmentCraft
{
    [Serializable]
    public class EquipmentStat
    {
        public string statId;
        public string displayName;
        public float value;
    }

    /// <summary>
    /// Тип экипировки.
    /// </summary>
    public enum EquipmentType
    {
        Body,
        Head,
        Legs,
        Feet,
        Pickaxe
    }

    [Serializable]
    public class EquipmentItem
    {
        public string itemName;

        /// <summary>Уровень редкости от 1 (Common) до 5 (Legendary).</summary>
        public int rarity;

        /// <summary>Тип экипировки (броня или кирка).</summary>
        public EquipmentType type;

        /// <summary>До 4 статов, выбранных случайно при генерации.</summary>
        public List<EquipmentStat> stats = new List<EquipmentStat>();

        public string RarityName => rarity switch
        {
            1 => "Common",
            2 => "Uncommon",
            3 => "Rare",
            4 => "Epic",
            5 => "Legendary",
            _ => "Unknown"
        };

        public string TypeName => type switch
        {
            EquipmentType.Body => "Body Armor",
            EquipmentType.Head => "Helmet",
            EquipmentType.Legs => "Leg Armor",
            EquipmentType.Feet => "Boots",
            EquipmentType.Pickaxe => "Pickaxe",
            _ => "Unknown"
        };
    }
}