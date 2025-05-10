using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MiningScript;
using static Player;


public class BlackSmithPanelScripts : MonoBehaviour
{

    public Text MoneyText;

    public TMP_Text OreCountCoal;
    public TMP_Text OreCountCopper;
    public TMP_Text OreCountIron;
    public TMP_Text OreCountGold;
    public TMP_Text OreCountRedstone;
    public TMP_Text OreCountDiamond;

    public TMP_Text RefinedCountCoal;
    public TMP_Text RefinedCountCopper;
    public TMP_Text RefinedCountIron;
    public TMP_Text RefinedCountGold;
    public TMP_Text RefinedCountRedstone;
    public TMP_Text RefinedCountDiamond;

    public Dictionary<string, TMP_Text> OreCounters = new Dictionary<string, TMP_Text>        {
            { OreNames.COAL, null },
            { OreNames.COPPER, null },
            { OreNames.IRON, null },
            { OreNames.GOLD, null },
            { OreNames.REDSTONE, null },
            { OreNames.DIAMOND, null },
            //{ OreNames.LAPIS, null },
            //{ OreNames.EMERALD, null },
            //{ OreNames.QUARTZ, null },
        };

    public Dictionary<string, TMP_Text> RefinedCounters = new Dictionary<string, TMP_Text>        {
            { OreNames.COAL, null },
            { OreNames.COPPER, null },
            { OreNames.IRON, null },
            { OreNames.GOLD, null },
            { OreNames.REDSTONE, null },
            { OreNames.DIAMOND, null },
            //{ OreNames.LAPIS, null },
            //{ OreNames.EMERALD, null },
            //{ OreNames.QUARTZ, null },
        };



    private Dictionary<string, int> OrePrices = new Dictionary<string, int>
        {
            { OreNames.COAL, 1 },
            { OreNames.COPPER, 2 },
            { OreNames.IRON, 4 },
            { OreNames.GOLD, 10 },
            { OreNames.REDSTONE, 20 },
            { OreNames.DIAMOND, 50 },
            //{ OreNames.LAPIS, 6 },
            //{ OreNames.EMERALD, 8 },
            //{ OreNames.QUARTZ, 9 },
        };

    private Dictionary<string, int> RefinedPrices = new Dictionary<string, int>
        {
            { OreNames.COAL, 5 },
            { OreNames.COPPER, 10 },
            { OreNames.IRON, 20 },
            { OreNames.GOLD, 50 },
            { OreNames.REDSTONE, 100 },
            { OreNames.DIAMOND, 250 },
            //{ OreNames.LAPIS, 6 },
            //{ OreNames.EMERALD, 8 },
            //{ OreNames.QUARTZ, 9 },
        };

    private void sellOre(string oreName)
    {   
        if (GetOre(oreName) > 0)
        {
            OnMoneyChanged?.Invoke(Money + GetOre(oreName) * OrePrices[oreName]);
            OnInventoryChanged?.Invoke(oreName, 0, ORE);
        }
    }

    private void sellRefined(string oreName)
    {
        if (GetRefined(oreName) > 0)
        {
            OnMoneyChanged?.Invoke(Money + GetRefined(oreName) * RefinedPrices[oreName]);
            OnInventoryChanged?.Invoke(oreName, 0, REFINED);
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


    public void OnClickSellCoalRefined() => sellRefined(OreNames.COAL);

    public void OnClickSellCopperRefined() => sellRefined(OreNames.COPPER);

    public void OnClickSellIronRefined() => sellRefined(OreNames.IRON);

    public void OnClickSellGoldRefined() => sellRefined(OreNames.GOLD);

    public void OnClickSellRedstoneRefined() => sellRefined(OreNames.REDSTONE);

    public void OnClickSellDiamondRefined() => sellRefined(OreNames.DIAMOND);

    public void OnClickSellAllOres()
    {
        foreach (string oreName in OreNames.names) sellOre(oreName);
    }

    public void OnClickSellAllRefined()
    {
        foreach (string oreName in OreNames.names) sellRefined(oreName);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    private void onMoneyChanged(int money)
    {
        MoneyText.text = money + "GOLD";
    }

    private void onInventoryChanged(string oreName, int amount, string type)
    {
        if (oreName == OreNames.LAPIS || oreName == OreNames.QUARTZ || oreName == OreNames.EMERALD) return;
        Debug.Log("Inventory changed: " + oreName + " " + type+ " " + amount);
        if (type == ORE)
            OreCounters[oreName].text = amount.ToString();
        else if (type == REFINED)
            RefinedCounters[oreName].text = amount.ToString();
        else
            Debug.LogError("Unknown type: " + type);
    }

    // Update is called once per frame
    void Update()
    {
      
    }
        
    private void Awake()
    {
        OreCounters[OreNames.COAL] = OreCountCoal;
        OreCounters[OreNames.COPPER] = OreCountCopper;
        OreCounters[OreNames.IRON] = OreCountIron;
        OreCounters[OreNames.GOLD] = OreCountGold;
        OreCounters[OreNames.REDSTONE] = OreCountRedstone;
        OreCounters[OreNames.DIAMOND] = OreCountDiamond;

        RefinedCounters[OreNames.COAL] = RefinedCountCoal;
        RefinedCounters[OreNames.COPPER] = RefinedCountCopper;
        RefinedCounters[OreNames.IRON] = RefinedCountIron;
        RefinedCounters[OreNames.GOLD] = RefinedCountGold;
        RefinedCounters[OreNames.REDSTONE] = RefinedCountRedstone;
        RefinedCounters[OreNames.DIAMOND] = RefinedCountDiamond;
        OnMoneyChanged += onMoneyChanged;
        OnInventoryChanged += onInventoryChanged;
    }
}
