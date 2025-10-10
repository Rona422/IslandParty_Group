using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceBack : Cell
{
    private enum ActionState
    {
        DICE,
        MOVE,
        END,
    }
    private ActionState m_CurrentState;


    private void Start()
    {
        m_CurrentState = ActionState.DICE;
    }

    private void Update()
    {
    }

    override public void Play()
    {
        switch (m_CurrentState)
        {
            case ActionState.DICE:
                CoroutineRunner.instance.RunOnce("DiceBack_Dice", Dice());
                break;
            case ActionState.MOVE:
                CoroutineRunner.instance.RunOnce("DiceBack_Move", Move());
                break;
            case ActionState.END:
                m_CurrentState = ActionState.DICE;
                m_Player.EndCellAction();
                break;
            default:
                break;
        }
    }

    private IEnumerator Dice()
    {
        DiceController dice = DiceManager.instance.GetDice((int)DiceManager.DiceType.WHITE, DiceManager.DicePositionType.SUGOROKU_MIDDLE);
        dice.gameObject.SetActive(true);

        InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.X, InputIconDisplayer.PositionType.DICE, m_Player.Player);
        yield return new WaitUntil(() => m_Player.IsInput());
        InputIconDisplayer.instance.HideAllIcon();

        dice.RollDice();
        yield return new WaitUntil(() => dice.GetIsRolling() == false);
        AudioManager.instance.PlaySE("Sugoroku", "BackDice");
        m_Player.SetMoveCellNum(-dice.GetDiceNum());
        Destroy(dice.gameObject);
        m_CurrentState = ActionState.MOVE;
    }
    private IEnumerator Move()
    {
        yield return m_Player.MoveCell();
        m_CurrentState = ActionState.END;
    }
}
