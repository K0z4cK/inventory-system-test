using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackUI : MonoBehaviour
{
    [SerializeField] private TMP_Text messageTMP;
    [SerializeField] private Image background;
    [SerializeField] private float showDuration = 2.5f;
    [SerializeField] private bool useFeedbackColors = true;
    [SerializeField] private Color informationColor = Color.white;
    [SerializeField] private Color successColor = new Color(0.55f, 1f, 0.55f);
    [SerializeField] private Color warningColor = new Color(1f, 0.85f, 0.35f);
    [SerializeField] private Color failureColor = new Color(1f, 0.45f, 0.45f);
    [SerializeField] private Color progressColor = new Color(0.45f, 0.85f, 1f);

    private IGameplayFeedbackSource _feedbackSource;
    private float _hideTime;
    private bool _isShowingMessage;

    private void Awake()
    {
        if (messageTMP == null)
            Debug.LogError("FeedbackUI requires message TMP reference.");

        HideMessage();
    }

    private void OnEnable()
    {
        Bind(ProjectContext.Get<IGameplayFeedbackSource>());
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void Update()
    {
        if (!_isShowingMessage || Time.time < _hideTime)
            return;

        HideMessage();
    }

    private void Bind(IGameplayFeedbackSource feedbackSource)
    {
        Unbind();
        _feedbackSource = feedbackSource;

        if (_feedbackSource != null)
            _feedbackSource.OnFeedbackRaised += ShowMessage;
        else
            Debug.LogError("FeedbackUI requires IGameplayFeedbackSource.");
    }

    private void Unbind()
    {
        if (_feedbackSource != null)
            _feedbackSource.OnFeedbackRaised -= ShowMessage;

        _feedbackSource = null;
    }

    private void ShowMessage(GameplayFeedbackMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
            return;

        if (messageTMP == null)
            return;

        messageTMP.text = message.Text;
        if (useFeedbackColors)
            messageTMP.color = GetMessageColor(message.Kind);

        messageTMP.gameObject.SetActive(true);
        if (background != null)
            background.enabled = true;

        _isShowingMessage = true;
        _hideTime = Time.time + showDuration;
    }

    private void HideMessage()
    {
        if (messageTMP == null)
            return;

        messageTMP.text = string.Empty;
        messageTMP.gameObject.SetActive(false);
        if (background != null)
            background.enabled = false;

        _isShowingMessage = false;
    }

    private Color GetMessageColor(GameplayFeedbackKind kind)
    {
        switch (kind)
        {
            case GameplayFeedbackKind.Success:
                return successColor;
            case GameplayFeedbackKind.Warning:
                return warningColor;
            case GameplayFeedbackKind.Failure:
                return failureColor;
            case GameplayFeedbackKind.Progress:
                return progressColor;
            default:
                return informationColor;
        }
    }

    private void OnDestroy()
    {
        Unbind();
    }
}
