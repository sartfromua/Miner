using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Texts")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI blocksText;
    public TextMeshProUGUI scoreText;
    
    [Header("Game Panels")]
    public List<GameObject> panels;
    
    [Header("Game Panels Buttons")]
    public List<Button> panelButtons;
    
    private void Start()
    {
        // 1. Подписка на обновление данных
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnDataUpdated += RefreshUI;
            RefreshUI(); // Обновляем сразу при старте
        }

        // 2. Настройка кнопок
        // Важно: Списки panels и panelButtons должны совпадать по размеру и порядку в Инспекторе!
        for (var i = 0; i < panelButtons.Count; i++)
        {
            var index = i; // ОЧЕНЬ ВАЖНО: создаем копию переменной для замыкания (lambda capture)
            
            panelButtons[i].onClick.AddListener(() => 
            {
                OpenPanel(index); 
            });
        }
    }
    
    private void OnDestroy()
    {
        // Не забываем отписываться
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnDataUpdated -= RefreshUI;
        }
    }

    // Метод принимает номер панели, которую надо открыть
    private void OpenPanel(int panelIndex)
    {
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
        for (var i = 0; i < panels.Count; i++)
        {
            if (i == panelIndex)
            {
                panels[i].SetActive(true);
                return;
            }
        }
    }

    private void RefreshUI() {
        moneyText.text = $"{GameDataManager.Instance.GetMoney()}";
        blocksText.text = $"{GameDataManager.Instance.GetBrokenBlocks()}";
        scoreText.text = $"{GameDataManager.Instance.GetScore()}";
    }
}