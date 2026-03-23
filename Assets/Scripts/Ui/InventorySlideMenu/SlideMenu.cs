using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Ui.InventorySlideMenu
{
    public class SlideMenu : MonoBehaviour
    {
        [Header("Настройки")]
        public RectTransform menuPanel;
        public Button openButton;
        public Button closeButton;
        [Range(0.1f, 2f)] public float animationDuration = 0.5f;

        [Header("Направление выдвижения")]
        [Tooltip("true = выезжает слева (вправо), false = выезжает справа (влево)")]
        public bool slideFromLeft = true;

        private Vector2 hiddenPos;
        private Vector2 shownPos;
        private Coroutine currentAnimation;

        private void Awake()
        {
            if (menuPanel == null)
            {
                Debug.LogError("menuPanel не назначен!", this);
                return;
            }

            // Запоминаем стартовое положение — оно будет скрытым
            hiddenPos = menuPanel.anchoredPosition;

            // Вычисляем показанное положение
            var directionMultiplier = slideFromLeft ? 1f : -1f;
            var panelWidth = menuPanel.rect.width;

            // Предполагаем, что в показанном состоянии панель должна быть полностью видна (X = 0 или около того)
            // Если у тебя якоря не по центру — может потребоваться корректировка
            shownPos = hiddenPos + new Vector2(panelWidth * directionMultiplier, 0);
        }

        private void Start()
        {
            // На старте прячем меню
            menuPanel.anchoredPosition = hiddenPos;

            if (openButton)    openButton.onClick.AddListener(OpenMenu);
            if (closeButton)   closeButton.onClick.AddListener(CloseMenu);

            // Начальное состояние кнопок
            if (openButton)    openButton.gameObject.SetActive(true);
            if (closeButton)   closeButton.gameObject.SetActive(false);
        }

        public void OpenMenu()
        {
            if (openButton)    openButton.gameObject.SetActive(false);
            if (closeButton)   closeButton.gameObject.SetActive(true);

            StopCurrentAnimation();
            currentAnimation = StartCoroutine(AnimateTo(shownPos));
        }

        public void CloseMenu()
        {
            StopCurrentAnimation();
            currentAnimation = StartCoroutine(AnimateTo(hiddenPos, () =>
            {
                if (openButton)    openButton.gameObject.SetActive(true);
                if (closeButton)   closeButton.gameObject.SetActive(false);
            }));
        }

        private void StopCurrentAnimation()
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }
        }

        private IEnumerator AnimateTo(Vector2 target, Action onComplete = null)
        {
            var start = menuPanel.anchoredPosition;
            var elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / animationDuration;
                t = Mathf.Clamp01(t); // на всякий случай

                menuPanel.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }

            menuPanel.anchoredPosition = target;
            onComplete?.Invoke();
        }

        private void OnDestroy()
        {
            StopCurrentAnimation();
        }
    }
}