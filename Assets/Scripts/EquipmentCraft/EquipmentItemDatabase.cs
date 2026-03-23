using System.Collections.Generic;
using UnityEngine;

namespace EquipmentCraft
{
    /// <summary>
    /// ScriptableObject — база всех доступных характеристик экипировки.
    /// Создать в меню: Data / Equipment / Item Database
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentItemDatabase", menuName = "Data/Equipment/Item Database")]
    public class EquipmentItemDatabase : ScriptableObject
    {
        [Tooltip("Все доступные характеристики, из которых будут случайно выбраны статы")]
        public List<EquipmentStatDefinition> allStats = new List<EquipmentStatDefinition>();
    }
}
