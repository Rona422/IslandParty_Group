using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    protected Sugoroku_PlayerBase m_Player;

    public void SetUsePlayer(Sugoroku_PlayerBase player)
    {
        m_Player = player;
    }

    public virtual void Play()
    {
        AudioManager.instance.PlaySE("Sugoroku", "Wait");
        m_Player.EndCellAction();
    }
}
