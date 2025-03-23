using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;
using static MiningScript;
using static Player;


// TODO: Добавить количество добытой руды на экран

public class BlackSmithPanelScripts : MonoBehaviour
{

    public Text MoneyText;

    public Text OreCountCoal;
    public Text OreCountCopper;
    public Text OreCountIron;
    public Text OreCountGold;
    public Text OreCountRedstone;
    public Text OreCountDiamond;

    public Dictionary<string, Text> OreCounters = new Dictionary<string, Text>        {
            { OreNames.COAL, null },
            { OreNames.COPPER, null },
            { OreNames.IRON, null },
            { OreNames.GOLD, null },
            { OreNames.DIAMOND, null },
            { OreNames.LAPIS, null },
            { OreNames.REDSTONE, null },
            { OreNames.EMERALD, null },
            { OreNames.QUARTZ, null },
        };

        

    private Dictionary<string, int> OrePrices = new Dictionary<string, int>
        {
            { OreNames.COAL, 1 },
            { OreNames.COPPER, 2 },
            { OreNames.IRON, 4 },
            { OreNames.GOLD, 10 },
            { OreNames.REDSTONE, 20 },
            { OreNames.DIAMOND, 50 },
            { OreNames.LAPIS, 6 },
            { OreNames.EMERALD, 8 },
            { OreNames.QUARTZ, 9 },
        };

    private void sellOre(string oreName)
    {   
        if (GetOre(oreName) > 0)
        {
            OnMoneyChanged?.Invoke(Money + GetOre(oreName) * OrePrices[oreName]);
            OnInventoryChanged?.Invoke(oreName, 0);
        }
    }

    public void OnClickSellCoal() => sellOre(OreNames.COAL);

    public void OnClickSellIron() => sellOre(OreNames.IRON);

    public void OnClickSellGold() => sellOre(OreNames.GOLD);

    public void OnClickSellCopper() => sellOre(OreNames.COPPER);

    public void OnClickSellDiamond() => sellOre(OreNames.DIAMOND);

    public void OnClickSellEmerald() => sellOre(OreNames.EMERALD);

    public void OnClickSellRedstone() => sellOre(OreNames.REDSTONE);

    public void OnClickSellLapis() => sellOre(OreNames.LAPIS);

    public void OnClickSellQuartz() => sellOre(OreNames.QUARTZ);

    public void OnClickSellAll()
    {
        foreach (string oreName in OreNames.names) sellOre(oreName);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnMoneyChanged += onMoneyChanged;
        OnInventoryChanged += onInventoryChanged;
        OreCounters[OreNames.COAL] = OreCountCoal;
        OreCounters[OreNames.COPPER] = OreCountCopper;
        OreCounters[OreNames.IRON] = OreCountIron;
        OreCounters[OreNames.GOLD] = OreCountGold;
        OreCounters[OreNames.REDSTONE] = OreCountRedstone;
        OreCounters[OreNames.DIAMOND] = OreCountDiamond;
    }
    private void onMoneyChanged(int money)
    {
        MoneyText.text = money + "GOLD";
    }

    private void onInventoryChanged(string oreName, int amount)
    {
        OreCounters[oreName].text = amount.ToString();
    }

    // Update is called once per frame
    void Update()
    {
      
    }
}
