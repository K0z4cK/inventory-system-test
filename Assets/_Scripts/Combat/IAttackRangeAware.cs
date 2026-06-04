using System;

public interface IAttackRangeAware
{
    event Action<bool> OnAttackRangeChanged;

    bool IsInAttackRange { get; }
    void SetInAttackRange(bool isInRange);
}
