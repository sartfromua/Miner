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
        }
    }
}