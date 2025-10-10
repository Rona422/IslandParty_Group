using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using UnityEngine;

using UnityEngine.UI;

using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class MiniGameManager : SingletonMonoBehaviour<MiniGameManager>
{
    public enum GameType
    {
        ONE_ALL,
        ONE_THREE,
        ONE_ONE,
    }
    public enum UpdateState
    {
        HOW_TO_PLAY,
        PLAY_COUNT_DOWN,
        PLAYING,
        PLAY_FINISH,
        DRAW_DICE,
        MOVE_TO_RANK,
        FINISH,
        RESULT
    }
    //進行させるゲームの管理用
    private MiniGameBase m_CurrentPlayGame;
    //今のUpdateの状態
    private UpdateState m_CurrentUpdateState;
    private Dictionary<UpdateState, Action> m_UpdateActions;

    //MainGameの方からこのboolを見てランクが決まっているかどうか見続ける
    private bool m_IsDesideRank;
    public bool GetIsDesideRank()
    {
        return m_IsDesideRank;
    }

    //ミニゲームに参加するプレイヤーリスト
    private List<MainPlayer> m_MiniGameJoinPlayers = new();
    public List<MainPlayer> GetMiniGameJoinPlayers() { return m_MiniGameJoinPlayers; }

    //メインのゲームから参照させるランキングリスト
    protected Dictionary<int, MainPlayer> m_RankList = new();
    public Dictionary<int, MainPlayer> GetRankList()
    {
        return m_RankList;
    }

    // カウントダウン
    [SerializeField] private Text m_CountDownText;
    private float m_CurrentCountDownTime;
    private float m_CountDownTime = 3.0f;
    // Startのテキスト
    [SerializeField] private Text m_StartText;
    // Finishのテキスト
    [SerializeField] private Text m_FinishText;

    //ランキング順に並べる用リスト
    private List<MainPlayer> m_RankPlayers = new();
    private class DiceDrawGroup
    {
        public List<MainPlayer> groupA = new();
        public List<MainPlayer> groupB = new(); // ONE_THREE 用（3人側代表 or 相手）
        public GameType gameType;
        public bool isResolved = false;
        // 生成時に自動で一意のIDを作る
        public string groupId { get; private set; } = Guid.NewGuid().ToString();
    }
    private List<DiceDrawGroup> m_DiceDrawGroups = new();
    private int m_InputWaitCount;

    // プレイヤー表示用
    private List<Sugoroku_PlayerBase> m_ResultPlayers = new();
    private List<Vector3> m_RankOffsetPositions = new();
    private float m_MoveTime = 1.5f;
    private float m_MoveTimer = 0f;
    [SerializeField] private List<Image> m_RankImages = new ();
    [SerializeField] private List<Sprite> m_RankSprites = new ();

    // 背景のImage
    [SerializeField] private Image m_BackGroundUI;
    // ミニゲームタイトルUI
    [SerializeField] private Image m_TitleBack;
    [SerializeField] private Text m_TitleText;
    // 遊び方UIの親
    [SerializeField] private GameObject m_HowToPlayUI;
    // 遊び方UI
    [SerializeField] private Image m_PlayImage;
    [SerializeField] private Text m_ExplanationText;
    [SerializeField] private Text m_HowToOperateText;
    // 引き分けメッセージ
    [SerializeField] private Image m_DrawMessage;
    protected override void Awake()
    {
        base.Awake();
        //キーとUpdate関数の関連付け初期化
        m_UpdateActions = new Dictionary<UpdateState, Action>()
        {
            {UpdateState.HOW_TO_PLAY,Update_HowToPlay },
            {UpdateState.PLAY_COUNT_DOWN,Update_PlayCountDown },
            {UpdateState.PLAYING,Update_Playing },
            {UpdateState.PLAY_FINISH,Update_PlayFinish },
            {UpdateState.DRAW_DICE,Update_DrawDice },
            {UpdateState.MOVE_TO_RANK, Update_MoveToRank},
            {UpdateState.FINISH,Update_Finish },
            {UpdateState.RESULT,Update_Result },
        };


        // 順位表示用ポジション（左から右へ）
        m_RankOffsetPositions = new List<Vector3>
        {
        new (-0.6f, 0, 1.2f), // 1位
        new (-0.2f, 0, 1.2f), // 2位
        new ( 0.2f, 0, 1.2f), // 3位
        new ( 0.6f, 0, 1.2f), // 4位
        };

        m_CountDownText.gameObject.SetActive(false);
        m_StartText.gameObject.SetActive(false);
        m_FinishText.gameObject.SetActive(false);
        // Startテキストの色を保持
        ResetMiniGameManagerData();
    }

    private void ResetMiniGameManagerData()
    {
        m_CurrentPlayGame = null;
        m_IsDesideRank = false;
        m_RankPlayers.Clear();
        m_MiniGameJoinPlayers.Clear();
        m_RankList.Clear();
        m_MoveTimer = 0.0f;
        m_InputWaitCount = 0;

        m_CurrentCountDownTime = m_CountDownTime;
        foreach (var rankImage in m_RankImages) { rankImage.gameObject.SetActive(false); }
        m_BackGroundUI.gameObject.SetActive(false);
        m_TitleBack.gameObject.SetActive(false);
        m_HowToPlayUI.SetActive(false);
        m_DrawMessage.gameObject.SetActive(false);
        m_ResultPlayers.Clear();
    }

    //メインのゲームの方からこの関数を呼びミニゲームを開始させる
    public void MiniGameStart(MiniGameBase _StartGame, List<MainPlayer> _MiniGameJoinPlayers)
    {
        if (m_CurrentPlayGame == null)
        {
            int _PlayerMax = 0;
            switch (_StartGame.m_GameType)
            {
                case GameType.ONE_ALL: _PlayerMax = 4; break;
                case GameType.ONE_THREE: _PlayerMax = 4; break;
                case GameType.ONE_ONE: _PlayerMax = 2; break;
                default: Debug.Log("ミニゲームの種類おかしくね？"); break;
            }
            if (_MiniGameJoinPlayers.Count != _PlayerMax)
            {
                Debug.Log("ミニゲームのプレイ人数と参加人数が合わへんで");
            }
            //ミニゲームに参加するプレイヤーを準備
            m_MiniGameJoinPlayers = new(_MiniGameJoinPlayers);
            //ミニゲーム生成
            m_CurrentPlayGame = Instantiate(_StartGame, gameObject.transform);
            //ミニゲームの処理を全て停止する
            m_CurrentPlayGame.gameObject.SetActive(true);
            m_CurrentPlayGame.SetFrozen(true);
            // ミニゲームのプレイヤー初期化
            m_CurrentPlayGame.InitPlayers();

            // ミニゲームのテキスト設定
            m_TitleText.text = m_CurrentPlayGame.m_GameTitle;
            m_PlayImage.sprite = m_CurrentPlayGame.m_PlayImage;
            m_ExplanationText.text = m_CurrentPlayGame.m_GameTutorial;
            m_HowToOperateText.text = m_CurrentPlayGame.m_GameControl;

            CameraManager.instance.SetMiniGameCamera(m_CurrentPlayGame.GetMiniGameVCam());
            CameraManager.instance.SwitchToMiniGameCam();

            //Update状態を更新
            m_CurrentUpdateState = UpdateState.HOW_TO_PLAY;
        }
        else
        {
            Debug.Log("まだ続いてるゲームがあるみたい>m<");
        }
    }

    public void MiniGameUpdate()
    {
        //m_CurrentUpdateStateに対応する関数を回す
        if (m_UpdateActions.TryGetValue(m_CurrentUpdateState, out var action))
        {
            Debug.Log(m_CurrentUpdateState);
            action.Invoke();
        }
    }
    private void Update_HowToPlay()
    {
        CoroutineRunner.instance.RunOnce("WaitPlayer_HowToPlay", WaitPlayer_HowToPlay());
    }

    private IEnumerator WaitPlayer_HowToPlay()
    {
        // UIの表示
        m_BackGroundUI.gameObject.SetActive(true);
        m_TitleBack.gameObject.SetActive(true);
        m_HowToPlayUI.SetActive(true);
        InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.A, InputIconDisplayer.PositionType.MIGAME_HOWTOPLAY);
        CoroutineRunner.instance.RunCoroutine(PlayersUi.instance.StateReflection(PlayersUi.PlayersUiState.MINIGAME_START));
        //int playerCount = m_MiniGameJoinPlayers.Count;
        //List<bool> playerInputList = new(playerCount);
        //bool npcTriggered = false; // NPC入力遅延を開始したかどうか

        //// 初期化
        //for (int i = 0; i < playerCount; i++)
        //{
        //    playerInputList.Add(false);
        //}

        //while (true)
        //{
        //    // 人間プレイヤーの入力をチェック
        //    for (int i = 0; i < playerCount; i++)
        //    {
        //        if (m_MiniGameJoinPlayers[i].IsNpc)
        //            continue; // NPCはまだ無視

        //        if (!playerInputList[i] &&
        //            m_MiniGameJoinPlayers[i].Input.actions["X_Action"].WasReleasedThisFrame())
        //        {
        //            playerInputList[i] = true;
        //            Debug.Log(i + "準備おk");
        //        }
        //    }

        //    // すべての人間プレイヤーが入力済みか判定
        //    bool allHumansInput = true;
        //    for (int i = 0; i < playerCount; i++)
        //    {
        //        if (!m_MiniGameJoinPlayers[i].IsNpc && !playerInputList[i])
        //        {
        //            allHumansInput = false;
        //            break;
        //        }
        //    }

        //    // まだNPC入力遅延処理開始してなければ、人間全員入力済みで開始
        //    if (allHumansInput && !npcTriggered)
        //    {
        //        npcTriggered = true;
        //        for (int i = 0; i < playerCount; i++)
        //        {
        //            if (m_MiniGameJoinPlayers[i].IsNpc && !playerInputList[i])
        //            {
        //                int index = i; // クロージャ対策
        //                CoroutineRunner.instance.DelayedCall(0.5f, () =>
        //                {
        //                    playerInputList[index] = true;
        //                    Debug.Log(index + "準備おk");

        //                });
        //            }
        //        }
        //    }

        //    // 全員入力済みなら終了
        //    if (playerInputList.All(pressed => pressed))
        //        break;

        //    yield return null;
        //}
        //全員の入力待ち
        yield return CoroutineRunner.instance.RunCoroutine(PlayersUi.instance.IsAllCheck());
        yield return new WaitForSeconds(1.0f);
        CoroutineRunner.instance.RunCoroutine(PlayersUi.instance.StateReflection(PlayersUi.PlayersUiState.STANDBY));

        // UIの非表示
        m_BackGroundUI.gameObject.SetActive(false);
        m_TitleBack.gameObject.SetActive(false);
        m_HowToPlayUI.SetActive(false);
        InputIconDisplayer.instance.HideAllIcon();

        m_CountDownText.gameObject.SetActive(true);
        m_CurrentCountDownTime -= Time.deltaTime;
        m_CurrentUpdateState = UpdateState.PLAY_COUNT_DOWN;
    }
    private void Update_PlayCountDown()
    {
        if (m_CurrentCountDownTime > 0f)
        {
            // タイマー減少
            m_CurrentCountDownTime -= Time.deltaTime;
            if (m_CurrentCountDownTime < 0f) m_CurrentCountDownTime = 0f;
            m_CountDownText.text = Mathf.CeilToInt(m_CurrentCountDownTime).ToString();
            return;
        }
        m_CountDownText.gameObject.SetActive(false);
        PlayStart();
    }
    private void PlayStart()
    {
        //ミニゲームの処理を再開する
        m_CurrentPlayGame.SetFrozen(false);
        PlayerManager.instance.IsJoined(false);
        CoroutineRunner.instance.RunCoroutine(FadeOutText(m_StartText));
        m_CurrentUpdateState = UpdateState.PLAYING;
    }

    private void Update_Playing()
    {
    }
    public void PlayFinish()
    {
        m_CurrentUpdateState = UpdateState.PLAY_FINISH;
    }

    private void Update_PlayFinish()
    {
        CoroutineRunner.instance.RunOnce("PlayFinishCoroutine", PlayFinishCoroutine());
    }

    private IEnumerator PlayFinishCoroutine()
    {
        // ミニゲームを停止
        m_CurrentPlayGame.SetFrozen(true);
        // ミニゲームのカメラも止まってしまうので起動する
        m_CurrentPlayGame.GetMiniGameVCam().enabled = true;

        // UI表示
        m_BackGroundUI.gameObject.SetActive(true);
        m_TitleBack.gameObject.SetActive(true);

        // リザルト作成
        SetupResult();

        yield return new WaitForSeconds(1.0f);

        // ダイス演出か、結果画面へ遷移
        if (m_DiceDrawGroups.Count > 0)
        {
            m_DrawMessage.gameObject.SetActive(true);
            yield return new WaitForSeconds(2.5f);
            m_DrawMessage.gameObject.SetActive(false);
            yield return new WaitForSeconds(1.0f);

            InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.X, InputIconDisplayer.PositionType.MIGAME_END);
            m_CurrentUpdateState = UpdateState.DRAW_DICE;
        }
        else
        {
            m_CurrentUpdateState = UpdateState.FINISH;
        }
    }

    private void SetupResult()
    {
        // リザルト画面背景をセット
        m_BackGroundUI.gameObject.SetActive(true);

        // 降順ソート（大きい方が上の前提）
        m_RankPlayers = m_MiniGameJoinPlayers
            .OrderByDescending(p => p.rank)
            .ToList();

        // 比較用に元のrank値を保存しておく
        List<int> originalRanks = m_RankPlayers.Select(p => p.rank).ToList();

        // 順位の再割り当て
        int currentRank = 4;
        int sameRankCount = 1;

        m_RankPlayers[0].rank = currentRank;

        for (int i = 1; i < m_RankPlayers.Count; i++)
        {
            if (originalRanks[i] == originalRanks[i - 1])
            {
                sameRankCount++;
            }
            else
            {
                currentRank -= sameRankCount;
                sameRankCount = 1;
            }

            m_RankPlayers[i].rank = currentRank;
        }


        var sugorokuPlayers = SugorokuManager.instance.GetPlayers();
        // プレイヤーのコピー作成
        for (int i = 0; i < m_MiniGameJoinPlayers.Count; i++)
        {
            var miniGamePlayer = m_MiniGameJoinPlayers[i];
            var playerBase = sugorokuPlayers[(int)miniGamePlayer.Type];
            var copy = Instantiate(playerBase);
            copy.SetPlayerNumber(playerBase.GetPlayerNum());
            copy.enabled = false;
            copy.transform.localScale = Vector3.one * 0.2f;

            if (m_CurrentPlayGame.m_GameType == GameType.ONE_ONE)
            {
                copy.transform.position = GetRankPos(i + 1);
            }
            else
            {
                copy.transform.position = GetRankPos(i);
            }
            m_ResultPlayers.Add(copy);
        }

        var sortedForPosition = m_MiniGameJoinPlayers
            .OrderByDescending(p => p.rank)  // スコア順に高い方が前
            .ThenBy(p => (int)p.Type)        // 同点ならプレイヤー番号順
            .ToList();

        for (int i = 0; i < sortedForPosition.Count; i++)
        {
            var sortedPlayer = sortedForPosition[i];
            var copy = m_ResultPlayers.FirstOrDefault(p => p.GetPlayerNum() == (int)sortedPlayer.Type);
            if (copy == null) continue;

            if (m_CurrentPlayGame.m_GameType == GameType.ONE_ONE)
            {
                copy.transform.position = GetRankPos(i + 1);
            }
            else
            {
                copy.transform.position = GetRankPos(i);
            }

            // カメラ方向（水平）
            Vector3 toCam = Camera.main.transform.position - copy.transform.position;
            toCam.y = 0;
            if (toCam != Vector3.zero)
                copy.transform.rotation = Quaternion.LookRotation(toCam);
        }

        for (int i = 0; i < m_RankPlayers.Count; i++)
        {
            int rank = 4 - m_RankPlayers[i].rank; // 必ず0~3
            if (m_CurrentPlayGame.m_GameType == GameType.ONE_ONE)
            {
                m_RankImages[i + 1].sprite = m_RankSprites[rank];
                m_RankImages[i + 1].gameObject.SetActive(true);
            }
            else
            {
                m_RankImages[i].sprite = m_RankSprites[rank];
                m_RankImages[i].gameObject.SetActive(true);
            }
        }

        m_DiceDrawGroups.Clear();

        switch (m_CurrentPlayGame.m_GameType)
        {
            case GameType.ONE_ALL:
                // 同rankプレイヤーグループを収集
                var drawGroups = m_RankPlayers
                    .GroupBy(p => p.rank)
                    .Where(g => g.Count() >= 2)
                    .ToList();

                // 複数グループ対応（例：1,1,3,3）
                foreach (var group in drawGroups)
                {
                    m_DiceDrawGroups.Add(new DiceDrawGroup
                    {
                        groupA = group.ToList(),
                        gameType = GameType.ONE_ALL
                    });
                }
                break;

            case GameType.ONE_THREE:
                var solo = m_MiniGameJoinPlayers[0];
                var team = m_MiniGameJoinPlayers.Skip(1).ToList();
                bool isDraw = team.All(p => p.rank == solo.rank);
                if (isDraw)
                {
                    m_DiceDrawGroups.Add(new DiceDrawGroup
                    {
                        groupA = new List<MainPlayer> { solo },
                        groupB = new List<MainPlayer> { team[UnityEngine.Random.Range(0, team.Count)] },
                        gameType = GameType.ONE_THREE
                    });
                }
                break;

            case GameType.ONE_ONE:
                if (m_RankPlayers[0].rank == m_RankPlayers[1].rank)
                {
                    m_DiceDrawGroups.Add(new DiceDrawGroup
                    {
                        groupA = new List<MainPlayer> { m_RankPlayers[0] },
                        groupB = new List<MainPlayer> { m_RankPlayers[1] },
                        gameType = GameType.ONE_ONE
                    });
                }
                break;
        }
    }

    private void Update_DrawDice()
    {
        // 未処理のグループをすべて取得
        var unresolvedGroups = m_DiceDrawGroups
            .Where(g => !g.isResolved)
            .ToList();

        if (unresolvedGroups.Count == 0)
        {
            m_CurrentUpdateState = UpdateState.MOVE_TO_RANK;
            return;
        }

        for (int i = 0; i < unresolvedGroups.Count; i++)
        {
            var group = unresolvedGroups[i];
            CoroutineRunner.instance.RunOnce(
                $"resolve_draw_{group.groupId}",   // ← インデックスではなくユニークID
                ResolveDrawGroup(group)
            );
        }
        if (m_InputWaitCount == 0)
        {
            InputIconDisplayer.instance.HideAllIcon();
        }
    }
    private IEnumerator ResolveDrawGroup(DiceDrawGroup group)
    {
        List<MainPlayer> allParticipants = new();

        switch (group.gameType)
        {
            case GameType.ONE_ONE:
                allParticipants.Add(group.groupA[0]);
                allParticipants.Add(group.groupB[0]);
                break;

            case GameType.ONE_THREE:
                allParticipants.Add(group.groupA[0]); // 一人側
                allParticipants.Add(group.groupB[0]); // 代表
                break;

            case GameType.ONE_ALL:
                allParticipants.AddRange(group.groupA); // 全員
                break;
        }

        // 出目をシャッフル
        List<int> shuffledNumbers = Enumerable.Range(1, 6)
                                              .OrderBy(_ => UnityEngine.Random.value)
                                              .ToList();

        Dictionary<MainPlayer, int> playerDiceResults = new();
        Dictionary<MainPlayer, DiceController> playerDiceControllers = new();
        HashSet<MainPlayer> rolledPlayers = new();

        // ダイスを全員分生成して並べておく
        for (int i = 0; i < allParticipants.Count; i++)
        {
            var player = allParticipants[i];

            DiceController dice;
            int dicePos = m_RankPlayers.FindIndex(p => (int)p.Type == (int)player.Type);
            if (m_CurrentPlayGame.m_GameType == GameType.ONE_ONE)
            {
                dice = DiceManager.instance.GetDice(
                    (int)DiceManager.DiceType.WHITE,
                    (DiceManager.DicePositionType)(dicePos + 6)
                    );
            }
            else
            {
                dice = DiceManager.instance.GetDice(
                    (int)DiceManager.DiceType.WHITE,
                    (DiceManager.DicePositionType)(dicePos + 5)
                    );
            }
            dice.gameObject.SetActive(true);

            playerDiceControllers[player] = dice;
            m_InputWaitCount++;

        }


        // 全員が振るまで待つループ
        while (rolledPlayers.Count < allParticipants.Count)
        {
            foreach (var player in allParticipants)
            {
                if (rolledPlayers.Contains(player))
                    continue;

                // 入力があったら振る
                if (player.IsNpc || player.Input.actions["X_Action"].WasReleasedThisFrame())
                {
                    int diceNum = shuffledNumbers[rolledPlayers.Count];
                    playerDiceControllers[player].RollDice(diceNum);
                    playerDiceResults[player] = diceNum;
                    rolledPlayers.Add(player);
                    m_InputWaitCount--;
                }
            }
            yield return null;
        }

        // 全ダイスが止まるまで待機
        yield return new WaitUntil(() => playerDiceControllers.Values.All(d => !d.GetIsRolling()));
        yield return new WaitForSeconds(2.0f);

        // ダイス削除
        foreach (var dice in playerDiceControllers.Values)
            GameObject.Destroy(dice.gameObject);

        // 順位決定
        switch (group.gameType)
        {
            case GameType.ONE_ONE:
                var ordered = playerDiceResults.OrderByDescending(p => p.Value).Select(p => p.Key).ToList();
                ReplacePlayersInRankList(group.groupA.Concat(group.groupB).ToList(), ordered);
                break;

            case GameType.ONE_THREE:
                int numA = playerDiceResults[group.groupA[0]];
                int numB = playerDiceResults[group.groupB[0]];
                if (numA >= numB)
                    SetWinnersToTop(group.groupA);
                else
                    SetWinnersToTop(group.groupB);
                break;

            case GameType.ONE_ALL:
                var sorted = playerDiceResults.OrderByDescending(p => p.Value).Select(p => p.Key).ToList();
                ReplacePlayersInRankList(group.groupA, sorted);
                break;
        }
        group.isResolved = true;
    }
    private void ReplacePlayersInRankList(List<MainPlayer> original, List<MainPlayer> sorted)
    {
        var indices = m_RankPlayers
            .Select((p, i) => new { player = p, index = i })
            .Where(x => original.Contains(x.player))
            .Select(x => x.index)
            .OrderBy(x => x)
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            m_RankPlayers[indices[i]] = sorted[i];
        }
    }

    // 指定グループをm_RankPlayersの先頭に持ってくる
    private void SetWinnersToTop(List<MainPlayer> winners)
    {
        var losers = m_RankPlayers.Where(p => !winners.Contains(p)).ToList();
        m_RankPlayers = new List<MainPlayer>();
        m_RankPlayers.AddRange(winners);
        m_RankPlayers.AddRange(losers);
    }
    private void Update_MoveToRank()
    {
        m_MoveTimer += Time.deltaTime;
        float t = Mathf.Clamp01(m_MoveTimer / m_MoveTime);

        for (int i = 0; i < m_ResultPlayers.Count; i++)
        {
            var player = m_ResultPlayers[i];
            Vector3 startPos = player.transform.position;
            Vector3 endPos;
            if (m_CurrentPlayGame.m_GameType == GameType.ONE_ONE)
            {
                endPos = GetRankPos(m_RankPlayers.FindIndex(p => (int)p.Type == player.GetPlayerNum()) + 1);
            }
            else
            {
                endPos = GetRankPos(m_RankPlayers.FindIndex(p => (int)p.Type == player.GetPlayerNum()));
            }

            player.transform.position = Vector3.Lerp(startPos, endPos, t);
            player.transform.LookAt(Camera.main.transform.position, Vector3.up); // カメラ方向
        }
        if (t >= 1f)
        {
            for (int i = 0; i < m_RankPlayers.Count; i++)
            {
                if (m_CurrentPlayGame.m_GameType == GameType.ONE_ONE)
                {
                    m_RankImages[i + 1].sprite = m_RankSprites[i];
                }
                else
                {
                    m_RankImages[i].sprite = m_RankSprites[i];
                }
            }
            m_CurrentUpdateState = UpdateState.FINISH;
        }
    }
    private void Update_Finish()
    {
        if (m_CurrentPlayGame.m_GameType == GameType.ONE_ONE)
        {
            m_RankList = new Dictionary<int, MainPlayer>
            {
                { 1,m_RankPlayers[0]},
                { 2,m_RankPlayers[1]},
            };
        }
        else
        {
            m_RankList = new Dictionary<int, MainPlayer>
            {
                { 1,m_RankPlayers[0]},
                { 2,m_RankPlayers[1]},
                { 3,m_RankPlayers[2]},
                { 4,m_RankPlayers[3]},
            };
        }

        InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.A, InputIconDisplayer.PositionType.MIGAME_END);
        isInputNpc = false;
        m_CurrentUpdateState = UpdateState.RESULT;
    }
    private void Update_Result()
    {
        if (IsInputPressed())
        {
            // ランクを確定させる
            m_IsDesideRank = true;
            InputIconDisplayer.instance.HideAllIcon();
        }
    }
    //メインのゲームがm_IsDesideRankを見てゲームを終わらせる流れ
    public void MiniGameEnd()
    {
        //ミニゲーム終了
        Destroy(m_CurrentPlayGame.gameObject);
        PlayerManager.instance.IsJoined(true);
        foreach (var resultPlayer in m_ResultPlayers)
        {
            Destroy(resultPlayer.gameObject);
        }
        ResetMiniGameManagerData();

        // カメラはすでに切り替わっている
    }

    bool isInputNpc;
    private bool IsInputPressed()
    {
        bool hasHuman = false;

        // 非NPCの入力チェック
        for (int i = 0; i < m_MiniGameJoinPlayers.Count; i++)
        {
            var player = m_MiniGameJoinPlayers[i];
            if (!player.IsNpc) // 人間プレイヤー
            {
                hasHuman = true;
                if (player.Input.actions["A_Decision"].WasReleasedThisFrame())
                {
                    return true;
                }
            }
        }

        // 人間がいない場合はtrue
        if (!hasHuman)
        {
            CoroutineRunner.instance.DelayedCallOnce("MinigameEndTrue", 2.0f, () => isInputNpc = true);
        }
        return isInputNpc;
    }

    private Vector3 GetRankPos(int n)
    {
        return Camera.main.transform.TransformPoint(m_RankOffsetPositions[n]);
    }

    private IEnumerator FadeOutText(Text targetText, float duration = 1f)
    {
        targetText.gameObject.SetActive(true);
        Color targetTextColor = targetText.color;

        ; // フェードアウト時間
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            targetText.color = new Color(targetTextColor.r, targetTextColor.g, targetTextColor.b, alpha);
            yield return null;
        }
        // 非表示
        targetText.color = targetTextColor;
        targetText.gameObject.SetActive(false);
    }

    public IEnumerator ShowFinishText()
    {
        m_FinishText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        m_FinishText.gameObject.SetActive(false);
    }
}
