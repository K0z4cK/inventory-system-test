using UnityEngine;

public class InteractableHighlightPresenter : MonoBehaviour
{
    [SerializeField] private GameObject highlightPrefab;
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 0.05f, 0f);
    [SerializeField] private Vector3 groundRotationEuler = new Vector3(90f, 0f, 0f);
    [SerializeField] private bool scaleToTargetBounds = true;
    [SerializeField] private float sizePadding = 1.2f;
    [SerializeField] private Vector2 fallbackWorldSize = Vector2.one;
    [SerializeField] private Vector2 minWorldSize = new Vector2(0.35f, 0.35f);
    [SerializeField] private Vector2 maxWorldSize = new Vector2(4f, 4f);

    private GameObject _highlightInstance;
    private Vector3 _baseScale = Vector3.one;
    private Transform _target;
    private InteractableHighlightAnchor _targetAnchor;

    private void Awake()
    {
        EnsureHighlightInstance();
        Hide();
    }

    private void LateUpdate()
    {
        if (_highlightInstance == null || !_highlightInstance.activeSelf || _target == null)
            return;

        ApplyTransform();
    }

    public void Show(Transform target)
    {
        if (target == null || !EnsureHighlightInstance())
            return;

        _target = target;
        _targetAnchor = target.GetComponentInChildren<InteractableHighlightAnchor>(true);
        _highlightInstance.SetActive(true);
        ApplyTransform();
    }

    public void Hide()
    {
        _target = null;
        _targetAnchor = null;

        if (_highlightInstance != null)
            _highlightInstance.SetActive(false);
    }

    private bool EnsureHighlightInstance()
    {
        if (_highlightInstance != null)
            return true;

        if (highlightPrefab == null)
            return false;

        _highlightInstance = Instantiate(highlightPrefab);
        _baseScale = _highlightInstance.transform.localScale;
        _highlightInstance.SetActive(false);
        return true;
    }

    private void ApplyTransform()
    {
        Vector3 targetPosition = _targetAnchor != null ? _targetAnchor.Position : _target.position;
        _highlightInstance.transform.position = targetPosition + positionOffset;
        _highlightInstance.transform.rotation = Quaternion.Euler(groundRotationEuler);

        if (scaleToTargetBounds)
            ApplyScale();
    }

    private void ApplyScale()
    {
        Vector2 targetSize = GetTargetGroundSize();
        _highlightInstance.transform.localScale = new Vector3(
            _baseScale.x * targetSize.x,
            _baseScale.y * targetSize.y,
            _baseScale.z);
    }

    private Vector2 GetTargetGroundSize()
    {
        Bounds bounds;
        bool hasBounds = TryGetTargetBounds(out bounds);
        Vector2 size = hasBounds
            ? new Vector2(bounds.size.x, bounds.size.z)
            : fallbackWorldSize;

        size *= Mathf.Max(0f, sizePadding);
        size.x = Mathf.Clamp(size.x, minWorldSize.x, maxWorldSize.x);
        size.y = Mathf.Clamp(size.y, minWorldSize.y, maxWorldSize.y);

        return size;
    }

    private bool TryGetTargetBounds(out Bounds bounds)
    {
        bounds = default;
        if (_target == null)
            return false;

        bool hasBounds = false;
        foreach (Renderer renderer in _target.GetComponentsInChildren<Renderer>())
        {
            if (!renderer.enabled)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
                continue;
            }

            bounds.Encapsulate(renderer.bounds);
        }

        if (hasBounds)
            return true;

        foreach (Collider collider in _target.GetComponentsInChildren<Collider>())
        {
            if (!collider.enabled)
                continue;

            if (!hasBounds)
            {
                bounds = collider.bounds;
                hasBounds = true;
                continue;
            }

            bounds.Encapsulate(collider.bounds);
        }

        return hasBounds;
    }
}
