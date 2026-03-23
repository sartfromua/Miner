using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    /// <summary>
    /// Автоматически подстраивает размер ячеек Grid Layout Group
    /// под размер контейнера с учётом заданного количества колонок и рядов.
    /// </summary>
    [RequireComponent(typeof(GridLayoutGroup))]
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class AutoGridLayoutSizer : MonoBehaviour
    {
        [Header("Grid Settings")]
        [Tooltip("Количество колонок в гриде")]
        public int columns = 8;

        [Tooltip("Количество рядов в гриде")]
        public int rows = 4;

        [Header("Spacing Settings")]
        [Tooltip("Отступ между ячейками по горизонтали")]
        public float spacingX = 10f;

        [Tooltip("Отступ между ячейками по вертикали")]
        public float spacingY = 10f;

        [Tooltip("Отступы от краёв контейнера (padding)")]
        public int paddingLeft = 10;
        public int paddingRight = 10;
        public int paddingTop = 10;
        public int paddingBottom = 10;

        [Header("Aspect Ratio")]
        [Tooltip("Сохранять квадратные ячейки (1:1)")]
        public bool keepSquare = true;

        [Tooltip("Если не квадратные, то соотношение ширина/высота (например 1.5 = шире чем выше)")]
        public float aspectRatio = 1f;

        private GridLayoutGroup gridLayout;
        private RectTransform rectTransform;

        private void Awake()
        {
            gridLayout = GetComponent<GridLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            UpdateCellSize();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateCellSize();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gridLayout == null)
                gridLayout = GetComponent<GridLayoutGroup>();

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            UpdateCellSize();
        }
#endif

        /// <summary>
        /// Пересчитывает и применяет размер ячеек на основе размера контейнера.
        /// </summary>
        public void UpdateCellSize()
        {
            if (gridLayout == null || rectTransform == null)
                return;

            if (columns <= 0 || rows <= 0)
            {
                Debug.LogWarning("[AutoGridLayoutSizer] Количество колонок и рядов должно быть > 0");
                return;
            }

            // Применяем padding к Grid Layout
            gridLayout.padding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);

            // Получаем доступную ширину и высоту контейнера (без padding)
            float availableWidth = rectTransform.rect.width - paddingLeft - paddingRight;
            float availableHeight = rectTransform.rect.height - paddingTop - paddingBottom;

            // Вычитаем пространство для отступов между ячейками
            float totalSpacingX = spacingX * (columns - 1);
            float totalSpacingY = spacingY * (rows - 1);

            availableWidth -= totalSpacingX;
            availableHeight -= totalSpacingY;

            // Рассчитываем размер одной ячейки
            float cellWidth = availableWidth / columns;
            float cellHeight = availableHeight / rows;

            // Если нужны квадратные ячейки, берём минимальное значение
            if (keepSquare)
            {
                float cellSize = Mathf.Min(cellWidth, cellHeight);
                gridLayout.cellSize = new Vector2(cellSize, cellSize);
            }
            else
            {
                // Применяем заданное соотношение сторон
                if (aspectRatio > 0)
                {
                    // Проверяем, какое ограничение сильнее
                    float widthBasedHeight = cellWidth / aspectRatio;
                    float heightBasedWidth = cellHeight * aspectRatio;

                    if (widthBasedHeight <= cellHeight)
                    {
                        // Ограничены шириной
                        gridLayout.cellSize = new Vector2(cellWidth, widthBasedHeight);
                    }
                    else
                    {
                        // Ограничены высотой
                        gridLayout.cellSize = new Vector2(heightBasedWidth, cellHeight);
                    }
                }
                else
                {
                    gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
                }
            }

            // Применяем отступы между ячейками
            gridLayout.spacing = new Vector2(spacingX, spacingY);

            // Устанавливаем constraint на Fixed Column Count
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = columns;
        }
    }
}
