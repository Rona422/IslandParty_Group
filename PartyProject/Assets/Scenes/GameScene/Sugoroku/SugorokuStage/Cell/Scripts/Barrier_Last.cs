using System.Collections;
using UnityEngine;
public class Barrier_Last : BarrierBase
{
    private DiceController m_RankDice;
    private int m_ResultDice;
    public void SetRankDice(int _rank)
    {
        if (_rank != 4)
        {
            m_RankDice = DiceManager.instance.GetDice(_rank);
            m_RankDice.gameObject.SetActive(true);
        }
    }
    protected override IEnumerator BarrierFlowCoroutine()
    {
        m_RankDice = null;
        m_ResultDice = 0;
        yield return CellExplanation.instance.ShowFadeExplanation("Last.�`�������W");
        yield return CellExplanation.instance.ShowWaitExplanation(
            "�T�C�R����U���āA�o���ڂ̐��̍��v��\r\n�U�ȏ�Ȃ�`�������W�N���A", m_Player.Player
            );
        yield return CoroutineRunner.instance.RunCoroutine(Event_Sugoroku.instance.PlayEvent(Event_Sugoroku.EventObject.BARRIER_LAST, Event_Sugoroku.EventState.START));

        yield return DiceRoll();

        yield return End();
    }

    private IEnumerator DiceRoll()
    {
        DiceController dice = DiceManager.instance.GetDice((int)DiceManager.DiceType.WHITE);
        dice.gameObject.SetActive(true);
        if (m_RankDice)
        {
            m_RankDice.gameObject.SetActive(true);
            dice.SetTransform(DiceManager.DicePositionType.SUGOROKU_TWO_LEFT);
            m_RankDice.SetTransform(DiceManager.DicePositionType.SUGOROKU_TWO_RIGHT);
        }
        else
        {
            dice.SetTransform(DiceManager.DicePositionType.SUGOROKU_MIDDLE);
        }
        InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.X, InputIconDisplayer.PositionType.DICE, m_Player.Player);
        yield return new WaitUntil(() => m_Player.IsInput());
        InputIconDisplayer.instance.HideAllIcon(); 
        dice.RollDice();
        m_ResultDice += dice.GetDiceNum();
        if (m_RankDice)
        {
            m_RankDice.RollDice();
            m_ResultDice += m_RankDice.GetDiceNum();
        }
        yield return new WaitUntil(() => dice.GetIsRolling() == false);

        bool isZoro = m_RankDice != null && dice.GetDiceNum() == m_RankDice.GetDiceNum();
        DiceController zoroDice = null;
        if (isZoro)
        {
            yield return new WaitForSeconds(0.5f);

            // ��������̏ꏊ�ړ�
            CoroutineRunner.instance.RunCoroutine(
                dice.MoveDiceToPosition(DiceManager.DicePositionType.SUGOROKU_LEFT));
            yield return CoroutineRunner.instance.RunCoroutine(
                m_RankDice.MoveDiceToPosition(DiceManager.DicePositionType.SUGOROKU_RIGHT));
            // ����ڗp�𐶐�
            zoroDice = DiceManager.instance.GetDice((int)DiceManager.DiceType.WHITE, DiceManager.DicePositionType.SUGOROKU_MIDDLE);
            zoroDice.gameObject.SetActive(true);

            InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.X, InputIconDisplayer.PositionType.DICE, m_Player.Player);
            // IsInput() �� true �ɂȂ�܂őҋ@
            yield return new WaitUntil(() => m_Player.IsInput());
            InputIconDisplayer.instance.HideAllIcon();
            zoroDice.RollDice();
            m_ResultDice = zoroDice.GetDiceNum();

            // �T�C�R������]���Ȃ�~�܂�܂őҋ@
            yield return new WaitUntil(() => zoroDice.GetIsRolling() == false);
        }

        yield return new WaitForSeconds(1.5f);
        Destroy(dice.gameObject);
        if (m_RankDice)
        {
            Destroy(m_RankDice.gameObject);
            m_RankDice = null;
        }
    }
    private IEnumerator End()
    {
        yield return new WaitForSeconds(1.0f);
        if (m_ResultDice >= 6)
        {
            yield return Event_Sugoroku.instance.PlayEvent(Event_Sugoroku.EventObject.BARRIER_LAST, Event_Sugoroku.EventState.WIN);
            m_IsExistence = false; // �ꉞ
            SugorokuManager.instance.FinishSugoroku();
        }
        yield return Event_Sugoroku.instance.PlayEvent(Event_Sugoroku.EventObject.BARRIER_LAST, Event_Sugoroku.EventState.LOSE);
        m_Player.EndCellAction();
    }
}
