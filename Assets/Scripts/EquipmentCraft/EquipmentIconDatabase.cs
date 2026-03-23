using System;
using UnityEngine;

namespace EquipmentCraft
{
    /// <summary>
    /// ScriptableObject для хранения иконок типов экипировки.
    /// Создать в меню: Data / Equipment / Icon Database
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentIconDatabase", menuName = "Data/Equipment/Icon Database")]
    public class EquipmentIconDatabase : ScriptableObject
    {
        [Serializable]
        public class TypeIcon
        {
            public EquipmentType type;
            public Sprite icon;
        }

        [Header("Icons for each equipment type")]
        public TypeIcon[] typeIcons = new TypeIcon[]
        {
            new TypeIcon { type = EquipmentType.Body, icon = null },
            new TypeIcon { type = EquipmentType.Head, icon = null },
            new TypeIcon { type = EquipmentType.Legs, icon = null },
            new TypeIcon { type = EquipmentType.Feet, icon = null },
            new TypeIcon { type = EquipmentType.Pickaxe, icon = null },
        };

        /// <summary>
        /// Возвращает иконку для указанного типа экипировки.
        /// </summary>
        public Sprite GetIcon(EquipmentType type)
        {
            foreach (var typeIcon in typeIcons)
            {
                if (typeIcon.type == type)
                    return typeIcon.icon;
            }

            return null;
        }
    }
}
