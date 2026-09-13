using UnityEngine;

[DefaultExecutionOrder(-100)]
public sealed class BattleFighterSpawner : MonoBehaviour
{
    [Header("出生点")]
    [SerializeField] private Transform player1Spawn;
    [SerializeField] private Transform player2Spawn;

    [Header("直接运行战斗场景时的默认角色")]
    [SerializeField]
    private CharacterSelectCharacterData defaultPlayer1Character;

    [SerializeField]
    private CharacterSelectCharacterData defaultPlayer2Character;

    [Header("运行时角色父对象")]
    [SerializeField] private Transform fighterRuntimeRoot;

    [Header("系统引用")]
    [SerializeField] private BattleRoundManager roundManager;
    [SerializeField] private BattleHUDController battleHUD;

    private GameObject player1Instance;
    private GameObject player2Instance;

    public GameObject Player1Instance => player1Instance;
    public GameObject Player2Instance => player2Instance;

    private void Awake()
    {
        SpawnSelectedFighters();
    }

    private void SpawnSelectedFighters()
    {
        CharacterSelectCharacterData player1Data =
            LocalVersusSelectionStore.Player1Character != null
                ? LocalVersusSelectionStore.Player1Character
                : defaultPlayer1Character;

        CharacterSelectCharacterData player2Data =
            LocalVersusSelectionStore.Player2Character != null
                ? LocalVersusSelectionStore.Player2Character
                : defaultPlayer2Character;

        if (!ValidateCharacterData(player1Data, "Player 1"))
        {
            return;
        }

        if (!ValidateCharacterData(player2Data, "Player 2"))
        {
            return;
        }

        FighterHealth player1Health = CreateFighter(
            player1Data,
            player1Spawn,
            LocalFighterController2D.LocalPlayerSlot.Player1,
            true,
            "P1"
        );

        FighterHealth player2Health = CreateFighter(
            player2Data,
            player2Spawn,
            LocalFighterController2D.LocalPlayerSlot.Player2,
            false,
            "P2"
        );

        if (player1Health == null || player2Health == null)
        {
            Debug.LogError(
                "BattleFighterSpawner：角色生成失败。",
                this
            );

            return;
        }

        player1Instance = player1Health.gameObject;
        player2Instance = player2Health.gameObject;

        if (roundManager != null)
        {
            roundManager.ConfigurePlayers(
                player1Health,
                player2Health
            );
        }
        else
        {
            Debug.LogError(
                "BattleFighterSpawner 没有连接 Round Manager。",
                this
            );
        }

        if (battleHUD != null)
        {
            battleHUD.ConfigurePlayers(
                player1Health,
                player2Health,
                player1Data.DisplayName,
                player2Data.DisplayName
            );
        }
        else
        {
            Debug.LogError(
                "BattleFighterSpawner 没有连接 Battle HUD。",
                this
            );
        }

        Debug.Log(
            "战斗角色生成完成：P1 = " +
            player1Data.DisplayName +
            "，P2 = " +
            player2Data.DisplayName
        );
    }

    private FighterHealth CreateFighter(
        CharacterSelectCharacterData characterData,
        Transform spawnPoint,
        LocalFighterController2D.LocalPlayerSlot playerSlot,
        bool faceRight,
        string playerPrefix
    )
    {
        if (spawnPoint == null)
        {
            Debug.LogError(
                "BattleFighterSpawner：" +
                playerPrefix +
                " 出生点没有连接。",
                this
            );

            return null;
        }

        GameObject instance = Instantiate(
            characterData.FighterPrefab,
            spawnPoint.position,
            Quaternion.identity,
            fighterRuntimeRoot
        );

        instance.name =
            playerPrefix + "_" + characterData.CharacterId;

        LocalFighterController2D controller =
            instance.GetComponent<LocalFighterController2D>();

        FighterHealth health =
            instance.GetComponent<FighterHealth>();

        if (controller == null)
        {
            Debug.LogError(
                instance.name +
                " 缺少 LocalFighterController2D。",
                instance
            );

            Destroy(instance);
            return null;
        }

        if (health == null)
        {
            Debug.LogError(
                instance.name +
                " 缺少 FighterHealth。",
                instance
            );

            Destroy(instance);
            return null;
        }

        controller.ConfigurePlayer(
            playerSlot,
            faceRight
        );

        return health;
    }

    private bool ValidateCharacterData(
        CharacterSelectCharacterData characterData,
        string playerLabel
    )
    {
        if (characterData == null)
        {
            Debug.LogError(
                "BattleFighterSpawner：" +
                playerLabel +
                " 没有角色数据，也没有设置默认角色。",
                this
            );

            return false;
        }

        if (characterData.FighterPrefab == null)
        {
            Debug.LogError(
                "BattleFighterSpawner：" +
                characterData.DisplayName +
                " 没有连接 Fighter Prefab。",
                characterData
            );

            return false;
        }

        return true;
    }
}