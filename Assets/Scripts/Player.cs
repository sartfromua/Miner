using System;
using System.Collections.Generic;
using UnityEngine;
using static MiningScript;

public class Player : MonoBehaviour
{
    private static Player _instance;

    public static Action<int> OnMoneyChanged;
    public static Action<string, int> OnInventoryChanged;
    public static Action<string> OnOneOreMined;


    private Dictionary<string, int> _oresInventory = new Dictionary<string, int>();
    [SerializeField] public int _money = 0;
    
    public static int pickaxePower = 1;
    public static int Money { get { return _instance._money; } }

    private void Awake()
    {
        _instance = this;
        OnMoneyChanged += changeMoney;
        OnInventoryChanged += changeInventory;
        OnOneOreMined += oneOreMined;
    }

    private void oneOreMined(string oreName)
    {
        OnInventoryChanged?.Invoke(oreName, _oresInventory[oreName]+1);
    }

    private void changeMoney(int money)
    {
        _money = money;
    }

    private void changeInventory(string oreName, int amount)
    {
        if (_oresInventory.ContainsKey(oreName))
            _oresInventory[oreName] = amount;
        else
            _oresInventory.Add(oreName, amount);
    }

    public static int GetOre(string oreName)
    {
        if (_instance._oresInventory.TryGetValue(oreName, out int amount))
        {
            return amount;
        }
        return 0; // Если руды нет, возвращаем 0
    }
}
