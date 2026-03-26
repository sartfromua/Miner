using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OreObject : MonoBehaviour, IPointerDownHandler
{
    [Header("Visuals")]
    public Image mainRenderer;
    public Image crackRenderer;
    public Sprite[] crackSprites;

    private OreData _data;
    private float _maxHp;
    private float _currentHp;

    // Действие, которое мы вызовем, когда руда сломается
    private Action _onOreBrokenCallback;

    // Таймер для автоматического урона
    private float _autoAttackTimer = 0f;
    private const float AUTO_ATTACK_INTERVAL = 1f; // Раз в секунду

    // Убрали OnEnable, так как руда сама себя не спавнит.
    // Этим управляет OreSpawnService.
    
    // Добавили второй аргумент: onBroken
    public void Setup(OreData data, Action onBroken)
    {
        Debug.Log("Setting up Ore Object " + data.oreId);
        _data = data;
        _onOreBrokenCallback = onBroken; // Запоминаем, кого уведомить

        // 1. Получаем прочность
        // _maxHp = data.durability;
        _maxHp = GameDataManager.GetOreDurability(data.oreId);
        _currentHp = _maxHp;

        // 2. Сбрасываем визуал
        mainRenderer.sprite = data.icon;

        if(crackRenderer) crackRenderer.sprite = crackSprites[0];

        // 3. Сбрасываем таймер автоатаки
        _autoAttackTimer = 0f;

        // Включаем объект, так как в конце жизни мы его выключаем
        gameObject.SetActive(true);
    }

    private void Update()
    {
        // Автоматический урон раз в секунду
        if (GameDataManager.Instance != null)
        {
            float autoPickaxeDamage = GameDataManager.Instance.GetAutoPickaxeDamage();

            if (autoPickaxeDamage > 0)
            {
                _autoAttackTimer += Time.deltaTime;

                if (_autoAttackTimer >= AUTO_ATTACK_INTERVAL)
                {
                    _autoAttackTimer = 0f;
                    TakeDamage(autoPickaxeDamage);
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Click on Ore!"); // Теперь это точно сработает
        
        if (GameDataManager.Instance != null)
        {
            TakeDamage(GameDataManager.Instance.GetDamage());
        }
    }

    private void TakeDamage(float damage)
    {
        _currentHp -= damage;

        UpdateCracks();

        if (_currentHp <= 0)
        {
            OnBroken();
        }
    }

    private void UpdateCracks()
    {
        if (crackRenderer == null || crackSprites.Length == 0) return;

        var hpPercent = _currentHp / _maxHp;
        var damagePercent = 1f - hpPercent; 

        var spriteIndex = Mathf.FloorToInt(damagePercent * crackSprites.Length);
        spriteIndex = Mathf.Clamp(spriteIndex, 0, crackSprites.Length - 1);

        if (_currentHp < _maxHp) // Показываем трещины только если есть урон
        {
            crackRenderer.sprite = crackSprites[spriteIndex];
        }
        else
        {
            crackRenderer.sprite = crackSprites[0];
        }
    }

    private void OnBroken()
    {
        Debug.Log($"Добыта руда: {_data.oreId}");
        
        // Тут можно добавить руду в инвентарь
        GameDataManager.Instance.AddOre(_data.oreId, 1);

        // Выключаем объект
        gameObject.SetActive(false);

        // ВАЖНО: Сообщаем Спавнеру, что мы сломались
        _onOreBrokenCallback?.Invoke();
    }
}