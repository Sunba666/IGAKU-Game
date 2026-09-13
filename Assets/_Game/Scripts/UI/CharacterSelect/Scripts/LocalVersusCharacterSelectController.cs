using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public sealed class LocalVersusCharacterSelectController : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private CharacterSelectSlot[] slots;
    [SerializeField, Min(1)] private int columns = 6;
    [SerializeField] private bool skipLockedSlots = true;
    [SerializeField] private bool allowMirrorMatch = true;

    [Header("Player 1 - WASD / J")]
    [SerializeField] private RectTransform player1Cursor;
    [SerializeField] private Vector2 player1CursorOffset = new Vector2(-5f, 0f);
    [SerializeField] private Image player1LargePreview;
    [SerializeField] private TMP_Text player1Name;
    [SerializeField] private GameObject player1ReadyIndicator;

    [Header("Player 2 - Arrow Keys / 1")]
    [SerializeField] private RectTransform player2Cursor;
    [SerializeField] private Vector2 player2CursorOffset = new Vector2(5f, 0f);
    [SerializeField] private Image player2LargePreview;
    [SerializeField] private TMP_Text player2Name;
    [SerializeField] private GameObject player2ReadyIndicator;

    [Header("Both Players Ready")]
    [Tooltip("两名玩家都确认后触发。可在这里连接 SimpleSceneLoader.LoadScene，并填写战斗场景名。")]
    [SerializeField] private UnityEvent onBothPlayersReady;

    [Header("Optional UI Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip confirmSound;
    [SerializeField] private AudioClip cancelSound;
    [SerializeField] private AudioClip bothPlayersReadySound;

    private int player1Index = -1;
    private int player2Index = -1;
    private bool player1Ready;
    private bool player2Ready;
    private bool readyEventSent;

    public CharacterSelectCharacterData Player1Selection => GetData(player1Index);
    public CharacterSelectCharacterData Player2Selection => GetData(player2Index);
    public bool Player1Ready => player1Ready;
    public bool Player2Ready => player2Ready;

    private void Start()
    {
        if (!ValidateSetup())
        {
            enabled = false;
            return;
        }

        foreach (CharacterSelectSlot slot in slots)
        {
            if (slot != null)
                slot.RefreshVisual();
        }

        player1Index = FindSelectableFrom(0, 1, -1);
        player2Index = FindSelectableFrom(slots.Length - 1, -1, player1Index);

        if (player1Index < 0 || player2Index < 0)
        {
            Debug.LogError(
                "角色选择：至少需要一个 Implemented 已勾选的角色。若禁止同角色对战，则至少需要两个。",
                this);
            enabled = false;
            return;
        }

        SetReady(1, false);
        SetReady(2, false);
        RefreshPlayer(1);
        RefreshPlayer(2);
    }

    private void Update()
    {
        HandlePlayer1Input();
        HandlePlayer2Input();
    }

    private void HandlePlayer1Input()
    {
        if (!player1Ready)
        {
            if (P1UpPressed()) MovePlayer(1, 0, -1);
            else if (P1DownPressed()) MovePlayer(1, 0, 1);
            else if (P1LeftPressed()) MovePlayer(1, -1, 0);
            else if (P1RightPressed()) MovePlayer(1, 1, 0);
        }

        if (P1ConfirmPressed())
            ToggleReady(1);
    }

    private void HandlePlayer2Input()
    {
        if (!player2Ready)
        {
            if (P2UpPressed()) MovePlayer(2, 0, -1);
            else if (P2DownPressed()) MovePlayer(2, 0, 1);
            else if (P2LeftPressed()) MovePlayer(2, -1, 0);
            else if (P2RightPressed()) MovePlayer(2, 1, 0);
        }

        if (P2ConfirmPressed())
            ToggleReady(2);
    }

    private void MovePlayer(int player, int directionX, int directionY)
    {
        int currentIndex = player == 1 ? player1Index : player2Index;
        int otherIndex = player == 1 ? player2Index : player1Index;
        int nextIndex = FindGridDestination(
            currentIndex,
            directionX,
            directionY,
            otherIndex);

        if (nextIndex < 0 || nextIndex == currentIndex)
            return;

        if (player == 1)
            player1Index = nextIndex;
        else
            player2Index = nextIndex;

        RefreshPlayer(player);
        PlayUiSound(moveSound);
    }

    private int FindGridDestination(
        int currentIndex,
        int directionX,
        int directionY,
        int otherIndex)
    {
        if (currentIndex < 0 || currentIndex >= slots.Length)
            return currentIndex;

        int rowCount = Mathf.CeilToInt(slots.Length / (float)columns);
        int currentRow = currentIndex / columns;
        int currentColumn = currentIndex % columns;
        int maximumAttempts = directionX != 0 ? columns : rowCount;

        for (int step = 1; step <= maximumAttempts; step++)
        {
            int row = currentRow;
            int column = currentColumn;

            if (directionX != 0)
                column = PositiveModulo(currentColumn + directionX * step, columns);
            else
                row = PositiveModulo(currentRow + directionY * step, rowCount);

            int candidate = row * columns + column;
            if (candidate < 0 || candidate >= slots.Length)
                continue;

            if (CanLandOn(candidate, otherIndex))
                return candidate;
        }

        return currentIndex;
    }

    private bool CanLandOn(int index, int otherIndex)
    {
        CharacterSelectSlot slot = slots[index];
        if (slot == null)
            return false;

        if (skipLockedSlots && !slot.IsSelectable)
            return false;

        if (!allowMirrorMatch && index == otherIndex)
            return false;

        return true;
    }

    private int FindSelectableFrom(int start, int direction, int otherIndex)
    {
        for (int step = 0; step < slots.Length; step++)
        {
            int candidate = PositiveModulo(start + direction * step, slots.Length);
            CharacterSelectSlot slot = slots[candidate];

            if (slot == null || !slot.IsSelectable)
                continue;

            if (!allowMirrorMatch && candidate == otherIndex)
                continue;

            return candidate;
        }

        return -1;
    }

    private void ToggleReady(int player)
    {
        int index = player == 1 ? player1Index : player2Index;
        if (index < 0 || index >= slots.Length || !slots[index].IsSelectable)
            return;

        bool nextReady = player == 1 ? !player1Ready : !player2Ready;
        bool willMakeBothReady = nextReady &&
                                 ((player == 1 && player2Ready) ||
                                  (player == 2 && player1Ready));

        SetReady(player, nextReady);

        if (willMakeBothReady && bothPlayersReadySound != null)
            PlayUiSound(bothPlayersReadySound);
        else
            PlayUiSound(nextReady ? confirmSound : cancelSound);

        CheckBothPlayersReady();
    }

    private void SetReady(int player, bool ready)
    {
        if (player == 1)
        {
            player1Ready = ready;
            if (player1ReadyIndicator != null)
                player1ReadyIndicator.SetActive(ready);
        }
        else
        {
            player2Ready = ready;
            if (player2ReadyIndicator != null)
                player2ReadyIndicator.SetActive(ready);
        }

        if (!player1Ready || !player2Ready)
            readyEventSent = false;
    }

    private void CheckBothPlayersReady()
    {
        if (!player1Ready || !player2Ready || readyEventSent)
            return;

        readyEventSent = true;
        LocalVersusSelectionStore.Save(Player1Selection, Player2Selection);
        onBothPlayersReady?.Invoke();
    }

    private void RefreshPlayer(int player)
    {
        int index = player == 1 ? player1Index : player2Index;
        CharacterSelectSlot slot = slots[index];
        CharacterSelectCharacterData data = slot.CharacterData;

        RectTransform cursor = player == 1 ? player1Cursor : player2Cursor;
        Vector2 cursorOffset = player == 1 ? player1CursorOffset : player2CursorOffset;
        MoveCursor(cursor, slot.RectTransform, cursorOffset);

        Image preview = player == 1 ? player1LargePreview : player2LargePreview;
        if (preview != null)
        {
            Sprite previewSprite = data != null ? data.LargePreview : null;
            preview.sprite = previewSprite;
            preview.enabled = previewSprite != null;
            preview.preserveAspect = true;

            CharacterSelectPreviewMotion previewMotion =
                preview.GetComponent<CharacterSelectPreviewMotion>();

            if (previewMotion != null && Application.isPlaying)
                previewMotion.Play();
        }

        TMP_Text nameLabel = player == 1 ? player1Name : player2Name;
        if (nameLabel != null)
            nameLabel.text = data != null ? data.DisplayName : "未实装";
    }

    private static void MoveCursor(
        RectTransform cursor,
        RectTransform target,
        Vector2 offset)
    {
        if (cursor == null || target == null)
            return;

        RectTransform cursorParent = cursor.parent as RectTransform;
        Canvas canvas = cursor.GetComponentInParent<Canvas>();
        if (cursorParent == null || canvas == null)
            return;

        Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            eventCamera,
            target.position);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cursorParent,
                screenPoint,
                eventCamera,
                out Vector2 localPoint))
        {
            cursor.anchoredPosition = localPoint + offset;
        }

        cursor.gameObject.SetActive(true);
    }

    private CharacterSelectCharacterData GetData(int index)
    {
        if (index < 0 || index >= slots.Length || slots[index] == null)
            return null;

        return slots[index].CharacterData;
    }

    private bool ValidateSetup()
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("角色选择：Slots 没有赋值。", this);
            return false;
        }

        if (columns <= 0)
        {
            Debug.LogError("角色选择：Columns 必须大于 0。", this);
            return false;
        }

        if (player1Cursor == null || player2Cursor == null)
        {
            Debug.LogError("角色选择：P1 Cursor 或 P2 Cursor 没有赋值。", this);
            return false;
        }

        return true;
    }

    private static int PositiveModulo(int value, int modulus)
    {
        return (value % modulus + modulus) % modulus;
    }

    private void PlayUiSound(AudioClip clip)
    {
        if (uiAudioSource == null || clip == null)
            return;

        uiAudioSource.PlayOneShot(clip);
    }

    private static bool P1UpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.W);
#endif
    }

    private static bool P1DownPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.S);
#endif
    }

    private static bool P1LeftPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.A);
#endif
    }

    private static bool P1RightPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.D);
#endif
    }

    private static bool P1ConfirmPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.J);
#endif
    }

    private static bool P2UpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.upArrowKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.UpArrow);
#endif
    }

    private static bool P2DownPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.downArrowKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.DownArrow);
#endif
    }

    private static bool P2LeftPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.leftArrowKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.LeftArrow);
#endif
    }

    private static bool P2RightPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.rightArrowKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.RightArrow);
#endif
    }

    private static bool P2ConfirmPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null &&
               (Keyboard.current.digit1Key.wasPressedThisFrame ||
                Keyboard.current.numpad1Key.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Alpha1) ||
               Input.GetKeyDown(KeyCode.Keypad1);
#endif
    }
}
