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
        [SerializeField] private Button takeRefinedButton;
        [SerializeField] private Button startCraftButton;
        [SerializeField] private Button finishCraftButton;

        [Header("Optional CanvasGroups for alpha")]
        [SerializeField] private CanvasGroup addRefinedCanvasGroup;
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

        private static GameDataManager Manager => GameDataManager.Instance;

        private void Awake()
        {
            if (addRefinedButton) addRefinedButton.onClick.AddListener(OnAddRefinedClicked);
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

            var canTake = !isRunning && state.storedAmount >= takeAmount;

            var canStart = !isRunning &&
                           Manager.CanStartRefinedCraft();

            var canFinish = isRunning &&
                            Manager.IsRefinedCraftReady();

            SetButtonState(addRefinedButton, addRefinedCanvasGroup, canAdd);
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
                Debug.Log($"[UI] Получен предмет: {item.itemName} | Редкость: {item.RarityName} | Статов: {item.stats.Count}");

            onFinishCraftClicked?.Invoke();
            RefreshUI();
        }
    }
}
