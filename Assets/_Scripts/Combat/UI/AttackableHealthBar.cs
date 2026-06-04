using UnityEngine;
using UnityEngine.UI;

public class AttackableHealthBar : MonoBehaviour
{
    [SerializeField] private AttackableObject attackable;
    [SerializeField] private GameObject healthBarRoot;
    [SerializeField] private Image fillImage;
    [SerializeField] private bool faceCamera = true;

    private Camera _camera;
    private bool _isSubscribed;

    private void Awake()
    {
        if (attackable == null)
            attackable = GetComponentInParent<AttackableObject>();

        if (healthBarRoot == null)
            healthBarRoot = gameObject;

        _camera = Camera.main;
        Subscribe();
        Refresh();
    }

    private void LateUpdate()
    {
        if (!faceCamera || healthBarRoot == null || !healthBarRoot.activeSelf)
            return;

        if (_camera == null)
            _camera = Camera.main;

        if (_camera != null)
            healthBarRoot.transform.rotation = Quaternion.LookRotation(healthBarRoot.transform.position - _camera.transform.position);
    }

    private void Subscribe()
    {
        if (_isSubscribed || attackable == null)
            return;

        attackable.OnHealthChanged += HandleHealthChanged;
        attackable.OnAttackRangeChanged += HandleAttackRangeChanged;
        _isSubscribed = true;
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        Refresh();
    }

    private void HandleAttackRangeChanged(bool isInRange)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (attackable == null || healthBarRoot == null)
            return;

        if (fillImage != null)
            fillImage.fillAmount = (float)attackable.CurrentHealth / attackable.MaxHealth;

        bool shouldShow = attackable.IsInAttackRange
                          && attackable.CurrentHealth > 0
                          && attackable.CurrentHealth < attackable.MaxHealth;
        healthBarRoot.SetActive(shouldShow);
    }

    private void OnDestroy()
    {
        if (!_isSubscribed || attackable == null)
            return;

        attackable.OnHealthChanged -= HandleHealthChanged;
        attackable.OnAttackRangeChanged -= HandleAttackRangeChanged;
    }
}
