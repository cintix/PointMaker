using UnityEngine;

public class InventoryPanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform panel;

    [Header("Settings")]
    [SerializeField] private float animationTime = 0.25f;
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private bool isOpen;
    private Vector2 hiddenPos;
    private Vector2 shownPos;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (panel == null)
        {
            Debug.LogError("InventoryPanelController: Panel reference missing.");
            enabled = false;
            return;
        }

        canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        shownPos = panel.anchoredPosition;
        hiddenPos = shownPos + new Vector2(panel.rect.width, 0f);

        // Start hidden
        SetImmediate(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    // ----------------------------
    // PUBLIC STATE API
    // ----------------------------

    public void Toggle()
    {
        SetState(!isOpen);
    }

    public void Open()
    {
        SetState(true);
    }

    public void Close()
    {
        SetState(false);
    }

    // ----------------------------
    // INTERNAL STATE HANDLING
    // ----------------------------

    private void SetState(bool open)
    {
        if (isOpen == open)
            return;

        isOpen = open;

        canvasGroup.interactable = isOpen;
        canvasGroup.blocksRaycasts = isOpen;

        if (isOpen)
        {
            Tween.Slide(panel, canvasGroup, shownPos, 1f, animationTime);
        }
        else
        {
            Tween.Slide(panel, canvasGroup, hiddenPos, 0f, animationTime);
        }
    }

    private void SetImmediate(bool open)
    {
        isOpen = open;

        panel.anchoredPosition = open ? shownPos : hiddenPos;
        canvasGroup.alpha = open ? 1f : 0f;
        canvasGroup.interactable = open;
        canvasGroup.blocksRaycasts = open;
    }
}
