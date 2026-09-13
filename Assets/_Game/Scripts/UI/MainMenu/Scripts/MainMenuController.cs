using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MainMenuController : MonoBehaviour
{
    private static readonly int OpenMenuHash = Animator.StringToHash("OpenMenu");

    [Header("Required References")]
    [SerializeField] private Animator mainMenuAnimator;
    [SerializeField] private Button startButton;
    [SerializeField] private CanvasGroup menuCanvasGroup;

    [Header("Optional References")]
    [SerializeField] private Selectable firstMenuButton;

    [Header("Timing")]
    [Tooltip("Set this slightly longer than the MainMenu_Open animation clip.")]
    [SerializeField, Min(0f)] private float menuUnlockDelay = 0.70f;

    private bool menuOpened;
    private Coroutine unlockRoutine;

    private void Awake()
    {
        if (mainMenuAnimator == null)
            mainMenuAnimator = GetComponent<Animator>();

        SetMenuInteraction(false);

        if (startButton != null)
        {
            startButton.interactable = true;
            startButton.onClick.AddListener(OpenMenu);
        }
        else
        {
            Debug.LogError("MainMenuController: Start Button is not assigned.", this);
        }

        if (mainMenuAnimator == null)
            Debug.LogError("MainMenuController: Main Menu Animator is not assigned.", this);

        if (menuCanvasGroup == null)
            Debug.LogError("MainMenuController: Menu Canvas Group is not assigned.", this);
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OpenMenu);
    }

    public void OpenMenu()
    {
        if (menuOpened)
            return;

        menuOpened = true;

        if (startButton != null)
            startButton.interactable = false;

        SetMenuInteraction(false);

        if (mainMenuAnimator != null)
            mainMenuAnimator.SetTrigger(OpenMenuHash);

        if (unlockRoutine != null)
            StopCoroutine(unlockRoutine);

        unlockRoutine = StartCoroutine(UnlockMenuAfterAnimation());
    }

    private IEnumerator UnlockMenuAfterAnimation()
    {
        yield return new WaitForSecondsRealtime(menuUnlockDelay);
        EnableMenuInteraction();
        unlockRoutine = null;
    }

    // Optional compatibility method. The recommended setup does not need an
    // AnimationEvent, but a correctly configured event may call this method.
    public void OnMenuOpenFinished()
    {
        if (unlockRoutine != null)
        {
            StopCoroutine(unlockRoutine);
            unlockRoutine = null;
        }

        EnableMenuInteraction();
    }

    private void EnableMenuInteraction()
    {
        SetMenuInteraction(true);

        if (firstMenuButton == null)
            return;

        firstMenuButton.Select();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstMenuButton.gameObject);
    }

    private void SetMenuInteraction(bool enabled)
    {
        if (menuCanvasGroup == null)
            return;

        menuCanvasGroup.interactable = enabled;
        menuCanvasGroup.blocksRaycasts = enabled;
    }
}
