using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MiningScript;
using static Player;


// TODO: Добавить количество добытой руды на экран

public class BlackSmithPanelScripts : MonoBehaviour
{

    public Text MoneyText;

    private Dictionary<string, int> OrePrices = new Dictionary<string, int>
    {
        { OreNames.COAL, 1 },
        { OreNames.COPPER, 2 },
        { OreNames.IRON, 3 },
        { OreNames.GOLD, 4 },
        { OreNames.DIAMOND, 5 },
        { OreNames.LAPIS, 6 },
        { OreNames.REDSTONE, 7 },
        { OreNames.EMERALD, 8 },
        { OreNames.QUARTZ, 9 },
    };

    public void OnClickSellCoal()
    {   
        if (GetOre(OreNames.COAL) > 0)
        {
            Money += GetOre(OreNames.COAL) * OrePrices[OreNames.COAL];
            SetOre(OreNames.COAL, 0);
        }
    }

    public void OnClickSellIron()
    {
        if (GetOre(OreNames.IRON) > 0)
        {
            Money += GetOre(OreNames.IRON) * OrePrices[OreNames.IRON];
            SetOre(OreNames.IRON, 0);
        }
    }

    public void OnClickSellGold()
    {
        if (GetOre(OreNames.GOLD) > 0)
        {
            Money += GetOre(OreNames.GOLD) * OrePrices[OreNames.GOLD];
            SetOre(OreNames.GOLD, 0);
        }
    }

    public void OnClickSellCopper()
    {
        if (GetOre(OreNames.COPPER) > 0)
        {
            Money += GetOre(OreNames.COPPER) * OrePrices[OreNames.COPPER];
            SetOre(OreNames.COPPER, 0);
        }
    }

    public void OnClickSellDiamond()
    {
        if (GetOre(OreNames.DIAMOND) > 0)
        {
            Money += GetOre(OreNames.DIAMOND) * OrePrices[OreNames.DIAMOND];
            SetOre(OreNames.DIAMOND, 0);
        }
    }



    public void OnClickSellEmerald()
    {
        if (GetOre(OreNames.EMERALD) > 0)
        {
            Money += GetOre(OreNames.EMERALD) * OrePrices[OreNames.EMERALD];
            SetOre(OreNames.EMERALD, 0);
        }
    }

    public void OnClickSellRedstone()
    {
        if (GetOre(OreNames.REDSTONE) > 0)
        {
            Money += GetOre(OreNames.REDSTONE) * OrePrices[OreNames.REDSTONE];
            SetOre(OreNames.REDSTONE, 0);
        }
    }

    public void OnClickSellLapis()
    {
        if (GetOre(OreNames.LAPIS) > 0)
        {
            Money += GetOre(OreNames.LAPIS) * OrePrices[OreNames.LAPIS];
            SetOre(OreNames.LAPIS, 0);
        }
    }

    public void OnClickSellQuartz()
    {
        if (GetOre(OreNames.QUARTZ) > 0)
        {
            Money += GetOre(OreNames.QUARTZ) * OrePrices[OreNames.QUARTZ];
            SetOre(OreNames.QUARTZ, 0);
        }
    }

    public void OnClickSellAll()
    {
        foreach (string oreName in OreNames.names)
        {
            if (GetOre(oreName) <= 0) continue;
            Money += GetOre(oreName) * OrePrices[oreName];
            SetOre(oreName, 0);
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MoneyText.text = Money + "GOLD";
    }
}
