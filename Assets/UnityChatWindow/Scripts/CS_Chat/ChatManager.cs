using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ChatWindowManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform closedChat;         // ͼ�� RectTransform�����Ͻ� anchor��
    public RectTransform openedChat;         // չ������ RectTransform������ anchor��
    public CanvasGroup openedChatGroup;      // ���ڽ���͸��
    public Image closedChatImage;            // ͼ�� Image ���

    [Header("��������")]
    public float animationDuration = 0.4f;

    [Header("������˸����")]
    public bool isWarning = false;
    public Color warningColor = Color.red;
    public Color defaultColor = Color.white;
    public float flashInterval = 0.5f;

    private Tween warningTween;

    private Vector2 savedOpenedPos;
    private Vector3 savedOpenedScale;
    private Vector2 closedSize;

    private RectTransform canvasRect;

    private void Start()
    {
        canvasRect = openedChat.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        closedSize = closedChat.sizeDelta;
        savedOpenedPos = openedChat.anchoredPosition;
        savedOpenedScale = openedChat.localScale;
        openedChatGroup.alpha = 0f;
        openedChat.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isWarning)
            StartWarning();
        else
            StopWarning();
    }

    public void OnClickClosedChat()
    {
        Expand();
    }

    public void OnClickClose()
    {
        Collapse();
    }

    public void Expand()
    {
        // ��ȡ closedChat ���������ĵ�
        Vector3 worldPos = closedChat.TransformPoint(closedChat.rect.center);

        // �������ת��Ϊ openedChat ���ڸ����ı������꣨ͨ���� Canvas��
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, worldPos, null, out localPoint);

        // ���ò����� openedChat ��ʼ״̬
        openedChat.gameObject.SetActive(true);
        openedChat.anchoredPosition = localPoint;

        // ���ű���
        Vector2 openedSize = openedChat.sizeDelta;
        float scaleX = closedSize.x / openedSize.x;
        float scaleY = closedSize.y / openedSize.y;
        openedChat.localScale = new Vector3(scaleX, scaleY, 1f);
        openedChatGroup.alpha = 0f;

        // ����չ������
        openedChat.DOAnchorPos(savedOpenedPos, animationDuration);
        openedChat.DOScale(Vector3.one, animationDuration);
        openedChatGroup.DOFade(1f, animationDuration);

        closedChat.DOScale(Vector3.zero, animationDuration).OnComplete(() =>
        {
            closedChat.gameObject.SetActive(false);
            closedChat.localScale = Vector3.one;
        });

    }

    public void Collapse()
    {
        // ��¼ openedChat ��ǰê��λ��
        savedOpenedPos = openedChat.anchoredPosition;
        savedOpenedScale = openedChat.localScale;

        // ��ȡ closedChat ���������ĵ�
        Vector3 worldPos = closedChat.TransformPoint(closedChat.rect.center);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, worldPos, null, out localPoint);

        // ���ű���
        Vector2 openedSize = openedChat.sizeDelta;
        float scaleX = closedSize.x / openedSize.x;
        float scaleY = closedSize.y / openedSize.y;

        // �������𶯻�
        openedChat.DOAnchorPos(localPoint, animationDuration);
        openedChat.DOScale(new Vector3(scaleX, scaleY, 1f), animationDuration);
        openedChatGroup.DOFade(0f, animationDuration).OnComplete(() =>
        {
            openedChat.gameObject.SetActive(false);
        });

        // ��ʾ�������ָ� closedChat
        closedChat.gameObject.SetActive(true);
        closedChat.localScale = Vector3.zero;
        closedChat.DOScale(Vector3.one, animationDuration);
    }

    void StartWarning()
    {
        if (warningTween != null || !closedChat.gameObject.activeInHierarchy)
            return;

        warningTween = closedChatImage.DOColor(warningColor, flashInterval)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear);
    }

    void StopWarning()
    {
        if (warningTween != null)
        {
            warningTween.Kill();
            warningTween = null;
            closedChatImage.color = defaultColor;
        }
    }
}
