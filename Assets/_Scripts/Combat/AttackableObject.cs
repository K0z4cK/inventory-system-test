using System;
using UnityEngine;
using UnityEngine.Serialization;

public class AttackableObject : MonoBehaviour, IAttackable, IDamageable, IAttackRangeAware
{
    [FormerlySerializedAs("health")]
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

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        _currentHealth = maxHealth;

        if (lootDropper == null)
            lootDropper = GetComponent<LootDropper>();
    }

    public virtual void ReceiveAttack(Character attacker, int damage)
    {
        if (!CanBeAttacked)
            return;

        _currentHealth = Mathf.Max(0, _currentHealth - Mathf.Max(1, damage));
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (_currentHealth > 0)
            return;

        Defeat(attacker);
    }

    public void SetInAttackRange(bool isInRange)
    {
        if (_isInAttackRange == isInRange)
            return;

        _isInAttackRange = isInRange;
        OnAttackRangeChanged?.Invoke(_isInAttackRange);
    }

    protected virtual void Defeat(Character attacker)
    {
        _isDefeated = true;
        SetInAttackRange(false);
        lootDropper?.DropLoot();

        if (destroyWhenDefeated)
            Destroy(gameObject);
    }

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
    }
}
