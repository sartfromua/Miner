using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static MiningScript;
using static ShopPanelScripts;

public class Player : MonoBehaviour
{
    private static Player _instance;

    public static string REFINED = "refined";
    public static string ORE = "ore";

    public static Action<int> OnMoneyChanged;
    public static Action<string, int, string> OnInventoryChanged;
    public static Action<string> OnOneOreMined;
    public static Action OnOreChancesChanged;
    public static Action OnFurnaceLevelChanged;
    public static string[] GoodsNames = { "Pickaxe", "More ores", "Coal melting", "Auto mining" };

    public static bool saveLoaded = false;
    
    private Dictionary<string, int> _oresInventory = new Dictionary<string, int>();
    private Dictionary<string, int> _refinedInventory = new Dictionary<string, int>();
    

    [SerializeField] private int _money = 0;
    public static DateTime lastSaveDate;

    public static int pickaxePower = 1;
    public static int pickaxeAutoPower = 0;
    public static int furnaceLevel = 0;
    public static int oreLevel = 0;
    public static int Money { get { return _instance._money; } }

    public static Dictionary<string, double> OreChances = new Dictionary<string, double>
    {
        { OreNames.COAL, 9 },
        { OreNames.COPPER, 1 },
        { OreNames.IRON, 0 },
        { OreNames.GOLD, 0 },
        { OreNames.REDSTONE, 0 },
        { OreNames.DIAMOND, 0 },
        //{ OreNames.LAPIS, 0 },
        //{ OreNames.EMERALD, 0 },
        //{ OreNames.QUARTZ, 0 },
    };

    public static Save makeSave()
    {
        Save sv = new Save();
        sv.money = Money;
        sv.oreLevel = oreLevel;
        sv.furnaceLevel = furnaceLevel;
        sv.pickaxePower = pickaxePower;
        sv.pickaxeAutoPower = pickaxeAutoPower;
        sv.oresInventory = new Dictionary<string, int>(_instance._oresInventory);
        sv.refinedInventory = new Dictionary<string, int>(_instance._refinedInventory);
        sv.goodsPrices = (int[]) GoodsPrices.Clone();
        sv.lastSaveDate[0] = DateTime.Now.Year;
        sv.lastSaveDate[1] = DateTime.Now.Month;
        sv.lastSaveDate[2] = DateTime.Now.Day;
        sv.lastSaveDate[3] = DateTime.Now.Hour;
        sv.lastSaveDate[4] = DateTime.Now.Minute;
        sv.lastSaveDate[5] = DateTime.Now.Second;
        return sv;
    }

    public static void LoadSave(Save sv)
    {   
        Debug.Log("Loading save");

        if (_instance == null)
        {
            Debug.LogError("Player _instance is null!");
            return;
        }

        if (sv.oresInventory == null)
        {
            Debug.LogError("sv.oresInventory is null!");
            return;
        }

        OnMoneyChanged?.Invoke(sv.money);
        oreLevel = sv.oreLevel;
        OreChances = CalculateOreChances(sv.oreLevel);
        furnaceLevel = sv.furnaceLevel;
        OnFurnaceLevelChanged?.Invoke();
        pickaxePower = sv.pickaxePower;
        pickaxeAutoPower = sv.pickaxeAutoPower;
        //Debug.Log("Ores inventory:");
        foreach (var ore in sv.oresInventory)
        {
            //Debug.Log(ore.Key + ": " + ore.Value);
            OnInventoryChanged?.Invoke(ore.Key, ore.Value, ORE);
        }

        foreach (var ore in sv.refinedInventory)
        {
            OnInventoryChanged?.Invoke(ore.Key, ore.Value, REFINED);
        }
        OnGoodsPricesChanged?.Invoke(sv.goodsPrices);
        saveLoaded = true;
        lastSaveDate = new DateTime(sv.lastSaveDate[0], sv.lastSaveDate[1], sv.lastSaveDate[2], sv.lastSaveDate[3], sv.lastSaveDate[4], sv.lastSaveDate[5]);
        if (DateTime.Now.Second - lastSaveDate.Second > 100000000) lastSaveDate = DateTime.Now;
    }

    private void Awake()
    {
        _instance = this;
        OnMoneyChanged += changeMoney;
        OnInventoryChanged += changeInventory;
        OnOneOreMined += oneOreMined;
    }

    private void Start()
    {
    }

    private void oneOreMined(string oreName)
    {
        if (_oresInventory.ContainsKey(oreName))
            _oresInventory[oreName]++;
        else
            _oresInventory.Add(oreName, 1);
        OnInventoryChanged?.Invoke(oreName, _oresInventory[oreName], ORE);
    }

    private void changeMoney(int money)
    {
        _money = money;
    }

    private void changeInventory(string oreName, int amount, string type)
    {   
        if (type == ORE)
        {
            if (_oresInventory.ContainsKey(oreName))
                _oresInventory[oreName] = amount;
            else
                _oresInventory.Add(oreName, amount);
        }
        else if (type == REFINED)
        {
            if (_refinedInventory.ContainsKey(oreName))
                _refinedInventory[oreName] = amount;
            else
                _refinedInventory.Add(oreName, amount);
        }
    }

    public static int GetOre(string oreName)
    {
        if (_instance._oresInventory.TryGetValue(oreName, out int amount))
        {
            return amount;
        }
        return 0; // Если руды нет, возвращаем 0
    }

    public static int GetRefined(string oreName)
    {
        if (_instance._refinedInventory.TryGetValue(oreName, out int amount))
        {
            return amount;
        }
        return 0; // Если руды нет, возвращаем 0
    }
}
