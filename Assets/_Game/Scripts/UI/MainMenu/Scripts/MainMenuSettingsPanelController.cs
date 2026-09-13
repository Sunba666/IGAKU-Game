using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MainMenuSettingsPanelController : MonoBehaviour, ICancelHandler
{
    [Header("Buttons")]
    [SerializeField] private Button openSettingsButton;
    [SerializeField] private Button closeSettingsButton;

    [Header("Canvas Groups")]
    [SerializeField] private CanvasGroup mainMenuCanvasGroup;
    [SerializeField] private CanvasGroup settingsCanvasGroup;

    [Header("Selection")]
    [Tooltip("The first slider, toggle, or button selected when the panel opens.")]
    [SerializeField] private Selectable firstSettingsControl;
    [Tooltip("Usually the SETTINGS button. It is selected again when the panel closes.")]
    [SerializeField] private Selectable returnSelection;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        // Do not enable the main menu here. MainMenuController owns its initial
        // locked state and unlocks it only after the START animation finishes.
        SetSettingsGroupVisible(false);
        IsOpen = false;

        if (openSettingsButton == null)
            Debug.LogError("MainMenuSettingsPanelController: Open Settings Button is not assigned.", this);

        if (closeSettingsButton == null)
            Debug.LogError("MainMenuSettingsPanelController: Close Settings Button is not assigned.", this);

        if (mainMenuCanvasGroup == null)
            Debug.LogError("MainMenuSettingsPanelController: Main Menu Canvas Group is not assigned.", this);

        if (settingsCanvasGroup == null)
            Debug.LogError("MainMenuSettingsPanelController: Settings Canvas Group is not assigned.", this);
    }

    private void OnEnable()
    {
        if (openSettingsButton != null)
            openSettingsButton.onClick.AddListener(OpenSettings);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);
    }

    private void OnDisable()
    {
        if (openSettingsButton != null)
            openSettingsButton.onClick.RemoveListener(OpenSettings);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.RemoveListener(CloseSettings);
    }

    public void OpenSettings()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        SetMainMenuInteraction(false);
        SetSettingsGroupVisible(true);
        Select(firstSettingsControl != null ? firstSettingsControl : closeSettingsButton);
    }

    public void CloseSettings()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        SetSettingsGroupVisible(false);
        SetMainMenuInteraction(true);
        Select(returnSelection != null ? returnSelection : openSettingsButton);
    }

    public void OnCancel(BaseEventData eventData)
    {
        if (!IsOpen)
            return;

        CloseSettings();
        eventData.Use();
    }

    private void SetMainMenuInteraction(bool enabled)
    {
        if (mainMenuCanvasGroup == null)
            return;

        // Keep the menu visible behind the overlay, but prevent it from
        // receiving clicks or navigation while Settings is open.
        mainMenuCanvasGroup.interactable = enabled;
        mainMenuCanvasGroup.blocksRaycasts = enabled;
    }

    private void SetSettingsGroupVisible(bool visible)
    {
        if (settingsCanvasGroup == null)
            return;

        settingsCanvasGroup.alpha = visible ? 1f : 0f;
        settingsCanvasGroup.interactable = visible;
        settingsCanvasGroup.blocksRaycasts = visible;
    }

    private static void Select(Selectable selectable)
    {
        if (selectable == null || !selectable.IsActive() || !selectable.IsInteractable())
            return;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        selectable.Select();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
    }
}
