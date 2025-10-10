using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier_3 : BarrierBase
{
    private int m_ResultDice;
    private int m_Count;
    protected override void Start()
    {
        base.Start();
        m_Count = 0;
    }
    protected override IEnumerator BarrierFlowCoroutine()
    {
        yield return CellExplanation.instance.ShowFadeExplanation("3rd.チャレンジ");
        yield return CellExplanation.instance.ShowWaitExplanation(
            $"サイコロを振って、出た目の数が\r\n{5-m_Count}以上ならチャレンジクリア", m_Player.Player
        );
        m_ResultDice = 0;
        yield return Event_Sugoroku.instance.PlayEvent(Event_Sugoroku.EventObject.BARRIER_THREE, Event_Sugoroku.EventState.START);
        CameraManager.instance.SwitchToSugorokuCam((CameraManager.SugorokuCameraType)m_Player.GetPlayerNum());
        yield return new WaitForSeconds(0.5f);
        yield return DiceRoll();
        yield return End();
    }
    private IEnumerator DiceRoll()
    {
        DiceController dice = DiceManager.instance.GetDice((int)DiceManager.DiceType.WHITE, DiceManager.DicePositionType.SUGOROKU_MIDDLE);
        dice.gameObject.SetActive(true);

        InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.X, InputIconDisplayer.PositionType.DICE, m_Player.Player);
        yield return new WaitUntil(() => m_Player.IsInput());
        InputIconDisplayer.instance.HideAllIcon();
        dice.RollDice();
        m_ResultDice += dice.GetDiceNum();

        yield return new WaitUntil(() => dice.GetIsRolling() == false);

        yield return new WaitForSeconds(1.5f);
        Destroy(dice.gameObject);
    }
    private IEnumerator End()
    {
        yield return new WaitForSeconds(1.0f);
        if (m_ResultDice >= 5 - m_Count)
        {
            yield return Event_Sugoroku.instance.PlayEvent(Event_Sugoroku.EventObject.BARRIER_THREE, Event_Sugoroku.EventState.WIN);
            
        }
        else
        {
            yield return Event_Sugoroku.instance.PlayEvent(Event_Sugoroku.EventObject.BARRIER_THREE, Event_Sugoroku.EventState.LOSE);
            m_Count++;
            m_Player.SetMoveCellNum(0);
        }
        yield return m_Player.MoveCell();
        // m_Player.EndCellAction(); の直前でfalseにする
        if (m_ResultDice >= 5 - m_Count) m_IsExistence = false;
        m_Player.EndCellAction();
    }
}
