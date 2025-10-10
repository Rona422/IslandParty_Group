using System.Collections;
using UnityEngine;


public class GameDirector_DarumaOtoshi : MiniGameBase
{
    private readonly int MAXCOUNT = 100;

    void Start()
    {
        CoroutineRunner.instance.RunCoroutine(PlayCoroutine());
    }

    private IEnumerator PlayCoroutine()
    {
        int finishedPlayer = 0;
        int totalPlayers = m_MiniGamePlayers.Count;

        while (finishedPlayer < totalPlayers)
        {
            foreach (var playerBase in m_MiniGamePlayers)
            {
                var playerDO = playerBase.GetComponent<Player_DarumaOtoshi>();
                if (playerDO == null) continue;
                // プレイヤーがMAXCOUNT分入力したらゲーム終了処理へ
                if (!playerDO.IsFinished && playerDO.DarumaDestroyCount >= MAXCOUNT)
                {
                    playerDO.SetFinished(true);

                    playerBase.Player.rank = totalPlayers - finishedPlayer;
                    finishedPlayer++;
                }
            }
            yield return null;
        }
        // ゲーム終了処理
        CoroutineRunner.instance.RunCoroutine(FinishCoroutine());
    }

    // ゲーム終了処理
    private IEnumerator FinishCoroutine()
    {
        yield return MiniGameManager.instance.ShowFinishText();
        // 最後にミニゲーム終了
        base.Finish();
        Debug.Log("だるまフィニッシュ");
    }
}
