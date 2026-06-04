using System;

public interface IDamageable
{
    event Action<int, int> OnHealthChanged;

    int CurrentHealth { get; }
    int MaxHealth { get; }
}
