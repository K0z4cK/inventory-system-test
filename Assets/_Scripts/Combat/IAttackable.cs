public interface IAttackable
{
    bool CanBeAttacked { get; }
    int AttackPriority { get; }
    void ReceiveAttack(Character attacker, int damage);
}
