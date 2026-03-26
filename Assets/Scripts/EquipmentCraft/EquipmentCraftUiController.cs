using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EquipmentCraft
{
    public class EquipmentCraftUiController : MonoBehaviour
    {
        [Header("Craft setup")]
        [SerializeField] private int addAmount = 1;
        [SerializeField] private int takeAmount = 1;

        [Header("Buttons")]
        [SerializeField] private Button addRefinedButton;
        [SerializeField] private Button addAllRefinedButton;
        [SerializeField] private Button takeRefinedButton;
        [SerializeField] private Button startCraftButton;
        [SerializeField] private Button finishCraftButton;

        [Header("Optional CanvasGroups for alpha")]
        [SerializeField] private CanvasGroup addRefinedCanvasGroup;
        [SerializeField] private CanvasGroup addAllRefinedCanvasGroup;
        [SerializeField] private CanvasGroup takeRefinedCanvasGroup;
        [SerializeField] private CanvasGroup startCraftCanvasGroup;
        [SerializeField] private CanvasGroup finishCraftCanvasGroup;
        [SerializeField] private float disabledAlpha = 0.35f;
        [SerializeField] private float enabledAlpha = 1f;

        [Header("Progress UI")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI timeLeftText;

        [Header("Ore preview")]
        [SerializeField] private Image nextOreImage;
        [SerializeField] private Sprite emptyOreSprite;
        [SerializeField] private Color emptyOreColor = new Color(1f, 1f, 1f, 0.25f);
        [SerializeField] private Color filledOreColor = Color.white;

        [Header("Finish action")]
        [SerializeField] private UnityEvent onFinishCraftClicked;

        [Header("Detail Panel")]
        [SerializeField] private Ui.EquipmentDetailPanel detailPanel;

        private static GameDataManager Manager => GameDataManager.Instance;

        private void Awake()
        {
            if (addRefinedButton) addRefinedButton.onClick.AddListener(OnAddRefinedClicked);
            if (addAllRefinedButton) addAllRefinedButton.onClick.AddListener(OnAddAllRefinedClicked);
            if (takeRefinedButton) takeRefinedButton.onClick.AddListener(OnTakeRefinedClicked);
            if (startCraftButton) startCraftButton.onClick.AddListener(OnStartCraftClicked);
            if (finishCraftButton) finishCraftButton.onClick.AddListener(OnFinishCraftClicked);
        }

        private void OnEnable()
        {
            if (Manager != null)
            {
                Manager.OnDataUpdated += RefreshUI;
                Manager.OnDataLoaded += RefreshUI;
            }

            RefreshUI();
        }

        private void OnDisable()
        {
            if (Manager != null)
            {
                Manager.OnDataUpdated -= RefreshUI;
                Manager.OnDataLoaded -= RefreshUI;
            }
        }

        private void Update()
        {
            RefreshProgressOnly();
            RefreshButtons();
        }

        private EquipmentCraftState GetState()
        {
            if (!Manager || Manager.playerData == null)
                return null;

            return Manager.playerData.equipmentCraftState;
        }

        private void RefreshUI()
        {
            RefreshNextOreIcon();
            RefreshProgressOnly();
            RefreshButtons();
        }

        private void RefreshProgressOnly()
        {
            var state = GetState();

            if (state == null)
            {
                if (progressSlider) progressSlider.value = 0f;
                if (timeLeftText) timeLeftText.text = "--:--";
                return;
            }

            var isRunning = state.craftStartTimeUnix > 0;
            var duration = state.craftDurationSeconds;

            if (!isRunning || duration <= 0)
            {
                if (progressSlider) progressSlider.value = 0f;
                if (timeLeftText) timeLeftText.text = isRunning ? "00:00" : "--:--";
                return;
            }

            var progress = Manager.GetRefinedCraftProgress(duration);
            if (progressSlider) progressSlider.value = progress;

            var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var elapsed = nowUnix - state.craftStartTimeUnix;
            var remaining = Mathf.Max(0, state.craftDurationSeconds - (int)elapsed);

            if (timeLeftText)
                timeLeftText.text = FormatTime(remaining);
        }

        private void RefreshNextOreIcon()
        {
            if (!nextOreImage)
                return;

            var state = GetState();
            if (state == null || !Manager || !Manager.refinedDataBase)
            {
                SetEmptyNextOreIcon();
                return;
            }

            var nextOreId = state.GetNextOreId();
            if (string.IsNullOrWhiteSpace(nextOreId))
            {
                SetEmptyNextOreIcon();
                return;
            }

            var oreData = Manager.refinedDataBase.GetOreById(nextOreId);
            if (!oreData || !oreData.icon)
            {
                SetEmptyNextOreIcon();
                return;
            }

            nextOreImage.sprite = oreData.icon;
            nextOreImage.color = filledOreColor;
            nextOreImage.enabled = true;
        }

        private void SetEmptyNextOreIcon()
        {
            if (!nextOreImage) return;

            nextOreImage.sprite = emptyOreSprite;
            nextOreImage.color = emptyOreColor;
            nextOreImage.enabled = emptyOreSprite;
        }

        private void RefreshButtons()
        {
            var state = GetState();
            if (state == null || !Manager)
            {
                SetButtonState(addRefinedButton, addRefinedCanvasGroup, false);
                SetButtonState(addAllRefinedButton, addAllRefinedCanvasGroup, false);
                SetButtonState(takeRefinedButton, takeRefinedCanvasGroup, false);
                SetButtonState(startCraftButton, startCraftCanvasGroup, false);
                SetButtonState(finishCraftButton, finishCraftCanvasGroup, false);
                return;
            }

            var isRunning = state.craftStartTimeUnix > 0;
            var nextOreId = state.GetNextOreId();

            var canAdd = !isRunning &&
                         !string.IsNullOrWhiteSpace(nextOreId) &&
                         Manager.GetRefinedOreAmount(nextOreId) >= addAmount;

            // Проверяем, есть ли следующая нужная руда для Add All
            var canAddAll = !isRunning && !string.IsNullOrWhiteSpace(nextOreId) && 
                           Manager.GetRefinedOreAmount(nextOreId) > 0;

            var canTake = !isRunning && state.storedAmount >= takeAmount;

            var canStart = !isRunning &&
                           Manager.CanStartRefinedCraft();

            var canFinish = isRunning &&
                            Manager.IsRefinedCraftReady();

            SetButtonState(addRefinedButton, addRefinedCanvasGroup, canAdd);
            SetButtonState(addAllRefinedButton, addAllRefinedCanvasGroup, canAddAll);
            SetButtonState(takeRefinedButton, takeRefinedCanvasGroup, canTake);
            SetButtonState(startCraftButton, startCraftCanvasGroup, canStart);
            SetButtonState(finishCraftButton, finishCraftCanvasGroup, canFinish);
        }

        private void SetButtonState(Button button, CanvasGroup canvasGroup, bool enabledState)
        {
            if (button)
                button.interactable = enabledState;

            if (!canvasGroup) return;
            canvasGroup.alpha = enabledState ? enabledAlpha : disabledAlpha;
            canvasGroup.interactable = enabledState;
            canvasGroup.blocksRaycasts = enabledState;
        }

        private string FormatTime(int totalSeconds)
        {
            var timeSpan = TimeSpan.FromSeconds(totalSeconds);
            return $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }

        public void OnAddRefinedClicked()
        {
            var state = GetState();
            if (state == null || Manager == null) return;

            Manager.PutRefinedOreToCraft(addAmount);
            RefreshUI();
        }

        public void OnAddAllRefinedClicked()
        {
            var state = GetState();
            if (state == null || Manager == null) return;

            int totalAdded = 0;
            int oreTypesAdded = 0;

            // Добавляем руду пока есть что добавлять
            while (true)
            {
                // Получаем ID следующей необходимой руды
                var nextOreId = state.GetNextOreId();
                if (string.IsNullOrWhiteSpace(nextOreId))
                    break; // Крафт заполнен

                // Получаем количество, которое нужно добавить для этой руды
                int neededAmount = state.GetNextOreAmount();

                // Проверяем сколько этой руды есть у игрока
                int availableAmount = Manager.GetRefinedOreAmount(nextOreId);

                if (availableAmount >= neededAmount)
                {
                    // Есть достаточно - добавляем необходимое количество
                    for (int j = 0; j < neededAmount; j++)
                    {
                        if (!Manager.PutRefinedOreToCraft(1))
                            break;
                    }

                    totalAdded += neededAmount;
                    
                    // Если это был полный набор (neededAmount == amountPerOre), считаем новый тип
                    if (neededAmount == state.amountPerOre)
                        oreTypesAdded++;
                    
                    Debug.Log($"[EquipmentCraft] Добавлено {neededAmount} × {nextOreId}");
                }
                else if (availableAmount > 0)
                {
                    // Меньше чем нужно - добавляем сколько есть и прерываем
                    for (int j = 0; j < availableAmount; j++)
                    {
                        if (!Manager.PutRefinedOreToCraft(1))
                            break;
                    }

                    totalAdded += availableAmount;
                    Debug.Log($"[EquipmentCraft] Добавлено {availableAmount} × {nextOreId} (недостаточно - остановка)");
                    break; // Прерываем цикл
                }
                else
                {
                    // Этой руды нет вообще - прерываем
                    Debug.Log($"[EquipmentCraft] Руда {nextOreId} недоступна - остановка");
                    break;
                }
            }

            if (totalAdded > 0)
            {
                Debug.Log($"[EquipmentCraft] Всего добавлено: {totalAdded} руды ({oreTypesAdded} полных типов)");
            }

            // Полное обновление всех UI элементов
            RefreshUI();
        }

        public void OnTakeRefinedClicked()
        {
            if (Manager == null) return;

            Manager.TakeRefinedOreFromCraft(takeAmount);
            RefreshUI();
        }

        public void OnStartCraftClicked()
        {
            var state = GetState();
            if (state == null || Manager == null) return;

            // Логирование количества руды в крафте перед стартом
            int totalOres = state.storedAmount;
            int oreTypes = (totalOres / state.amountPerOre) + ((totalOres % state.amountPerOre > 0) ? 1 : 0);

            Debug.Log($"[EquipmentCraft] Запуск крафта с {totalOres} руды ({oreTypes} типов, по {state.amountPerOre} на тип)");

            // Детальный лог по каждому типу руды
            int currentAmount = totalOres;
            for (int i = 0; i < Manager.oreDataBase.allOres.Count && currentAmount > 0; i++)
            {
                var oreId = Manager.oreDataBase.allOres[i].oreId;
                int oreCount = UnityEngine.Mathf.Min(currentAmount, state.amountPerOre);
                Debug.Log($"  - {oreId}: {oreCount} шт.");
                currentAmount -= oreCount;
            }

            Manager.StartRefinedCraft(state.amountPerOre);
            RefreshUI();
        }

        public void OnFinishCraftClicked()
        {
            var state = GetState();
            if (state == null || Manager == null) return;
            if (!Manager.IsRefinedCraftReady()) return;

            var item = Manager.FinishEquipmentCraft();
            if (item != null)
            {
                Debug.Log($"[EquipmentCraftUI] Получен предмет: {item.itemName} | Редкость: {item.RarityName} | Статов: {item.stats.Count}");

                // Открываем панель детализации с полученным предметом
                if (detailPanel != null)
                {
                    Debug.Log($"[EquipmentCraftUI] Открываем detailPanel для предмета {item.itemName}");
                    detailPanel.Show(item);
                }
                else
                {
                    Debug.LogWarning("[EquipmentCraftUI] detailPanel не назначен в инспекторе!");
                }
            }
            else
            {
                Debug.LogWarning("[EquipmentCraftUI] FinishEquipmentCraft вернул null!");
            }

            onFinishCraftClicked?.Invoke();
            RefreshUI();
        }
    }
}
