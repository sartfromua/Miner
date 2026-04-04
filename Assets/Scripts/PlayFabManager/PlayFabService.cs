using System;
using System.Collections.Generic;
using EquipmentCraft;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using Shop.Upgrades;
using UnityEngine;

public class PlayFabService : MonoBehaviour
{
    /// <summary>
    /// PlayFab ID поточного гравця. Заповнюється після логіну.
    /// </summary>
    public static string LocalPlayFabId { get; private set; }

    // ─────────────────────────────────────────────────────────────
    // КЛЮЧІ USERDАТА — кожна секція зберігається окремо
    // ─────────────────────────────────────────────────────────────

    private const string KeyProfile          = "PlayerProfile";      // { playerName }
    private const string KeyStats            = "PlayerStats";         // { score, money, blocksBroken, damage }
    private const string KeyOresInventory    = "OresInventory";       // Dictionary<string, int>
    private const string KeyRefinedInventory = "RefinedInventory";    // Dictionary<string, int>
    private const string KeyUpgrades         = "Upgrades";            // Dictionary<string, int>
    private const string KeyCraftState       = "EquipmentCraftState"; // EquipmentCraftState
    private const string KeyEquipInventory   = "EquipmentInventory";  // List<EquipmentItem>
    private const string KeyEquippedItems    = "EquippedItems";       // Dictionary<EquipmentType, EquipmentItem>
    private const string KeyCraftSlots       = "CraftSlots";          // List<FurnaceSlotData>

    private static readonly List<string> AllKeys = new List<string>
    {
        KeyProfile, KeyStats, KeyOresInventory, KeyRefinedInventory,
        KeyUpgrades, KeyCraftState, KeyEquipInventory, KeyEquippedItems, KeyCraftSlots
    };

    // ─────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("Нет сети! Вход невозможен.");
            return;
        }
        Login();
    }

    // ─────────────────────────────────────────────────────────────
    // 1. ВХІД
    // ─────────────────────────────────────────────────────────────

    public void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, result =>
        {
            LocalPlayFabId = result.PlayFabId;
            LoadTitleData();
        }, OnError);
    }

    // ─────────────────────────────────────────────────────────────
    // 2. TITLE DATA (налаштування сервера: ціни, шанси)
    // ─────────────────────────────────────────────────────────────

    private void LoadTitleData()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(), result =>
        {
            if (result.Data.TryGetValue("OreSettings", out var oresJson))
            {
                var parsedOres = JsonConvert.DeserializeObject<Dictionary<string, OreServerData>>(oresJson);
                GameDataManager.Instance.UpdateOresData(parsedOres);
            }
            if (result.Data.TryGetValue("UpgradeSettings", out var upgradesJson))
            {
                var parsedUpgrades = JsonConvert.DeserializeObject<Dictionary<string, UpgradeServerData>>(upgradesJson);
                GameDataManager.Instance.UpdateUpgradesData(parsedUpgrades);
            }
            LoadPlayerData();
        }, OnError);
    }

    // ─────────────────────────────────────────────────────────────
    // 3. ЗАВАНТАЖЕННЯ ДАНИХ ГРАВЦЯ
    // ─────────────────────────────────────────────────────────────

    private void LoadPlayerData()
    {
        Debug.Log($"<color=cyan>LoadPlayerData()</color> <b>Start</b>");

        PlayFabClientAPI.GetUserData(new GetUserDataRequest { Keys = AllKeys }, result =>
        {
            var data = result.Data;

            // Новий гравець — профіль ще не створено
            if (!data.ContainsKey(KeyProfile))
            {
                Debug.Log("Новий гравець! Створюємо стартовий профіль...");
                var freshData = new PlayerData();
                GameDataManager.Instance.UpdatePlayerData(freshData);
                SavePlayerData(freshData);
            }
            else
            {
                var playerData = AssemblePlayerData(data);
                Debug.Log($"<color=cyan>LoadPlayerData()</color> PlayerData: {playerData}");
                GameDataManager.Instance.UpdatePlayerData(playerData);
            }

            Debug.Log($"<color=cyan>LoadPlayerData()</color> <b>Success</b>");
            GameDataManager.Instance.OnDataLoaded?.Invoke();
        }, OnError);

        Debug.Log($"<color=cyan>LoadPlayerData()</color> <b>Finish</b>");
    }

    /// <summary>
    /// Збирає <see cref="PlayerData"/> з окремих UserData-записів.
    /// Будь-який відсутній ключ замінюється дефолтним значенням.
    /// </summary>
    private static PlayerData AssemblePlayerData(Dictionary<string, UserDataRecord> data)
    {
        var pd = new PlayerData();

        if (data.TryGetValue(KeyProfile, out var profileRec))
        {
            var profile = JsonConvert.DeserializeObject<ProfileSaveData>(profileRec.Value);
            if (profile != null)
                pd.playerName = profile.playerName;
        }

        if (data.TryGetValue(KeyStats, out var statsRec))
        {
            var stats = JsonConvert.DeserializeObject<StatsSaveData>(statsRec.Value);
            if (stats != null)
            {
                pd.score        = stats.score;
                pd.money        = stats.money;
                pd.blocksBroken = stats.blocksBroken;
                pd.damage       = stats.damage;
            }
        }

        if (data.TryGetValue(KeyOresInventory, out var oresRec))
            pd.OresInventory = JsonConvert.DeserializeObject<Dictionary<string, int>>(oresRec.Value)
                               ?? new Dictionary<string, int>();

        if (data.TryGetValue(KeyRefinedInventory, out var refinedRec))
            pd.RefinedInventory = JsonConvert.DeserializeObject<Dictionary<string, int>>(refinedRec.Value)
                                  ?? new Dictionary<string, int>();

        if (data.TryGetValue(KeyUpgrades, out var upgradesRec))
            pd.Upgrades = JsonConvert.DeserializeObject<Dictionary<string, int>>(upgradesRec.Value)
                          ?? new Dictionary<string, int>();

        if (data.TryGetValue(KeyCraftState, out var craftStateRec))
            pd.equipmentCraftState = JsonConvert.DeserializeObject<EquipmentCraftState>(craftStateRec.Value)
                                     ?? new EquipmentCraftState();

        if (data.TryGetValue(KeyEquipInventory, out var equipInvRec))
            pd.equipmentInventory = JsonConvert.DeserializeObject<List<EquipmentItem>>(equipInvRec.Value)
                                    ?? new List<EquipmentItem>();

        if (data.TryGetValue(KeyEquippedItems, out var equippedRec))
            pd.equippedItems = JsonConvert.DeserializeObject<Dictionary<EquipmentType, EquipmentItem>>(equippedRec.Value)
                               ?? new Dictionary<EquipmentType, EquipmentItem>();

        if (data.TryGetValue(KeyCraftSlots, out var craftSlotsRec))
            pd.craftSlots = JsonConvert.DeserializeObject<List<PlayerData.FurnaceSlotData>>(craftSlotsRec.Value)
                            ?? new List<PlayerData.FurnaceSlotData>();

        return pd;
    }

    // ─────────────────────────────────────────────────────────────
    // 4. ЗБЕРЕЖЕННЯ ДАНИХ ГРАВЦЯ
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Зберігає всі дані гравця в PlayFab UserData, кожну секцію окремим ключем.
    /// </summary>
    public static void SavePlayerData(PlayerData data, Action onSuccess = null, Action<string> onError = null)
    {
        var saveData = new Dictionary<string, string>
        {
            [KeyProfile]          = JsonConvert.SerializeObject(new ProfileSaveData { playerName = data.playerName }),
            [KeyStats]            = JsonConvert.SerializeObject(new StatsSaveData
            {
                score        = data.score,
                money        = data.money,
                blocksBroken = data.blocksBroken,
                damage       = data.damage
            }),
            [KeyOresInventory]    = JsonConvert.SerializeObject(data.OresInventory),
            [KeyRefinedInventory] = JsonConvert.SerializeObject(data.RefinedInventory),
            [KeyUpgrades]         = JsonConvert.SerializeObject(data.Upgrades),
            [KeyCraftState]       = JsonConvert.SerializeObject(data.equipmentCraftState),
            [KeyEquipInventory]   = JsonConvert.SerializeObject(data.equipmentInventory),
            [KeyEquippedItems]    = JsonConvert.SerializeObject(data.equippedItems),
            [KeyCraftSlots]       = JsonConvert.SerializeObject(data.craftSlots),
        };

        var request = new UpdateUserDataRequest { Data = saveData };
        PlayFabClientAPI.UpdateUserData(request, _ =>
        {
            Debug.Log("Збережено");
            // Статистики для лідерборду оновлюються через CloudScript (SyncLeaderboardStats handler)
            SyncLeaderboardStatisticsViaCloudScript(data);
            onSuccess?.Invoke();
        }, error =>
        {
            OnErrorStatic(error);
            onError?.Invoke(error.ErrorMessage);
        });
    }

    /// <summary>
    /// Синхронізує score/money/blocksBroken у PlayFab Statistics через CloudScript.
    /// CloudScript має Server API доступ — не потребує клієнтських дозволів.
    /// </summary>
    private static void SyncLeaderboardStatisticsViaCloudScript(PlayerData data)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "SyncLeaderboardStats",
            FunctionParameter = new
            {
                score        = data.score,
                money        = data.money,
                blocksBroken = data.blocksBroken
            },
            GeneratePlayStreamEvent = false
        };
        PlayFabClientAPI.ExecuteCloudScript(request,
            _ => Debug.Log("Leaderboard stats synced"),
            OnErrorStatic);
    }

    /// <summary>
    /// Оновлює відображуване ім'я гравця на PlayFab (використовується в лідерборді).
    /// Викликати явно після зміни нікнейму.
    /// </summary>
    public static void SetDisplayName(string name, Action onSuccess = null, Action<string> onError = null)
    {
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = name };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            _ =>
            {
                Debug.Log($"DisplayName оновлено: {name}");
                onSuccess?.Invoke();
            },
            error =>
            {
                OnErrorStatic(error);
                onError?.Invoke(error.ErrorMessage);
            });
    }

    // ─────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────
    
    private void OnError(PlayFabError error)
    {
        Debug.Log($"HttpCode: {error.HttpCode}, Error: {error.Error}, ErrorMessage: {error.ErrorMessage}");
        if (error.HttpCode == 409)
        {
            Debug.LogWarning("Account already exists — logging in without CreateAccount...");
        
            var request = new LoginWithCustomIDRequest
            {
                CustomId = PlayerPrefs.GetString("CustomId", SystemInfo.deviceUniqueIdentifier),
                CreateAccount = false // ← просто логінимось в існуючий
            };
        
            PlayFabClientAPI.LoginWithCustomID(request, result =>
            {
                LocalPlayFabId = result.PlayFabId;
                LoadTitleData();
            }, error => Debug.LogError(error.GenerateErrorReport()));
        }
        else
        {
            Debug.LogError(error.GenerateErrorReport());
        }
    }
    
    private string GenerateFreshCustomId()
    {
        var deviceId = SystemInfo.deviceUniqueIdentifier;

        // deviceUniqueIdentifier може повертати "00000000..." на деяких пристроях
        if (string.IsNullOrEmpty(deviceId) || deviceId == SystemInfo.unsupportedIdentifier)
        {
            deviceId = Guid.NewGuid().ToString("N");
        }

        // Додаємо суфікс, щоб уникнути колізії якщо той самий пристрій
        // реєструється повторно після видалення акаунта
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return $"player_{deviceId}_{suffix}";
    }
    
    private static void OnErrorStatic(PlayFabError error) => Debug.LogError(error.GenerateErrorReport());

    // Дані профілю — зберігається під KeyProfile
    [Serializable]
    private class ProfileSaveData
    {
        public string playerName;
    }

    // Основні числові характеристики — зберігаються під KeyStats
    [Serializable]
    private class StatsSaveData
    {
        public int score;
        public int money;
        public int blocksBroken;
        public int damage;
    }
}
