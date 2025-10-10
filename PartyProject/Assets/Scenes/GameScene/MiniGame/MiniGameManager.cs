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
    //�i�s������Q�[���̊Ǘ��p
    private MiniGameBase m_CurrentPlayGame;
    //����Update�̏��
    private UpdateState m_CurrentUpdateState;
    private Dictionary<UpdateState, Action> m_UpdateActions;

    //MainGame�̕����炱��bool�����ă����N�����܂��Ă��邩�ǂ�����������
    private bool m_IsDesideRank;
    public bool GetIsDesideRank()
    {
        return m_IsDesideRank;
    }

    //�~�j�Q�[���ɎQ������v���C���[���X�g
    private List<MainPlayer> m_MiniGameJoinPlayers = new();
    public List<MainPlayer> GetMiniGameJoinPlayers() { return m_MiniGameJoinPlayers; }

    //���C���̃Q�[������Q�Ƃ����郉���L���O���X�g
    protected Dictionary<int, MainPlayer> m_RankList = new();
    public Dictionary<int, MainPlayer> GetRankList()
    {
        return m_RankList;
    }

    // �J�E���g�_�E��
    [SerializeField] private Text m_CountDownText;
    private float m_CurrentCountDownTime;
    private float m_CountDownTime = 3.0f;
    // Start�̃e�L�X�g
    [SerializeField] private Text m_StartText;
    // Finish�̃e�L�X�g
    [SerializeField] private Text m_FinishText;

    //�����L���O���ɕ��ׂ�p���X�g
    private List<MainPlayer> m_RankPlayers = new();
    private class DiceDrawGroup
    {
        public List<MainPlayer> groupA = new();
        public List<MainPlayer> groupB = new(); // ONE_THREE �p�i3�l����\ or ����j
        public GameType gameType;
        public bool isResolved = false;
        // �������Ɏ����ň�ӂ�ID�����
        public string groupId { get; private set; } = Guid.NewGuid().ToString();
    }
    private List<DiceDrawGroup> m_DiceDrawGroups = new();
    private int m_InputWaitCount;

    // �v���C���[�\���p
    private List<Sugoroku_PlayerBase> m_ResultPlayers = new();
    private List<Vector3> m_RankOffsetPositions = new();
    private float m_MoveTime = 1.5f;
    private float m_MoveTimer = 0f;
    [SerializeField] private List<Image> m_RankImages = new ();
    [SerializeField] private List<Sprite> m_RankSprites = new ();

    // �w�i��Image
    [SerializeField] private Image m_BackGroundUI;
    // �~�j�Q�[���^�C�g��UI
    [SerializeField] private Image m_TitleBack;
    [SerializeField] private Text m_TitleText;
    // �V�ѕ�UI�̐e
    [SerializeField] private GameObject m_HowToPlayUI;
    // �V�ѕ�UI
    [SerializeField] private Image m_PlayImage;
    [SerializeField] private Text m_ExplanationText;
    [SerializeField] private Text m_HowToOperateText;
    // �����������b�Z�[�W
    [SerializeField] private Image m_DrawMessage;
    protected override void Awake()
    {
        base.Awake();
        //�L�[��Update�֐��̊֘A�t��������
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


        // ���ʕ\���p�|�W�V�����i������E�ցj
        m_RankOffsetPositions = new List<Vector3>
        {
        new (-0.6f, 0, 1.2f), // 1��
        new (-0.2f, 0, 1.2f), // 2��
        new ( 0.2f, 0, 1.2f), // 3��
        new ( 0.6f, 0, 1.2f), // 4��
        };

        m_CountDownText.gameObject.SetActive(false);
        m_StartText.gameObject.SetActive(false);
        m_FinishText.gameObject.SetActive(false);
        // Start�e�L�X�g�̐F��ێ�
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

    //���C���̃Q�[���̕����炱�̊֐����Ăу~�j�Q�[�����J�n������
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
                default: Debug.Log("�~�j�Q�[���̎�ނ��������ˁH"); break;
            }
            if (_MiniGameJoinPlayers.Count != _PlayerMax)
            {
                Debug.Log("�~�j�Q�[���̃v���C�l���ƎQ���l��������ւ��");
            }
            //�~�j�Q�[���ɎQ������v���C���[������
            m_MiniGameJoinPlayers = new(_MiniGameJoinPlayers);
            //�~�j�Q�[������
            m_CurrentPlayGame = Instantiate(_StartGame, gameObject.transform);
            //�~�j�Q�[���̏�����S�Ē�~����
            m_CurrentPlayGame.gameObject.SetActive(true);
            m_CurrentPlayGame.SetFrozen(true);
            // �~�j�Q�[���̃v���C���[������
            m_CurrentPlayGame.InitPlayers();

            // �~�j�Q�[���̃e�L�X�g�ݒ�
            m_TitleText.text = m_CurrentPlayGame.m_GameTitle;
            m_PlayImage.sprite = m_CurrentPlayGame.m_PlayImage;
            m_ExplanationText.text = m_CurrentPlayGame.m_GameTutorial;
            m_HowToOperateText.text = m_CurrentPlayGame.m_GameControl;

            CameraManager.instance.SetMiniGameCamera(m_CurrentPlayGame.GetMiniGameVCam());
            CameraManager.instance.SwitchToMiniGameCam();

            //Update��Ԃ��X�V
            m_CurrentUpdateState = UpdateState.HOW_TO_PLAY;
        }
        else
        {
            Debug.Log("�܂������Ă�Q�[��������݂���>m<");
        }
    }

    public void MiniGameUpdate()
    {
        //m_CurrentUpdateState�ɑΉ�����֐�����
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
        // UI�̕\��
        m_BackGroundUI.gameObject.SetActive(true);
        m_TitleBack.gameObject.SetActive(true);
        m_HowToPlayUI.SetActive(true);
        InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.A, InputIconDisplayer.PositionType.MIGAME_HOWTOPLAY);
        CoroutineRunner.instance.RunCoroutine(PlayersUi.instance.StateReflection(PlayersUi.PlayersUiState.MINIGAME_START));
        //int playerCount = m_MiniGameJoinPlayers.Count;
        //List<bool> playerInputList = new(playerCount);
        //bool npcTriggered = false; // NPC���͒x�����J�n�������ǂ���

        //// ������
        //for (int i = 0; i < playerCount; i++)
        //{
        //    playerInputList.Add(false);
        //}

        //while (true)
        //{
        //    // �l�ԃv���C���[�̓��͂��`�F�b�N
        //    for (int i = 0; i < playerCount; i++)
        //    {
        //        if (m_MiniGameJoinPlayers[i].IsNpc)
        //            continue; // NPC�͂܂�����

        //        if (!playerInputList[i] &&
        //            m_MiniGameJoinPlayers[i].Input.actions["X_Action"].WasReleasedThisFrame())
        //        {
        //            playerInputList[i] = true;
        //            Debug.Log(i + "������k");
        //        }
        //    }

        //    // ���ׂĂ̐l�ԃv���C���[�����͍ς݂�����
        //    bool allHumansInput = true;
        //    for (int i = 0; i < playerCount; i++)
        //    {
        //        if (!m_MiniGameJoinPlayers[i].IsNpc && !playerInputList[i])
        //        {
        //            allHumansInput = false;
        //            break;
        //        }
        //    }

        //    // �܂�NPC���͒x�������J�n���ĂȂ���΁A�l�ԑS�����͍ς݂ŊJ�n
        //    if (allHumansInput && !npcTriggered)
        //    {
        //        npcTriggered = true;
        //        for (int i = 0; i < playerCount; i++)
        //        {
        //            if (m_MiniGameJoinPlayers[i].IsNpc && !playerInputList[i])
        //            {
        //                int index = i; // �N���[�W���΍�
        //                CoroutineRunner.instance.DelayedCall(0.5f, () =>
        //                {
        //                    playerInputList[index] = true;
        //                    Debug.Log(index + "������k");

        //                });
        //            }
        //        }
        //    }

        //    // �S�����͍ς݂Ȃ�I��
        //    if (playerInputList.All(pressed => pressed))
        //        break;

        //    yield return null;
        //}
        //�S���̓��͑҂�
        yield return CoroutineRunner.instance.RunCoroutine(PlayersUi.instance.IsAllCheck());
        yield return new WaitForSeconds(1.0f);
        CoroutineRunner.instance.RunCoroutine(PlayersUi.instance.StateReflection(PlayersUi.PlayersUiState.STANDBY));

        // UI�̔�\��
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
            // �^�C�}�[����
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
        //�~�j�Q�[���̏������ĊJ����
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
        // �~�j�Q�[�����~
        m_CurrentPlayGame.SetFrozen(true);
        // �~�j�Q�[���̃J�������~�܂��Ă��܂��̂ŋN������
        m_CurrentPlayGame.GetMiniGameVCam().enabled = true;

        // UI�\��
        m_BackGroundUI.gameObject.SetActive(true);
        m_TitleBack.gameObject.SetActive(true);

        // ���U���g�쐬
        SetupResult();

        yield return new WaitForSeconds(1.0f);

        // �_�C�X���o���A���ʉ�ʂ֑J��
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
        // ���U���g��ʔw�i���Z�b�g
        m_BackGroundUI.gameObject.SetActive(true);

        // �~���\�[�g�i�傫��������̑O��j
        m_RankPlayers = m_MiniGameJoinPlayers
            .OrderByDescending(p => p.rank)
            .ToList();

        // ��r�p�Ɍ���rank�l��ۑ����Ă���
        List<int> originalRanks = m_RankPlayers.Select(p => p.rank).ToList();

        // ���ʂ̍Ċ��蓖��
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
        // �v���C���[�̃R�s�[�쐬
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
            .OrderByDescending(p => p.rank)  // �X�R�A���ɍ��������O
            .ThenBy(p => (int)p.Type)        // ���_�Ȃ�v���C���[�ԍ���
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

            // �J���������i�����j
            Vector3 toCam = Camera.main.transform.position - copy.transform.position;
            toCam.y = 0;
            if (toCam != Vector3.zero)
                copy.transform.rotation = Quaternion.LookRotation(toCam);
        }

        for (int i = 0; i < m_RankPlayers.Count; i++)
        {
            int rank = 4 - m_RankPlayers[i].rank; // �K��0~3
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
                // ��rank�v���C���[�O���[�v�����W
                var drawGroups = m_RankPlayers
                    .GroupBy(p => p.rank)
                    .Where(g => g.Count() >= 2)
                    .ToList();

                // �����O���[�v�Ή��i��F1,1,3,3�j
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
        // �������̃O���[�v�����ׂĎ擾
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
                $"resolve_draw_{group.groupId}",   // �� �C���f�b�N�X�ł͂Ȃ����j�[�NID
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
                allParticipants.Add(group.groupA[0]); // ��l��
                allParticipants.Add(group.groupB[0]); // ��\
                break;

            case GameType.ONE_ALL:
                allParticipants.AddRange(group.groupA); // �S��
                break;
        }

        // �o�ڂ��V���b�t��
        List<int> shuffledNumbers = Enumerable.Range(1, 6)
                                              .OrderBy(_ => UnityEngine.Random.value)
                                              .ToList();

        Dictionary<MainPlayer, int> playerDiceResults = new();
        Dictionary<MainPlayer, DiceController> playerDiceControllers = new();
        HashSet<MainPlayer> rolledPlayers = new();

        // �_�C�X��S�����������ĕ��ׂĂ���
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


        // �S�����U��܂ő҂��[�v
        while (rolledPlayers.Count < allParticipants.Count)
        {
            foreach (var player in allParticipants)
            {
                if (rolledPlayers.Contains(player))
                    continue;

                // ���͂���������U��
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

        // �S�_�C�X���~�܂�܂őҋ@
        yield return new WaitUntil(() => playerDiceControllers.Values.All(d => !d.GetIsRolling()));
        yield return new WaitForSeconds(2.0f);

        // �_�C�X�폜
        foreach (var dice in playerDiceControllers.Values)
            GameObject.Destroy(dice.gameObject);

        // ���ʌ���
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

    // �w��O���[�v��m_RankPlayers�̐擪�Ɏ����Ă���
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
            player.transform.LookAt(Camera.main.transform.position, Vector3.up); // �J��������
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
            // �����N���m�肳����
            m_IsDesideRank = true;
            InputIconDisplayer.instance.HideAllIcon();
        }
    }
    //���C���̃Q�[����m_IsDesideRank�����ăQ�[�����I��点�闬��
    public void MiniGameEnd()
    {
        //�~�j�Q�[���I��
        Destroy(m_CurrentPlayGame.gameObject);
        PlayerManager.instance.IsJoined(true);
        foreach (var resultPlayer in m_ResultPlayers)
        {
            Destroy(resultPlayer.gameObject);
        }
        ResetMiniGameManagerData();

        // �J�����͂��łɐ؂�ւ���Ă���
    }

    bool isInputNpc;
    private bool IsInputPressed()
    {
        bool hasHuman = false;

        // ��NPC�̓��̓`�F�b�N
        for (int i = 0; i < m_MiniGameJoinPlayers.Count; i++)
        {
            var player = m_MiniGameJoinPlayers[i];
            if (!player.IsNpc) // �l�ԃv���C���[
            {
                hasHuman = true;
                if (player.Input.actions["A_Decision"].WasReleasedThisFrame())
                {
                    return true;
                }
            }
        }

        // �l�Ԃ����Ȃ��ꍇ��true
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

        ; // �t�F�[�h�A�E�g����
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            targetText.color = new Color(targetTextColor.r, targetTextColor.g, targetTextColor.b, alpha);
            yield return null;
        }
        // ��\��
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
