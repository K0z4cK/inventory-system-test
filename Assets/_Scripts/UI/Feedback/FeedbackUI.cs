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

    private GameplayFeedbackService _feedbackService;
    private float _hideTime;

    private void Awake()
    {
        if (messageTMP == null)
            Debug.LogError("FeedbackUI requires message TMP reference.");

        HideMessage();
    }

    private void Update()
    {
        if (messageTMP == null || !messageTMP.gameObject.activeSelf || Time.time < _hideTime)
            return;

        HideMessage();
    }

    public void Initialize(GameplayFeedbackService feedbackService)
    {
        if (_feedbackService != null)
            _feedbackService.OnMessageRaised -= ShowMessage;

        _feedbackService = feedbackService;

        if (_feedbackService != null)
            _feedbackService.OnMessageRaised += ShowMessage;
        else
            Debug.LogError("FeedbackUI requires GameplayFeedbackService.");
    }

    private void ShowMessage(string message)
    {
        if (messageTMP == null)
            return;

        messageTMP.text = message;
        messageTMP.gameObject.SetActive(true);
        if (background != null)
            background.enabled = true;

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
    }

    private void OnDestroy()
    {
        if (_feedbackService != null)
            _feedbackService.OnMessageRaised -= ShowMessage;
    }
}
