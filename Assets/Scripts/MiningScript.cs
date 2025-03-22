using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Player;

public class MiningScript : MonoBehaviour
{
    public GameObject ShopPanel;
    public GameObject MainPanel;
    public GameObject SmithPanel;
    public Image OreImage;
    public Image DestroyStageImage;
    private int destroyStage = 0;

    List<Ore> Ores = new List<Ore>();
    List<Sprite> DestroyStages = new List<Sprite>();

    private Ore ChosenOre;

    public Text MoneyText;

    public static class OreNames
    {
        public const string COAL = "coal";
        public const string IRON = "iron";
        public const string GOLD = "gold";
        public const string COPPER = "copper";
        public const string DIAMOND = "diamond";
        public const string LAPIS = "lapis";
        public const string REDSTONE = "redstone";
        public const string EMERALD = "emerald";
        public const string QUARTZ = "quartz";
        public static readonly string[] names = { COAL, IRON, GOLD, COPPER, DIAMOND, LAPIS, REDSTONE, EMERALD, QUARTZ };

    }

    public static Dictionary<string, double> OreChances = new Dictionary<string, double>
    {
        { OreNames.COAL, 9 },
        { OreNames.COPPER, 1 },
        { OreNames.IRON, 0 },
        { OreNames.GOLD, 0 },
        { OreNames.DIAMOND, 0 },
        { OreNames.LAPIS, 0 },
        { OreNames.REDSTONE, 0 },
        { OreNames.EMERALD, 0 },
        { OreNames.QUARTZ, 0 },
    };


    private void TogglePanel(GameObject panel)
    {
        ShopPanel.SetActive(false);
        MainPanel.SetActive(false);
        SmithPanel.SetActive(false);
        panel.SetActive(true);
        ShopPanelScripts.checkMoneyForShopPrices();
    }

    public void OnClickOreImage() {

        if (NextDestroyStage())
        {
            addToInventory(ChosenOre);
            ChooseRandomOre();
        }

    }

    private void addToInventory(Ore ore) => AddOneOre(ore.name);

    private bool NextDestroyStage()
    {
        destroyStage += pickaxePower;
        DestroyStageImage.sprite = DestroyStages[destroyStage % DestroyStages.Count];
        if (destroyStage >= DestroyStages.Count)
        {
            destroyStage -= DestroyStages.Count;
            return true;
        }
        return false;
    }

    private void ChooseRandomOre()
    {   
        string oreName = GetRandomOreName(OreChances);
        ChosenOre = Ores.Find(ore => ore.name == oreName);  
        Debug.Log("Chosen ore: " + ChosenOre);
        OreImage.sprite = ChosenOre.sprite;
    }

    public string GetRandomOreName(Dictionary<string, double> oreChances)
    {
        double totalWeight = oreChances.Values.Sum(); // Сумма всех вероятностей
        double randomValue = Random.Range(0f, (float)totalWeight);
        double cumulative = 0;

        foreach (var ore in oreChances)
        {
            cumulative += ore.Value;
            if (randomValue <= cumulative)
            {
                return ore.Key; // Выбрали руду
            }
        }

        return oreChances.Keys.First(); // На случай ошибки, вернем первый элемент
    }

    public void OnClickShopButton()
    {
        TogglePanel(ShopPanel);
    }

    public void OnClickMainButton()
    {
        TogglePanel(MainPanel);
    }

    public void OnClickSmithButton()
    {
        TogglePanel(SmithPanel);
    }

    


    private void loadSprites()
    {
        var array = Resources.LoadAll("Ores", typeof(Sprite));
        for (var i = 0; i < array.Length; i++)
        {
            Sprite sprite = array[i] as Sprite;
            string name = "";
            foreach (var naming in OreNames.names) {
                if (sprite.name.Contains(naming))
                {
                    name = naming;
                    break;
                }
            }  
            int durability = 2;
            Ores.Add(new Ore(name, sprite, durability));
        }

        foreach (var name in OreNames.names)
        {
            SetOre(name, 0);
        }
        

        array = Resources.LoadAll("Destroy_stages", typeof(Sprite));
        for (var i = 0; i < array.Length; i++) DestroyStages.Add(array[i] as Sprite);

        ChooseRandomOre();
    }
    
    private void Start()
    {
        loadSprites();
        Money = 1000;
    }

    private void Update() {
        MoneyText.text = Money + "GOLD";
    }

    private class Ore
    {
        public int price;
        public string name;
        public Sprite sprite;
        public int durability;

        public Ore(string name, Sprite sprite, int durability)
        {
            this.name = name;
            this.sprite = sprite;
            this.durability = durability;
        }
        
        public override string ToString()
        {
            return string.Format("{0}: price={1} durability={2}", name, price, durability);
        }
    }

}
