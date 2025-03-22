using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player _instance;


    [SerializeField] public Dictionary<string, int> _oresInventory = new Dictionary<string, int>();
    [SerializeField] public int _money = 0;
    public static int pickaxePower = 1;

    private void Awake()
    {
        _instance = this;
    }

    public static int Money
    {
        get
        {
            return _instance._money;
        }
        set
        {
            _instance._money = Mathf.Max(0, value); // Запрещаем отрицательные деньги
        }
    }

    public static void SetOre(string oreName, int amount)
    {
        if (_instance == null)
        {
            Debug.LogError("Player не найден на сцене!");
            return;
        }

        if (_instance._oresInventory.ContainsKey(oreName))
            _instance._oresInventory[oreName] = amount;
        else
            _instance._oresInventory.Add(oreName, amount);
    }

    public static void AddOneOre(string oreName)
    {
        if (_instance == null)
        {
            Debug.LogError("Player не найден на сцене!");
            return;
        }

        if (_instance._oresInventory.ContainsKey(oreName))
            _instance._oresInventory[oreName] += 1;
        else
            _instance._oresInventory.Add(oreName, 1);
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
