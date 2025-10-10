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
                // �v���C���[��MAXCOUNT�����͂�����Q�[���I��������
                if (!playerDO.IsFinished && playerDO.DarumaDestroyCount >= MAXCOUNT)
                {
                    playerDO.SetFinished(true);

                    playerBase.Player.rank = totalPlayers - finishedPlayer;
                    finishedPlayer++;
                }
            }
            yield return null;
        }
        // �Q�[���I������
        CoroutineRunner.instance.RunCoroutine(FinishCoroutine());
    }

    // �Q�[���I������
    private IEnumerator FinishCoroutine()
    {
        yield return MiniGameManager.instance.ShowFinishText();
        // �Ō�Ƀ~�j�Q�[���I��
        base.Finish();
        Debug.Log("����܃t�B�j�b�V��");
    }
}
