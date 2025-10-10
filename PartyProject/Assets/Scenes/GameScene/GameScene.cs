using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    void Start()
    {
        AudioManager.instance.PlayBGM("Sugoroku", "StartSugorokuBGM");
        CoroutineRunner.instance.RunCoroutine(PlayersUi.instance.StateReflection(PlayersUi.PlayersUiState.STANDBY));
    }
    void Update()
    {

    }
}
