using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class FeedbackUI : MonoBehaviour
{
    [FormerlySerializedAs("_messageTMP")]
    [SerializeField] private TMP_Text messageTMP;
    [SerializeField] private Image background;
    [SerializeField] private float showDuration = 2.5f;
    [SerializeField, Min(1)] private int maxQueuedMessages = 8;
    [SerializeField] private bool useFeedbackColors = true;
    [SerializeField] private Color informationColor = Color.white;
    [SerializeField] private Color successColor = new Color(0.55f, 1f, 0.55f);
    [SerializeField] private Color warningColor = new Color(1f, 0.85f, 0.35f);
    [SerializeField] private Color failureColor = new Color(1f, 0.45f, 0.45f);
    [SerializeField] private Color progressColor = new Color(0.45f, 0.85f, 1f);

    private GameplayFeedbackService _feedbackService;
    private readonly Queue<GameplayFeedbackMessage> _messageQueue = new Queue<GameplayFeedbackMessage>();
    private float _hideTime;
    private bool _isShowingMessage;
    private string _currentMessageText;

    private void Awake()
    {
        if (messageTMP == null)
            Debug.LogError("FeedbackUI requires message TMP reference.");

        HideMessage();
    }

    private void Update()
    {
        if (!_isShowingMessage || Time.time < _hideTime)
            return;

        ShowNextMessage();
    }

    public void Initialize(GameplayFeedbackService feedbackService)
    {
        if (_feedbackService != null)
            _feedbackService.OnFeedbackRaised -= EnqueueMessage;

        _feedbackService = feedbackService;

        if (_feedbackService != null)
            _feedbackService.OnFeedbackRaised += EnqueueMessage;
        else
            Debug.LogError("FeedbackUI requires GameplayFeedbackService.");
    }

    private void EnqueueMessage(GameplayFeedbackMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
            return;

        if (_isShowingMessage && _currentMessageText == message.Text)
        {
            _hideTime = Time.time + showDuration;
            return;
        }

        foreach (GameplayFeedbackMessage queuedMessage in _messageQueue)
        {
            if (queuedMessage.Text == message.Text)
                return;
        }

        while (_messageQueue.Count >= Mathf.Max(1, maxQueuedMessages))
            _messageQueue.Dequeue();

        _messageQueue.Enqueue(message);
        if (!_isShowingMessage)
            ShowNextMessage();
    }

    private void ShowNextMessage()
    {
        if (messageTMP == null)
            return;

        if (_messageQueue.Count == 0)
        {
            HideMessage();
            return;
        }

        GameplayFeedbackMessage message = _messageQueue.Dequeue();
        messageTMP.text = message.Text;
        if (useFeedbackColors)
            messageTMP.color = GetMessageColor(message.Kind);

        messageTMP.gameObject.SetActive(true);
        if (background != null)
            background.enabled = true;

        _isShowingMessage = true;
        _currentMessageText = message.Text;
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
        _currentMessageText = null;
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
        if (_feedbackService != null)
            _feedbackService.OnFeedbackRaised -= EnqueueMessage;
    }
}
