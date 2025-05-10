using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Player;

public class MiningScript : MonoBehaviour
{
    public GameObject ShopPanel;
    public GameObject MainPanel;
    public GameObject SmithPanel;
    public GameObject CraftPanel;
    public GameObject OfflineIncomePanel;
    public Image OreImage;
    public Image DestroyStageImage;
    public AudioSource StoneSound;
    public TMP_Text[] OreChancesUI = new TMP_Text[6];

    private int destroyStage = 0;

    private Save sv = new Save();

    List<Ore> Ores = new List<Ore>();
    List<Sprite> DestroyStages = new List<Sprite>();

    private Ore ChosenOre;

    public Text MoneyText;

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
        public static readonly string[] names = { COAL, COPPER, IRON, GOLD, REDSTONE, DIAMOND };

    }

    public void TogglePanel(GameObject panel)
    {
        ShopPanel.SetActive(false);
        MainPanel.SetActive(false);
        SmithPanel.SetActive(false);
        CraftPanel.SetActive(false);
        OfflineIncomePanel.SetActive(false);
        panel.SetActive(true);
    }

    public void OnClickOreImage() {

        StoneSound.Play();
        destroyStage += pickaxePower;
        if (NextDestroyStage())
        {
            ChooseRandomOre();
        }

    }

    private void addToInventory(Ore ore) => OnOneOreMined?.Invoke(ore.name);

    private bool checkDestroyStage()
    {
        if (destroyStage >= DestroyStages.Count)
        {
            while (destroyStage >= DestroyStages.Count)
            {
                addToInventory(ChosenOre);
                destroyStage -= DestroyStages.Count;
            }
            return true;
        }
        return false;
    }

    private bool NextDestroyStage()
    {
        DestroyStageImage.sprite = DestroyStages[destroyStage % Math.Max(DestroyStages.Count, 1)];
        return checkDestroyStage();
    }

    private void ChooseRandomOre()
    {   
        string oreName = GetRandomOreName();
        ChosenOre = Ores.Find(ore => ore.name == oreName);  
        Debug.Log("Chosen ore: " + ChosenOre);
        OreImage.sprite = ChosenOre.sprite;
    }

    public string GetRandomOreName()
    {
        double totalWeight = OreChances.Values.Sum(); // Сумма всех вероятностей
        double randomValue = UnityEngine.Random.Range(0f, (float)totalWeight);
        double cumulative = 0;

        foreach (var ore in OreChances)
        {
            cumulative += ore.Value;
            if (randomValue <= cumulative)
            {
                return ore.Key; // Выбрали руду
            }
        }

        return OreChances.Keys.First(); // На случай ошибки, вернем первый элемент
    }

    public void OnClickShopButton()
    {
        TogglePanel(ShopPanel);
    }

    public void OnClickCraftButton()
    {
        TogglePanel(CraftPanel);
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

        array = Resources.LoadAll("Destroy_stages", typeof(Sprite));
        for (var i = 0; i < array.Length; i++) DestroyStages.Add(array[i] as Sprite);
    }

    private IEnumerator LoadSpritesAsync()
    {
        // Загрузка руд
        var oreArray = Resources.LoadAll("Ores", typeof(Sprite));
        for (var i = 0; i < oreArray.Length; i++)
        {
            Sprite sprite = oreArray[i] as Sprite;
            string name = "";

            foreach (var naming in OreNames.names)
            {
                if (sprite.name.Contains(naming))
                {
                    name = naming;
                    break;
                }
            }

            int durability = 2;
            Ores.Add(new Ore(name, sprite, durability));

            // Немного подождем, чтобы не фризить главный поток
            if (i % 10 == 0)
                yield return null;
        }

        // Загрузка стадий разрушения
        var stageArray = Resources.LoadAll("Destroy_stages", typeof(Sprite));
        for (var i = 0; i < stageArray.Length; i++)
        {
            DestroyStages.Add(stageArray[i] as Sprite);

            if (i % 10 == 0)
                yield return null;
        }

        // Загрузка завершена
        Debug.Log("Спрайты загружены");
    }

    private IEnumerator LoadSpritesAndThenLoadSave()
    {
        yield return StartCoroutine(LoadSpritesAsync());
        ChooseRandomOre();

        // Loading save
        if (PlayerPrefs.HasKey("SV") && true)
        {
            sv = JsonConvert.DeserializeObject<Save>(PlayerPrefs.GetString("SV"));
            LoadSave(sv);

            // Calculating offline income
            getOfflineIncome();
        }
    }

    private void onMoneyChanged(int money)
    {
        MoneyText.text = money + "GOLD";
    }

    private void getOfflineIncome()
    {
        DateTime now = DateTime.Now;
        TimeSpan offlineTime = now - lastSaveDate;
        int offlineSeconds = (int)offlineTime.TotalSeconds;

        // Если нечего считать — выходим
        if (offlineSeconds <= 0 || pickaxeAutoPower <= 0) return;

        // Общая сумма шансов (нормализация)
        double totalChance = OreChances.Values.Sum();

        // Подсчёт общего количества попыток
        double totalAttempts = (offlineSeconds * pickaxeAutoPower) / DestroyStages.Count;

        // Подсчёт количества каждой руды на основе вероятности
        Dictionary<string, int> earnedOres = new Dictionary<string, int>();
        foreach (var kvp in OreChances)
        {
            double chance = kvp.Value / totalChance;
            int oreCount = (int)(chance * totalAttempts); // можно использовать Math.Floor/Math.Round по желанию
            earnedOres[kvp.Key] = oreCount;
        }


        // Пример вывода результатов (можешь заменить на добавление в инвентарь и т.п.)


        // Auto-melting
        Dictionary<string, int> smeltedOres = new Dictionary<string, int>();

        foreach (var ore in OreNames.names)
        {
            smeltedOres[ore] = 0;

            // Проверка: есть ли руда в CraftTime и в списке names
            if (!CraftPanelScript.CraftTime.TryGetValue(ore, out int timePerOne) || timePerOne <= 0)
                continue;

            int oreIndex = Array.IndexOf(OreNames.names, ore);
            if (oreIndex < 0)
                continue;

            // furnaceLevel = 0 — ничего, 2 — первая руда, 4 — две и т.д.
            if (furnaceLevel >= oreIndex * 2)
            {
                int earned = earnedOres[ore];
                smeltedOres[ore] = Math.Min(earned, offlineSeconds / timePerOne);
                earnedOres[ore] -= smeltedOres[ore];
            }
        }

        foreach (var ore in OreNames.names)
        {
            OnInventoryChanged?.Invoke(ore, GetRefined(ore) + smeltedOres[ore], REFINED);
            OnInventoryChanged?.Invoke(ore, GetOre(ore) + earnedOres[ore], ORE);
            Debug.Log($"Smelted {smeltedOres[ore]} единиц {ore} во время оффлайна.");
            Debug.Log($"Mined {earnedOres[ore]} единиц {ore} во время оффлайна.");
            TogglePanel(OfflineIncomePanel);
            showOfflineIncome(earnedOres, smeltedOres);
        }
    }

    public TMP_Text[] OfflineResCounters = new TMP_Text[12];

    public void showOfflineIncome(Dictionary<string, int> earnedOres, Dictionary<string, int> smeltedOres)
    {
        foreach (var ore in OreNames.names)
        {
            int index = Array.IndexOf(OreNames.names, ore);
            if (index == -1)
            {
                Debug.LogError("Ore name not found in OreNames");
                return;
            }
            OfflineResCounters[index].text = earnedOres[ore].ToString();
            OfflineResCounters[index + 6].text = smeltedOres[ore].ToString();
        }
    }

    private void onOreChancesChanged()
    {
        int i = 0;
        double totalWeight = OreChances.Values.Sum();
        foreach (TMP_Text tMP_Text in OreChancesUI)
        {
            string oreName = OreNames.names[i];
            double weight = OreChances[oreName];
            double percentage = (weight / totalWeight) * 100;
            tMP_Text.text = " " + percentage.ToString("F2");
            i++;
        }
    }


    private void Start()
    {
        // Observers
        OnMoneyChanged += onMoneyChanged;
        OnOreChancesChanged += onOreChancesChanged;

        TogglePanel(MainPanel);

        StartCoroutine(LoadSpritesAndThenLoadSave());

        OreChances = ShopPanelScripts.CalculateOreChances(oreLevel);

        OnOreChancesChanged?.Invoke();

        StartCoroutine(AutoMine());

        
    }

    private IEnumerator AutoMine()
    {
        while (true)
        {
            if (pickaxeAutoPower > 0)
            {
                destroyStage += pickaxeAutoPower;
                if (NextDestroyStage())
                {
                    ChooseRandomOre();
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void Update() {
        
    }

    private void Awake()
    {
        
    }

    private void OnApplicationQuit()
    {
        sv = makeSave();
        PlayerPrefs.SetString("SV", JsonConvert.SerializeObject(sv));
    }

}

public class Save
{
    public int money;
    public Dictionary<string, int> oresInventory;
    public Dictionary<string, int> refinedInventory;
    public int pickaxePower;
    public int pickaxeAutoPower;
    public int oreLevel;
    public int furnaceLevel;
    public int[] goodsPrices;
    public int[] lastSaveDate = new int[6];
}
