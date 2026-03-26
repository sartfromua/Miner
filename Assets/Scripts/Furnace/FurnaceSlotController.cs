using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Furnace
{
    public class FurnaceSlotController : MonoBehaviour
    {
        [Header("Уникальный ID слота (должен быть разным у разных префабов)")]
        [SerializeField] public string slotId = "Furnace";

        [Header("Настройки крафта (меняй в инспекторе)")]
        [SerializeField] public string inputOreId = "coal_ore";
        [SerializeField] public int inputAmount = 1;
        [SerializeField] public string outputOreId = "coal_ore";
        [SerializeField] public int outputAmount = 1;
        [SerializeField] public float craftTimeSeconds = 30f;

        [Header("UI ссылки")]
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] public Image leftIcon;
        [SerializeField] public Image rightIcon;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI timerText;

        private void Awake()
        {
            leftButton.onClick.AddListener(OnLeftClick);
            rightButton.onClick.AddListener(OnRightClick);
        }

        private void Start()
        {
            // Фиксируем иконки (можно менять в инспекторе)
            GameDataManager.Instance.OnDataUpdated += RefreshUI;
            RefreshUI();
        }

        private void OnDestroy()
        {
            if (GameDataManager.Instance != null)
                GameDataManager.Instance.OnDataUpdated -= RefreshUI;
        }

        private void Update()
        {
            UpdateAutoSmelt(); // проверяем автоплавку
            RefreshUI(); // обновляем прогресс каждый кадр
        }

        /// <summary>
        /// Обработка автоматической плавки
        /// </summary>
        private void UpdateAutoSmelt()
        {
            if (GameDataManager.Instance == null)
                return;

            // Проверяем, включена ли автоплавка для этой руды
            if (!GameDataManager.Instance.IsAutoSmeltEnabled(inputOreId))
                return;

            var slot = GameDataManager.Instance.GetFurnaceSlot(slotId);
            bool isCrafting = slot.startTimeUnix > 0;
            bool isReady = GameDataManager.Instance.IsFurnaceCraftReady(slotId, craftTimeSeconds);

            // Если крафт готов - забираем результат
            if (isReady)
            {
                if (GameDataManager.Instance.ClaimFurnaceCraft(slotId, outputOreId, outputAmount))
                {
                    Debug.Log($"[AutoSmelt] Автоматически получено {outputAmount} × {outputOreId}");
                }
            }

            // Если крафт не идёт - пытаемся запустить
            if (!isCrafting)
            {
                if (GameDataManager.Instance.StartFurnaceCraft(slotId, inputOreId, inputAmount, craftTimeSeconds))
                {
                    Debug.Log($"[AutoSmelt] Автоматически запущена плавка {slotId}");
                }
            }
        }

        private void OnLeftClick()
        {
            if (GameDataManager.Instance.StartFurnaceCraft(slotId, inputOreId, inputAmount, craftTimeSeconds))
            {
                Debug.Log($"Плавка {slotId} запущена!");
            }
        }

        private void OnRightClick()
        {
            if (GameDataManager.Instance.ClaimFurnaceCraft(slotId, outputOreId, outputAmount))
            {
                Debug.Log($"Получено {outputAmount} × {outputOreId} refined!");
            }
        }
        
        // FurnaceSlotController.cs
        public void Setup(RefinedData recipe, OreData oreData)
        {
            slotId           = "Smelt_" + recipe.oreId;
            inputOreId       = recipe.oreId;
            inputAmount      = 1;
            outputAmount      = 1;
            outputOreId      = oreData.oreId;
            craftTimeSeconds = recipe.timeToMelt;
            slotId        = "Smelt_" + recipe.oreId;     // уникальный id слота
            inputOreId    = recipe.oreId;                // та же строка что и в RefinedData
            outputOreId   = recipe.oreId;                // или "refined_" + refined.oreId
            craftTimeSeconds = recipe.timeToMelt;

                
            // Опционально: иконки (если в префабе есть публичные Image)
            if (oreData) leftIcon.sprite  = oreData.icon;
            rightIcon.sprite = recipe.icon;

            RefreshUI();
        }

        public void RefreshUI()
        {
            var progress = GameDataManager.Instance.GetFurnaceCraftProgress(slotId, craftTimeSeconds);
            var isReady = GameDataManager.Instance.IsFurnaceCraftReady(slotId, craftTimeSeconds);
            var isCrafting = GameDataManager.Instance.GetFurnaceSlot(slotId).startTimeUnix > 0;
            var isAutoSmeltEnabled = GameDataManager.Instance.IsAutoSmeltEnabled(inputOreId);

            progressBar.value = progress;

            // Отключаем кнопки, если включена автоплавка
            leftButton.interactable = !isCrafting && !isAutoSmeltEnabled;
            rightButton.interactable = isReady && !isAutoSmeltEnabled;

            if (isCrafting)
            {
                var remaining = Mathf.Max(0, craftTimeSeconds - (progress * craftTimeSeconds));
                var baseText = remaining > 0
                    ? $"{Mathf.FloorToInt(remaining / 60)}:{Mathf.FloorToInt(remaining % 60):00}"
                    : "ГОТОВО!";

                timerText.text = isAutoSmeltEnabled
                    ? $"<color=yellow>AUTO</color> {baseText}"
                    : baseText;
            }
            else
            {
                timerText.text = isAutoSmeltEnabled ? "<color=yellow>AUTO</color>" : "";
                progressBar.value = 0;
            }
        }
    }
}