using System;
using UnityEngine;
using UnityEngine.UI;


namespace Ui
{
    
    public class CheatAddMoney: MonoBehaviour
    {
        [Header("Button")]
        public Button button;

        public void Awake()
        {
            button.onClick.AddListener(Cheat);
        }

        private static void Cheat()
        {
            GameDataManager.Instance.AddMoney(100);
            GameDataManager.Instance.AddRefinedOre("iron_ore", 10);
            GameDataManager.Instance.AddRefinedOre("coal_ore", 10);
            GameDataManager.Instance.AddRefinedOre("copper_ore", 10);
            GameDataManager.Instance.AddRefinedOre("emerald_ore", 10);
            GameDataManager.Instance.AddRefinedOre("gold_ore", 10);
            GameDataManager.Instance.AddRefinedOre("orichalcum_ore", 10);
            GameDataManager.Instance.AddRefinedOre("adamantite_ore", 10);
            GameDataManager.Instance.AddRefinedOre("silver_ore", 10);
        }
    }
}