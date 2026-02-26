using System.Collections.Generic;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using Shop.Upgrades;
using UnityEngine;

public class PlayFabService : MonoBehaviour
{
    
    private void Start()
    {
        // Проверка интернета перед входом
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("Нет сети! Вход невозможен.");
            return; 
        }
        Login(); // <--- АВТОМАТИЧЕСКИЙ ВЫЗОВ
    }
    // 1. Вход
    public void Login()
    {
        var request = new LoginWithCustomIDRequest {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, result => {
            LoadTitleData(); // Сначала грузим общие настройки
        }, OnError);
    }

    // 2. Загрузка настроек сервера (Цены, Шансы)
    private void LoadTitleData()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), result => {
            if (result.Data.TryGetValue("OreSettings", out var oresJson)) {
            
                // 1. Превращаем JSON-строку в Словарь
                var parsedOres = JsonConvert.DeserializeObject<Dictionary<string, OreServerData>>(oresJson);
            
                // 2. Передаем готовый словарь в менеджер (через Instance, т.к. метод не статический)
                GameDataManager.Instance.UpdateOresData(parsedOres);
            }
            if (result.Data.TryGetValue("UpgradeSettings", out var upgradesJson)) {
            
                // 1. Превращаем JSON-строку в Словарь
                var parsedUpgrades = JsonConvert.DeserializeObject<Dictionary<string, UpgradeServerData>>(upgradesJson);
            
                // 2. Передаем готовый словарь в менеджер (через Instance, т.к. метод не статический)
                GameDataManager.Instance.UpdateUpgradesData(parsedUpgrades);
            }
            
            
        
            LoadPlayerData(); // Грузим игрока
        }, OnError);
    }

    // 3. Загрузка данных игрока (Деньги, Инвентарь)
    private void LoadPlayerData()
    {
        Debug.Log($"<color=cyan>LoadPlayerData()</color> <b>Start</b>");
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result => {
            if (result.Data.TryGetValue("PlayerData", out var record)) {
                // Если данные есть, десериализуем JSON в наш класс
                var data = JsonConvert.DeserializeObject<PlayerData>(record.Value);
                Debug.Log($"<color=cyan>LoadPlayerData()</color> PlayerData: {data}");
                GameDataManager.Instance.UpdatePlayerData(data);
            } else {
                // Если данных нет (новый игрок), инициализируем пустые
                Debug.Log("Новый игрок! Создаем стартовый профиль...");
                var freshData = new PlayerData(); 
                GameDataManager.Instance.UpdatePlayerData(freshData);
                SavePlayerData(freshData); // Сразу сохраняем дефолт на сервер
            }
            Debug.Log($"<color=cyan>LoadPlayerData()</color> <b>Success</b>");
            GameDataManager.Instance.OnDataLoaded?.Invoke();
        }, OnError);
        Debug.Log($"<color=cyan>LoadPlayerData()</color> <b>Finish</b>");
        
    }

    private void OnError(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());

    // Статический метод для сохранения (вызывай его из любого места)
    public static void SavePlayerData(PlayerData data)
    {
        var json = JsonConvert.SerializeObject(data);
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> { { "PlayerData", json } }
        };
        PlayFabClientAPI.UpdateUserData(request, r => Debug.Log("Сохранено"), OnErrorStatic);
    }

    private static void OnErrorStatic(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());
}