using System;
using UnityEngine;

public class AttackableObject : MonoBehaviour, IAttackable, IDamageable, IAttackRangeAware
{
    [SerializeField] private string displayName;
    [SerializeField, Min(1)] private int maxHealth = 1;
    [SerializeField] private int attackPriority;
    [SerializeField] private bool destroyWhenDefeated = true;
    [SerializeField] private LootDropper lootDropper;

    private bool _isDefeated;
    private int _currentHealth;
    private bool _isInAttackRange;

    public event Action<int, int> OnHealthChanged;
    public event Action<bool> OnAttackRangeChanged;

    public bool CanBeAttacked => !_isDefeated && _currentHealth > 0;
    public int AttackPriority => attackPriority;
    public int CurrentHealth => Mathf.Max(0, _currentHealth);
    public int MaxHealth => Mathf.Max(1, maxHealth);
    public bool IsInAttackRange => _isInAttackRange;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        _currentHealth = maxHealth;

        if (lootDropper == null)
            lootDropper = GetComponent<LootDropper>();
    }

    public virtual AttackResult ReceiveAttack(Character attacker, int damage)
    {
        if (!CanBeAttacked)
            return default;

        int previousHealth = _currentHealth;
        _currentHealth = Mathf.Max(0, _currentHealth - Mathf.Max(1, damage));
        int damageDealt = previousHealth - _currentHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (_currentHealth > 0)
            return new AttackResult(DisplayName, damageDealt, CurrentHealth, MaxHealth, false, 0);

        int droppedLootCount = Defeat(attacker);
        return new AttackResult(DisplayName, damageDealt, CurrentHealth, MaxHealth, true, droppedLootCount);
    }

    public void SetInAttackRange(bool isInRange)
    {
        if (_isInAttackRange == isInRange)
            return;

        _isInAttackRange = isInRange;
        OnAttackRangeChanged?.Invoke(_isInAttackRange);
    }

    protected virtual int Defeat(Character attacker)
    {
        _isDefeated = true;
        SetInAttackRange(false);
        int droppedLootCount = lootDropper != null ? lootDropper.DropLoot() : 0;

        if (destroyWhenDefeated)
            Destroy(gameObject);

        return droppedLootCount;
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
    }
}
