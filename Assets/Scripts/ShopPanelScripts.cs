using UnityEngine;
using UnityEngine.UI;
using static Player;
using static MiningScript;
using System.Collections.Generic;
using System;
using System.Linq;

public class ShopPanelScripts : MonoBehaviour
{
    private static ShopPanelScripts _instance;

    public static int[] GoodsPrices = new int[] { 100, 200, 125, 250 };
    public Text[] GoodsPriceText = new Text[4];
    public Text[] GoodsNameText = new Text[4];
    public Text MoneyText;
    public static Action<int[]> OnGoodsPricesChanged;

    private string[] furnaceUpgrades = { "Coal melting", "Coal auto-melting", "Copper melting", "Copper auto-melting", 
        "Iron melting", "Iron auto-melting", "Gold melting", "Gold auto-melting", "Redstone melting", "Redstone auto-melting",
        "Diamond melting", "Diamond auto-melting" };

    private void Awake()
    {
        _instance = this; // Запоминаем экземпляр
        OnMoneyChanged += onMoneyChanged;
        OnGoodsPricesChanged += onGoodsPricesChanged;
    }


    private void setGoodsLabels()
    {
        if (furnaceLevel < 12) GoodsNames[2] = furnaceUpgrades[furnaceLevel];
        else GoodsNames[2] = "No more upgrades";
        for (int i = 0; i < GoodsPrices.Length; i++)
            {
                GoodsPriceText[i].text = GoodsPrices[i].ToString();
                GoodsNameText[i].text = GoodsNames[i].ToString();
                checkMoneyForShopPrices();
            }
    }

    private void onGoodsPricesChanged(int[] prices)
    {
        GoodsPrices = prices;
        setGoodsLabels();
    }

    public void OnClickBuyPickaxe()
    {
        if (Money >= GoodsPrices[0])
        {
            OnMoneyChanged?.Invoke(Money - GoodsPrices[0]);
            GoodsPrices[0] *= 2;
            OnGoodsPricesChanged?.Invoke(GoodsPrices);
            
            pickaxePower++;
        }
    }

    public void OnClickBuyMoreOres()
    {
        if (Money >= GoodsPrices[1])
        {
            OnMoneyChanged?.Invoke(Money - GoodsPrices[1]);
            GoodsPrices[1] *= 2;
            OnGoodsPricesChanged?.Invoke(GoodsPrices);

            oreLevel += 2;
            OreChances = CalculateOreChances(oreLevel);
            OnOreChancesChanged?.Invoke();
        }
    }

    public void OnClickBuyFurnaceUpgrade()
    {
        if (Money >= GoodsPrices[2])
        {
            OnMoneyChanged?.Invoke(Money - GoodsPrices[2]);
            GoodsPrices[2] *= 2;
            furnaceLevel++;
            OnFurnaceLevelChanged?.Invoke();

            OnGoodsPricesChanged?.Invoke(GoodsPrices);

        }
    }

    public void OnClickBuyAutoPickaxeUpgrade()
    {
        if (Money >= GoodsPrices[3])
        {
            OnMoneyChanged?.Invoke(Money - GoodsPrices[3]);
            GoodsPrices[3] *= 2;
            OnGoodsPricesChanged?.Invoke(GoodsPrices);
            pickaxeAutoPower++;
        }
    }

    // Makes text green if u have enough or else red
    public static void checkMoneyForShopPrices()
    {
        for (int i = 0; i < _instance.GoodsPriceText.Length; i++)
        {
            if (Money >= GoodsPrices[i])
                _instance.GoodsPriceText[i].color = Color.green;
            else
                _instance.GoodsPriceText[i].color = Color.red;
            if (_instance.GoodsPriceText[i].text.Length > 4) _instance.GoodsPriceText[i].fontSize = 60;
            else _instance.GoodsPriceText[i].fontSize = 75;
        }
    }

    private void onMoneyChanged(int money)
    {
        MoneyText.text = money + " GOLD";
        checkMoneyForShopPrices();
    }

    private static int GetOreLevel(string ore)
    {
        Dictionary<string, int> OreLevels = new Dictionary<string, int>
        {
            { OreNames.COAL, 1 },
            { OreNames.COPPER, 5 },
            { OreNames.IRON, 10 },
            { OreNames.GOLD, 15 },
            { OreNames.REDSTONE, 20 },
            { OreNames.DIAMOND, 25 },
            //{ OreNames.LAPIS, 30 },
            //{ OreNames.EMERALD, 35 },
            //{ OreNames.QUARTZ, 40 },
        };

        return OreLevels.ContainsKey(ore) ? OreLevels[ore] : 1;
    }

    public static Dictionary<string, double> CalculateOreChances(int oreLevel)
    {
        Dictionary<string, double> newOreChances = new Dictionary<string, double>
        {
            { OreNames.COAL, 0 },
            { OreNames.COPPER, 0 },
            { OreNames.IRON, 0 },
            { OreNames.GOLD, 0 },
            { OreNames.REDSTONE, 0 },
            { OreNames.DIAMOND, 0 },
            //{ OreNames.LAPIS, 0 },
            //{ OreNames.EMERALD, 0 },
            //{ OreNames.QUARTZ, 0 },
        };

        double sum = 0;  // Для нормализации
        List<string> keys = OreChances.Keys.ToList();

        foreach (var ore in keys)
        {
            int oreLvl = GetOreLevel(ore);  // Уровень руды
            double chance;

            if (oreLevel < oreLvl)
            {
                const double speed = 0.5;
                chance = Math.Exp(-speed * (oreLvl - oreLevel)) + Math.Exp(-5.0);
            }
            else
            {
                chance = Math.Exp(-2.0 * (oreLevel - oreLvl));  // Резкий спад
            }

            newOreChances[ore] = chance;
            sum += chance;
        }

        // Нормализация (сумма = 100%)
        foreach (var ore in keys)
        {
            newOreChances[ore] = (newOreChances[ore] / sum) * 100.0;
        }

        return newOreChances;
    }

    public void OnCheatButtonClick()
    {
        OnMoneyChanged?.Invoke(Money + 1000);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!saveLoaded) OnGoodsPricesChanged?.Invoke(GoodsPrices);
        for (int i = 0; i < GoodsPrices.Length; i++)
        {
            GoodsPriceText[i].resizeTextForBestFit = true;
            GoodsPriceText[i].resizeTextMaxSize = 60;
            GoodsPriceText[i].resizeTextMinSize = 35;
            GoodsNameText[i].resizeTextForBestFit = true;
            GoodsNameText[i].resizeTextMaxSize = 60;
            GoodsNameText[i].resizeTextMinSize = 35;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
