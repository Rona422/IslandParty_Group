using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forward5 : Cell
{
    override public void Play()
    {
        CoroutineRunner.instance.RunOnce("Forward5", Move());
    }

    private IEnumerator Move()
    {
        AudioManager.instance.PlaySE("Sugoroku", "Forward");
        m_Player.SetMoveCellNum(5);
        yield return m_Player.MoveCell();
        m_Player.EndCellAction();
    }
}
