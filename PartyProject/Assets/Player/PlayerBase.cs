using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static PlayerManager;
public class PlayerBase : MonoBehaviour
{
    [System.NonSerialized]
    public MainPlayer Player;
    public void SetMainPlayer(MainPlayer _mainPlayer)
    {
        Player = _mainPlayer;
    }
}
