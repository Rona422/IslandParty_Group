using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Back5 : Cell
{
    override public void Play()
    {
        CoroutineRunner.instance.RunOnce("Back5_Move", Move());
    }

    private IEnumerator Move()
    {
        AudioManager.instance.PlaySE("Sugoroku", "Back");
        m_Player.SetMoveCellNum(-5);
        yield return m_Player.MoveCell();
        m_Player.EndCellAction();
    }
}

