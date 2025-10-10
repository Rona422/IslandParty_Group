using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier_1 : BarrierBase
{
    private int m_ResultDice;
    protected override IEnumerator BarrierFlowCoroutine()
    {
        m_ResultDice = 0;
        yield return Event_Sugoroku.instance.PlayEvent(Event_Sugoroku.EventObject.BARRIER_ONE, Event_Sugoroku.EventState.START);
        yield return CellExplanation.instance.ShowFadeExplanation("1st.チャレンジ");
        yield return CellExplanation.instance.ShowWaitExplanation(
            "サイコロを振って、出た目の数が\r\n3以上ならチャレンジクリア", m_Player.Player
            );
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
        if (m_ResultDice >= 3)
        {
            yield return Event_Sugoroku.instance.PlayEvent(Event_Sugoroku.EventObject.BARRIER_ONE, Event_Sugoroku.EventState.WIN);
            
        }
        else
        {
            yield return Event_Sugoroku.instance.PlayEvent(Event_Sugoroku.EventObject.BARRIER_ONE, Event_Sugoroku.EventState.LOSE);
            m_Player.SetMoveCellNum(-1);
        }
        CameraManager.instance.SwitchToSugorokuCam((CameraManager.SugorokuCameraType)m_Player.GetPlayerNum());
        yield return m_Player.MoveCell();
        // m_Player.EndCellAction(); の直前でfalseにする
        if (m_ResultDice >= 3)  m_IsExistence = false;
        m_Player.EndCellAction();
    }
}