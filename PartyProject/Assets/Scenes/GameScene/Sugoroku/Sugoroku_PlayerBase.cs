using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sugoroku_PlayerBase : PlayerBase
{
    private int m_PlayerNumber;
    private bool m_IsTurnEnd;
    public bool IsTurnEnd() { return m_IsTurnEnd; }

    private Cell m_CurrentCell;
    private int m_CurrentCellNum;
    private Vector3 m_CellOffsetPos;
    private Func<bool> m_InputAction;
    private bool m_IsNpcInput;

    private enum TurnState
    {
        DICE,
        MOVE,
        CELL,
        END,
    }
    private Dictionary<TurnState, Action> m_TurnActions;
    private TurnState m_CurrentTurnState;
    private enum DiceState
    {
        ROLL_DICE,
        ROLL_ZORO,
        DICE_END,
    }
    private Dictionary<DiceState, Action> m_DiceActions;
    private DiceState m_CurrentDiceState;
    private int m_Rank;
    private int m_SumDiceNum;
    private DiceController m_WhiteDice;
    private DiceController m_RankDice;
    private DiceController m_ZoroDice;
    private int m_WhiteDiceResult;
    private int m_RankDiceResult;
    private int m_ZoroDiceResult;

    private int m_MoveCellNumber;
    private const float MOVE_DURATION = 0.5f;  // 1�}�X������̈ړ�����

    private bool m_IsLastBarrier;

    private void Awake()
    {
        m_PlayerNumber = 0;

        m_CurrentCell = null;
        m_CurrentCellNum = 0;
        m_CellOffsetPos = Vector3.zero;

        m_TurnActions = new Dictionary<TurnState, Action>
        {
            {TurnState.DICE, Turn_Dice},
            {TurnState.MOVE, Turn_Move},
            {TurnState.CELL, Turn_Cell},
            {TurnState.END, Turn_End},
        };
        m_CurrentTurnState = TurnState.DICE;

        m_DiceActions = new Dictionary<DiceState, Action>
        {
            {   DiceState.ROLL_DICE,Dice_RollDice  },
            {   DiceState.ROLL_ZORO,Dice_RollZoro  },
            {   DiceState.DICE_END,Dice_End  },
        };
        m_CurrentDiceState = DiceState.ROLL_DICE;
        m_Rank = 0;
        m_SumDiceNum = 0;
        m_WhiteDice = null;
        m_RankDice = null;
        m_ZoroDice = null;
        m_WhiteDiceResult = 0;
        m_RankDiceResult = 0;
        m_ZoroDiceResult = 0;

        m_MoveCellNumber = 0;

        m_InputAction = null;
        m_IsNpcInput = false;

        m_IsLastBarrier = false;
    }

    private void Start()
    {
        SetCurrentCell(0);
        SetTransformFromCell();
    }

    public bool IsInput()
    {
        if (Player == null) return false;
        if (Player.IsNpc) m_InputAction = InputCPU;
        else m_InputAction = InputPlayer;
        return m_InputAction?.Invoke() ?? false;
    }

    //Player���������̏���
    private bool InputPlayer()
    {
        return Player.Input.actions["X_Action"].WasReleasedThisFrame();
    }
    //Npc���������̏���
    private bool InputCPU()
    {
        CoroutineRunner.instance.DelayedCallOnce("IsInput", 0.5f, () => m_IsNpcInput = true);
        if (m_IsNpcInput)
        {
            m_IsNpcInput = false;
            return true;
        }
        else
        {
            return false;
        }
    }
    public void TurnStart()
    {
        m_IsTurnEnd = false;
        if (m_IsLastBarrier)
        {
            // �ŏI�֖傩��^�[�����X�^�[�g����Ƃ���
            // �ŏ�����֖�̃`�������W������
            m_CurrentTurnState = TurnState.CELL;
            Barrier_Last barrier_Last = m_CurrentCell.GetComponent<Barrier_Last>();
            if (barrier_Last != null)
            {
                barrier_Last.SetUsePlayer(this);
                barrier_Last.SetRankDice(m_Rank);
            }
        }
        else
        {
            m_CurrentTurnState = TurnState.DICE;
            m_CurrentDiceState = DiceState.ROLL_DICE;
            m_SumDiceNum = 0;
        }
    }
    public void TurnProcess()
    {
        if (m_TurnActions.TryGetValue(m_CurrentTurnState, out var action))
        {
            action.Invoke();
        }
    }
    private void Turn_Dice()
    {
        if (m_DiceActions.TryGetValue(m_CurrentDiceState, out var action))
        {
            action.Invoke();
        }
    }
    private void Turn_Move()
    {
        CoroutineRunner.instance.RunOnce("PlayerMove", Move());
    }
    private void Turn_Cell()
    {
        // �~�܂��Ă���}�X�̌���
        m_CurrentCell.Play();
    }
    private void Turn_End()
    {
        // �^�[���I��
        m_MoveCellNumber = 0;
        CoroutineRunner.instance.DelayedCallOnce("TurnEnd", 1.0f, () => m_IsTurnEnd = true);
    }
    public void SetPlayerNumber(int num)
    {
        m_PlayerNumber = num;
    }
    public int GetPlayerNum()
    {
        return m_PlayerNumber;
    }
    public void SetRank(int rank)
    {
        m_Rank = rank;
    }
    public void EndCellAction()
    {
        m_CurrentTurnState = TurnState.END;
    }
    public void SetOffsetPos(Vector3 offsetPos)
    {
        m_CellOffsetPos = offsetPos;
    }
    public Vector3 GetOffsetPos()
    {
        return m_CellOffsetPos;
    }
    private void Dice_RollDice()
    {
        CoroutineRunner.instance.RunOnce("RollDice", RollDice());
    }
    private void Dice_RollZoro()
    {
        CoroutineRunner.instance.RunOnce("RollZoro", RollZoro());
    }
    private void Dice_End()
    {
        m_SumDiceNum = m_WhiteDiceResult + m_RankDiceResult + m_ZoroDiceResult;
        m_CurrentTurnState = TurnState.MOVE;

        Destroy(m_WhiteDice.gameObject);
        if (m_RankDice)
        {
            Destroy(m_RankDice.gameObject);
        }
        if (m_ZoroDice)
        {
            Destroy(m_ZoroDice.gameObject);
        }

        m_WhiteDiceResult = 0;
        m_RankDiceResult = 0;
        m_ZoroDiceResult = 0;
    }
    private IEnumerator RollDice()
    {
        // �J�����������I���܂őҋ@
        yield return CameraManager.instance.WaitCameraMove();

        // �T�C�R���̃Z�b�g�A�b�v
        m_WhiteDice = DiceManager.instance.GetDice((int)DiceManager.DiceType.WHITE);
        m_WhiteDice.gameObject.SetActive(true);
        if (m_Rank == 4)
        {
            m_WhiteDice.SetTransform(DiceManager.DicePositionType.SUGOROKU_MIDDLE);
        }
        else
        {
            m_RankDice = DiceManager.instance.GetDice(m_Rank, DiceManager.DicePositionType.SUGOROKU_TWO_RIGHT);
            m_RankDice.gameObject.SetActive(true);

            m_WhiteDice.SetTransform(DiceManager.DicePositionType.SUGOROKU_TWO_LEFT);
        }
        InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.X, InputIconDisplayer.PositionType.DICE, Player);
        // IsInput() �� true �ɂȂ�܂őҋ@
        yield return new WaitUntil(() => IsInput());

        InputIconDisplayer.instance.HideAllIcon();

        m_WhiteDice.RollDice();
        m_WhiteDiceResult = m_WhiteDice.GetDiceNum();
        if (m_RankDice)
        {
            m_RankDice.RollDice();
            m_RankDiceResult = m_RankDice.GetDiceNum();
        }

        // �T�C�R������]���Ȃ�~�܂�܂őҋ@
        yield return new WaitUntil(() => m_WhiteDice.GetIsRolling() == false);

        // �]���ڂȂ�ǉ�����
        if (m_RankDice && m_WhiteDiceResult == m_RankDiceResult)
        {
            m_CurrentDiceState = DiceState.ROLL_ZORO;
        }
        else
        {
            m_CurrentDiceState = DiceState.DICE_END;
        }
    }
    private IEnumerator RollZoro()
    {
        // �����҂�
        yield return new WaitForSeconds(0.5f);

        // �T�C�R���̈ړ�
        CoroutineRunner.instance.RunCoroutine(
            m_WhiteDice.MoveDiceToPosition(DiceManager.DicePositionType.SUGOROKU_LEFT));
        yield return m_RankDice.MoveDiceToPosition(DiceManager.DicePositionType.SUGOROKU_RIGHT);
        // ����ڗp�𐶐�
        m_ZoroDice = DiceManager.instance.GetDice((int)DiceManager.DiceType.WHITE, DiceManager.DicePositionType.SUGOROKU_MIDDLE);
        m_ZoroDice.gameObject.SetActive(true);

        InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.X, InputIconDisplayer.PositionType.DICE, Player);
        // IsInput() �� true �ɂȂ�܂őҋ@
        yield return new WaitUntil(() => IsInput());
        InputIconDisplayer.instance.HideAllIcon();

        m_ZoroDice.RollDice();
        m_ZoroDiceResult = m_ZoroDice.GetDiceNum();

        // �T�C�R������]���Ȃ�~�܂�܂őҋ@
        yield return new WaitUntil(() => m_ZoroDice.GetIsRolling() == false);

        m_CurrentDiceState = DiceState.DICE_END;
    }

    private IEnumerator Move()
    {
        m_MoveCellNumber = m_SumDiceNum;
        yield return MoveCell(false);
        m_CurrentCell = SugorokuStage.instance.GetCell(m_CurrentCellNum);
        // ���̃}�X�Ɏ~�܂��Ă��錻�݂̃v���C���[��ۑ�
        m_CurrentCell.SetUsePlayer(this);
        m_CurrentTurnState = TurnState.CELL;
    }
    private void SetTransformFromCell()
    {
        transform.position = m_CurrentCell.transform.TransformPoint(m_CellOffsetPos);
        Vector3 euler = transform.eulerAngles;
        euler.y = m_CurrentCell.transform.eulerAngles.y;
        transform.eulerAngles = euler;
    }
    public void SetCurrentCell(int num)
    {
        m_CurrentCellNum = num;
        m_CurrentCell = SugorokuStage.instance.GetCell(m_CurrentCellNum);
        m_IsLastBarrier = m_CurrentCellNum == 52;
    }
    public int GetCurrentCellNum()
    {
        return m_CurrentCellNum;
    }
    private IEnumerator MoveToCellCoroutine(int cellNum)
    {
        var cell = SugorokuStage.instance.GetCell(cellNum);
        if (cell == null)
        {
            Debug.Log(cellNum);
            Debug.Log("�Z���Ȃ��I");
            yield break;
        }

        Vector3 start = transform.position;
        Vector3 end = cell.transform.TransformPoint(m_CellOffsetPos);

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0, cell.transform.eulerAngles.y, 0);

        float elapsed = 0f;

        while (elapsed < MOVE_DURATION)
        {
            float t = elapsed / MOVE_DURATION;
            transform.SetPositionAndRotation(Vector3.Lerp(start, end, t), Quaternion.Lerp(startRot, endRot, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.SetPositionAndRotation(end, endRot);
    }
    public IEnumerator MoveCell(bool stopBeforeBarrier = true)
    {
        for (; ; )
        {
            if (m_MoveCellNumber == 0)
            {
                // �ړ��I��
                break;
            }

            int move = 0;

            if (m_MoveCellNumber > 0)
            {
                // �O�ɐi��
                move = 1;
            }
            else if (m_MoveCellNumber < 0)
            {
                // ���ɐi�ށB������0�Ȃ瓮���Ȃ�
                if (m_CurrentCellNum == 0)
                {
                    m_MoveCellNumber = 0;
                    break;
                }
                move = -1;
            }

            int nextCell = m_CurrentCellNum + move;

            // �ړ��悪�֖�Z�����o���A���L���Ȃ珈��
            if (SugorokuStage.instance.IsBarrierCell(nextCell))
            {
                // �֖�̏����擾
                var barrier = SugorokuStage.instance.GetCell(nextCell).GetComponent<BarrierBase>();
                if (barrier != null && barrier.IsExistence())
                {
                    if (stopBeforeBarrier)
                    {
                        // �֖�ɓ���O�ŏI��
                        break;
                    }
                    else
                    {
                        // �֖�ɓ����Ă���I��
                        yield return MoveToCellCoroutine(nextCell);
                        AudioManager.instance.PlaySE("Sugoroku", "Walk");

                        m_CurrentCellNum = nextCell;
                        m_MoveCellNumber -= move;

                        if (nextCell == 52)
                        {
                            m_IsLastBarrier = true;
                        }
                        break;
                    }
                }
                // �֖傪�����Ȃ疳�����đ��s
            }

            // �ʏ�ړ�
            yield return MoveToCellCoroutine(nextCell);
            AudioManager.instance.PlaySE("Sugoroku", "Walk");

            // �J�E���g�X�V
            m_CurrentCellNum = nextCell;
            m_MoveCellNumber -= move;
        }

        // �Ō�ɃJ�����ҋ@
        yield return CameraManager.instance.WaitCameraMove();
    }
    public void SetMoveCellNum(int num)
    {
        m_MoveCellNumber = num;
    }
    public int GetMoveCellNum()
    {
        return m_MoveCellNumber;
    }
}
