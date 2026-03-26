namespace Shop.Upgrades
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class UpgradeName
    {
        private UpgradeName(string value) { Value = value; }
        public string Value { get; private set; }

        public static UpgradeName MoreOres { get; } = new UpgradeName("MoreOres");
        public static UpgradeName Pickaxe { get; } = new UpgradeName("Pickaxe");
        public static UpgradeName DoubleHarvest { get; } = new UpgradeName("DoubleHarvest");
        public static UpgradeName SellingOresMult { get; } = new UpgradeName("SellingOresMultiplier");
        public static UpgradeName AutoPickaxe { get; } = new UpgradeName("AutoPickaxe");
        public static UpgradeName AutoSmelt { get; } = new UpgradeName("AutoSmelt");

        // Словник для швидкого пошуку за рядком
        private static readonly Dictionary<string, UpgradeName> AllNames;

        static UpgradeName()
        {
            // Автоматично знаходимо всі статичні властивості типу UpgradeName у цьому класі
            AllNames = typeof(UpgradeName)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(UpgradeName))
                .Select(p => (UpgradeName)p.GetValue(null))
                .ToDictionary(x => x.Value, x => x);
        }

        public static UpgradeName GetNameFromString(string value)
        {
            return string.IsNullOrEmpty(value) 
                ? null 
                : AllNames.GetValueOrDefault(value, MoreOres);
        }

        // Дозволяє використовувати об'єкт як рядок автоматично
        public override string ToString() => Value;
        public static implicit operator string(UpgradeName name) => name?.Value;
    }}