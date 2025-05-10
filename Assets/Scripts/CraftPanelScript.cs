using UnityEngine;
using UnityEngine.UI;
using static Player;
using static MiningScript;
using System.Collections.Generic;
using System;

public class CraftPanelScript : MonoBehaviour
{
    public Slider[] sliders = new Slider[6];
    public Image[] crafts = new Image[6];
    private int autoCraftLevel = -1;

    public static Dictionary<string, int> CraftTime = new Dictionary<string, int>
        {
            { OreNames.COAL, 3 },
            { OreNames.COPPER, 6 },
            { OreNames.IRON, 10 },
            { OreNames.GOLD, 20 },
            { OreNames.REDSTONE, 30 },
            { OreNames.DIAMOND, 60 },
            //{ OreNames.LAPIS, 120 },
            //{ OreNames.EMERALD,180 },
            //{ OreNames.QUARTZ, 300 },
        };

    private int[] craftingTimeLeft = new int[6];

    private void startCraft(string oreName)
    {
        int index = Array.IndexOf(OreNames.names, oreName);
        if (GetOre(oreName) >= 1 && furnaceLevel != 0 && (furnaceLevel - 1) / 2 >= index) {
            OnInventoryChanged?.Invoke(oreName, GetOre(oreName) - 1, ORE);
            craftingTimeLeft[index] = CraftTime[oreName];
            sliders[index].value = 0;
        }
    }

    private void endCraft(string oreName)
    {
        int index = Array.IndexOf(OreNames.names, oreName);
        if (index == -1)
        {
            Debug.LogError("Ore name not found in OreNames");
            return;
        }
        if (sliders[index].value >= sliders[index].maxValue && furnaceLevel != 0 && (furnaceLevel - 1) / 2 >= index)
        {
            // Crafting finished
            sliders[index].value = 0;
            craftingTimeLeft[index] = -1;
            OnInventoryChanged?.Invoke(oreName, GetRefined(oreName) + 1, REFINED);
        }
    }

    public void OnClickCraftCoal() => startCraft(OreNames.COAL);
    public void OnClickCraftCopper() => startCraft(OreNames.COPPER);
    public void OnClickCraftIron() => startCraft(OreNames.IRON);
    public void OnClickCraftGold() => startCraft(OreNames.GOLD);
    public void OnClickCraftRedstone() => startCraft(OreNames.REDSTONE);
    public void OnClickCraftDiamond() => startCraft(OreNames.DIAMOND);

    public void OnClickEndCraftCoal() => endCraft(OreNames.COAL);
    public void OnClickEndCraftCopper() => endCraft(OreNames.COPPER);
    public void OnClickEndCraftIron() => endCraft(OreNames.IRON);
    public void OnClickEndCraftGold() => endCraft(OreNames.GOLD);
    public void OnClickEndCraftRedstone() => endCraft(OreNames.REDSTONE);
    public void OnClickEndCraftDiamond() => endCraft(OreNames.DIAMOND);



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnFurnaceLevelChanged += onFurnaceLevelChanged;
        onFurnaceLevelChanged();

        for (int i = 0; i < sliders.Length; i++)
        {
            sliders[i].value = 0;
            sliders[i].maxValue = CraftTime[OreNames.names[i]];
            craftingTimeLeft[i] = -1; // Initialize crafting time left to -1 (not crafting)
            if ((furnaceLevel - 1) / 2 >= i) crafts[i].gameObject.SetActive(true);
            else crafts[i].gameObject.SetActive(false);
        }
    }

    private void onFurnaceLevelChanged()
    {
        autoCraftLevel = furnaceLevel / 2 - 1;
        for (int i = 0; i < crafts.Length; i++)
        {
            if (furnaceLevel == 0) break;
            if ((furnaceLevel - 1) / 2 >= i) crafts[i].gameObject.SetActive(true);
            else crafts[i].gameObject.SetActive(false);
        }
    }


    // Update is called once per frame
    void Update()
    {   
        for (int i=0; i < sliders.Length; i++)
        {
            if (sliders[i].value < sliders[i].maxValue && craftingTimeLeft[i] != -1) sliders[i].value += Time.deltaTime;
            else
            {
                if (autoCraftLevel >= i)
                {
                    endCraft(OreNames.names[i]);
                    startCraft(OreNames.names[i]);
                } else craftingTimeLeft[i] = -1;
            }
        }
    }
}
