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

    [SerializeField] int[] GoodsPrices;
    [SerializeField] Text[] GoodsPriceText;
    private int oreLevel = 0;
    public Text MoneyText;

    private void Awake()
    {
        _instance = this; // Запоминаем экземпляр
    }

    public void OnClickBuyPickaxe()
    {
        if (Money >= GoodsPrices[0])
        {
            OnMoneyChanged?.Invoke(Money - GoodsPrices[0]);
            GoodsPrices[0] *= 2;
            pickaxePower++;

        }
        for (int i = 0; i < GoodsPriceText.Length; i++)
        {
            GoodsPriceText[i].text = GoodsPrices[i].ToString();
            checkMoneyForShopPrices();
        }
    }

    public void OnClickBuyMoreOres()
    {
        if (Money >= GoodsPrices[1])
        {
            OnMoneyChanged?.Invoke(Money - GoodsPrices[1]);
            GoodsPrices[1] *= 2;

            oreLevel += 2;
            OreChances = CalculateOreChances(oreLevel);
            Debug.Log("Ore level: " + oreLevel);
            foreach (var pair in OreChances)
            {
                Debug.Log($"{pair.Key}: {pair.Value}");
            }
            Debug.Log("Ore chances: " + OreChances);
        }
        for (int i = 0; i < GoodsPriceText.Length; i++)
        {
            GoodsPriceText[i].text = GoodsPrices[i].ToString();
            if (GoodsPriceText[i].text.Length > 5) GoodsPriceText[i].fontSize = 60;
            else GoodsPriceText[i].fontSize = 60;
            checkMoneyForShopPrices();
        }

    }

    // Makes text green if u have enough or else red
    public static void checkMoneyForShopPrices()
    {
        for (int i = 0; i < _instance.GoodsPriceText.Length; i++)
        {
            if (Money >= _instance.GoodsPrices[i])
                _instance.GoodsPriceText[i].color = Color.green;
            else
                _instance.GoodsPriceText[i].color = Color.red;
        }
    }

    public static void SetGoodsPrice(int index, int newPrice)
    {
        if (_instance == null)
        {
            Debug.LogError("ShopPanelScripts не найден на сцене!");
            return;
        }

        if (index >= 0 && index < _instance.GoodsPrices.Length)
        {
            _instance.GoodsPrices[index] = newPrice;
            _instance.GoodsPriceText[index].text = newPrice.ToString();
        }
    }

    private void onMoneyChanged(int money)
    {
        MoneyText.text = money + "GOLD";
    }

    private static int GetOreLevel(string ore)
    {
        Dictionary<string, int> OreLevels = new Dictionary<string, int>
        {
            { OreNames.COAL, 1 },
            { OreNames.COPPER, 5 },
            { OreNames.IRON, 10 },
            { OreNames.GOLD, 15 },
            { OreNames.DIAMOND, 20 },
            { OreNames.LAPIS, 25 },
            { OreNames.REDSTONE, 30 },
            { OreNames.EMERALD, 35 },
            { OreNames.QUARTZ, 40 },
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
            { OreNames.DIAMOND, 0 },
            { OreNames.LAPIS, 0 },
            { OreNames.REDSTONE, 0 },
            { OreNames.EMERALD, 0 },
            { OreNames.QUARTZ, 0 },
        };

        double sigma = OreChances.Count / 3.0;  // Стандартное отклонение (чем больше OreChances, тем шире разброс)
        double sum = 0;  // Для нормализации

        // Рассчитываем сырые вероятности
        List<string> keys = OreChances.Keys.ToList();
        foreach (var ore in keys)
        {
            int oreLvl = GetOreLevel(ore);  // Получаем уровень руды
            double chance = Math.Exp(-Math.Pow(oreLvl - oreLevel, 2) / (2 * sigma * sigma));
            newOreChances[ore] = chance;
            sum += chance;
        }

        // Нормализация (чтобы сумма шансов = 100%)
        foreach (var ore in keys)
        {
            newOreChances[ore] = (newOreChances[ore] / sum) * 100;
        }

        return newOreChances;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnMoneyChanged += onMoneyChanged;
        for (int i = 0; i < GoodsPrices.Length; i++)
        {
            GoodsPriceText[i].text = GoodsPrices[i].ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        MoneyText.text = Money + "GOLD";
    }
}
