using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlideMenu : MonoBehaviour
{
    [Header("Настройки")]
    public RectTransform menuPanel;
    public Button openButton;
    public Button closeButton;
    public float animationDuration = 0.5f;

    private Vector2 _hiddenPos;
    private Vector2 _shownPos;
    private Coroutine _currentAnimation;

    void Start()
    {
        _hiddenPos = menuPanel.anchoredPosition;
        // Предполагаем, что открытое состояние - это X=0
        _shownPos = new Vector2(0, _hiddenPos.y);

        openButton.onClick.AddListener(OpenMenu);
        if (closeButton != null) closeButton.onClick.AddListener(CloseMenu);

        // --- ИСПРАВЛЕНИЕ 1: Стартовое состояние ---
        // При старте игры убеждаемся, что кнопка Open включена, а Close - выключена
        openButton.gameObject.SetActive(true);
        if (closeButton != null) 
            closeButton.gameObject.SetActive(false); // Выключаем Close, чтобы не мешала
    }

    public void OpenMenu()
    {
        // Выключаем кнопку открытия
        openButton.gameObject.SetActive(false);
        
        // --- ИСПРАВЛЕНИЕ 2: Включаем кнопку закрытия ---
        // Меню поехало - теперь кнопка Close нужна
        if (closeButton != null) 
            closeButton.gameObject.SetActive(true);

        if (_currentAnimation != null) StopCoroutine(_currentAnimation);
        _currentAnimation = StartCoroutine(AnimateMenu(_shownPos));
    }

    public void CloseMenu()
    {

        if (_currentAnimation != null) StopCoroutine(_currentAnimation);
        StartCoroutine(AnimateMenu(_hiddenPos, () => 
        {
            // Callback: Меню уехало -> возвращаем кнопку Open
            openButton.gameObject.SetActive(true);
            if (closeButton) 
                closeButton.gameObject.SetActive(false);
        }));
    }

    IEnumerator AnimateMenu(Vector2 targetPos, System.Action onComplete = null)
    {
        var startPos = menuPanel.anchoredPosition;
        float time = 0;

        while (time < animationDuration)
        {
            menuPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, time / animationDuration);
            time += Time.deltaTime;
            yield return null;
        }
        menuPanel.anchoredPosition = targetPos;
        onComplete?.Invoke();
    }
}