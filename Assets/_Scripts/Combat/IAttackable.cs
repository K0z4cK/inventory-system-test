public readonly struct AttackResult
{
    public bool Succeeded { get; }
    public string TargetName { get; }
    public int DamageDealt { get; }
    public int RemainingHealth { get; }
    public int MaxHealth { get; }
    public bool Defeated { get; }
    public int DroppedLootCount { get; }

    public AttackResult(
        string targetName,
        int damageDealt,
        int remainingHealth,
        int maxHealth,
        bool defeated,
        int droppedLootCount)
    {
        Succeeded = true;
        TargetName = targetName;
        DamageDealt = damageDealt;
        RemainingHealth = remainingHealth;
        MaxHealth = maxHealth;
        Defeated = defeated;
        DroppedLootCount = droppedLootCount;
    }
}

public interface IAttackable
{
    bool CanBeAttacked { get; }
    int AttackPriority { get; }
    AttackResult ReceiveAttack(Character attacker, int damage);
}
