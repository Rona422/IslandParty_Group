using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier_2 : BarrierBase
{
    protected override IEnumerator BarrierFlowCoroutine()
    {
        yield return CellExplanation.instance.ShowFadeExplanation("2nd.ƒ`ƒƒƒŒƒ“ƒW");
        yield return Event_Sugoroku.instance.PlayEvent(Event_Sugoroku.EventObject.BARRIER_TWO, Event_Sugoroku.EventState.START);
        yield return End();
    }
    private IEnumerator End()
    {
        switch (UnityEngine.Random.Range(0, 2))
        {
            case 0:
                yield return Event_Sugoroku.instance.PlayEvent(Event_Sugoroku.EventObject.BARRIER_TWO, Event_Sugoroku.EventState.WIN);
                break;
            case 1:
                yield return Event_Sugoroku.instance.PlayEvent(Event_Sugoroku.EventObject.BARRIER_TWO, Event_Sugoroku.EventState.LOSE);
                m_Player.SetMoveCellNum(-1);
                break;
        }
        CameraManager.instance.SwitchToSugorokuCam((CameraManager.SugorokuCameraType)m_Player.GetPlayerNum());
        yield return new WaitForSeconds(0.1f);
        yield return m_Player.MoveCell();
        m_Player.EndCellAction();
    }
}
