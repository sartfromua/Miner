using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class PlayerData
{
    public int score = 0;
    public int money = 0;
    public int blocksBroken = 0;
    public int damage = 1;
    public string playerName;
    // Ключ — ID руды, значение — количество
    public Dictionary<string, int> OresInventory = new Dictionary<string, int>();
    // Ключ — ID апгрейда, значение — текущий уровень
    public Dictionary<string, int> Upgrades = new Dictionary<string, int>();

    public PlayerData()
    {
        score = 0;
        money = 0;
        playerName = "NewbieMiner";
    }

    public override string ToString()
    {
        return playerName + " \nMoney: " + money + 
               " \nOres: " + JsonConvert.SerializeObject(OresInventory, Formatting.Indented) + 
               " \nUpgrades: " + JsonConvert.SerializeObject(Upgrades, Formatting.Indented);
    }
}