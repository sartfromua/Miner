using UnityEngine;

namespace EquipmentCraft
{
    /// <summary>
    /// ScriptableObject, описывающий одну характеристику предмета экипировки.
    /// Создать в меню: Data / Equipment / Stat Definition
    /// </summary>
    [CreateAssetMenu(fileName = "New Stat Definition", menuName = "Data/Equipment/Stat Definition")]
    public class EquipmentStatDefinition : ScriptableObject
    {
        [Tooltip("Уникальный ID характеристики, например: attack, defense, speed")]
        public string statId;

        [Tooltip("Отображаемое имя характеристики в UI")]
        public string displayName;

        [Tooltip("Базовое значение (умножается на множитель редкости при генерации)")]
        public float baseValue = 10f;
    }
}
