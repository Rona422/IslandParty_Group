using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SwapPlayer : Cell
{

    private enum ActionState
    {
        TEXT,
        DICE,
        SWAP,
        END,
    }
    private Dictionary<ActionState, Action> m_Actions;
    private ActionState m_CurrentActionState;

    private Sugoroku_PlayerBase m_Partner;

    private int m_PlayerCellNum;
    private int m_PartnerCellNum;

    private const float MOVE_DURATION = 1.0f;  // 上昇/下降にかける時間
    private const float JUMP_HEIGHT = 60.0f;   // 上に飛ぶ高さ

    Cinemachine.CinemachineVirtualCamera m_SwapVCam;


    private void Start()
    {
        m_Partner = null;
        m_PlayerCellNum = -1;
        m_PartnerCellNum = -1;
        m_Actions = new Dictionary<ActionState, Action>
        {
            {ActionState.TEXT,Action_Text },
            {ActionState.DICE,Action_Dice },
            {ActionState.SWAP,Action_Swap },
            {ActionState.END,Action_End },
        };
        m_CurrentActionState = ActionState.TEXT;
    }

    override public void Play()
    {
        if (m_Actions.TryGetValue(m_CurrentActionState, out var action))
        {
            action.Invoke();
        }
    }
    private void Action_Text()
    {
        CoroutineRunner.instance.RunOnce("Swap_Text", Text());
    }

    private IEnumerator Text()
    {
        yield return CellExplanation.instance.ShowFadeExplanation("スワップ");

        yield return CellExplanation.instance.ShowWaitExplanation(
            "サイコロを振って、出たライバルと\r\n位置が入れ替わります", m_Player.Player
            );
        m_CurrentActionState = ActionState.DICE;
    }
    private void Action_Dice()
    {
        CoroutineRunner.instance.RunOnce("Swap_Dice", Dice());
    }

    private IEnumerator Dice()
    {
        int playerNum = m_Player.GetPlayerNum();
        DiceController selectPartnerDice = DiceManager.instance.GetDice(playerNum + 4, DiceManager.DicePositionType.SUGOROKU_MIDDLE);
        selectPartnerDice.gameObject.SetActive(true);
        InputIconDisplayer.instance.ShowInputIcon(InputIconDisplayer.InputKey.X, InputIconDisplayer.PositionType.DICE, m_Player.Player);
        yield return new WaitUntil(() => m_Player.IsInput());
        InputIconDisplayer.instance.HideAllIcon();
        selectPartnerDice.RollDice();
        AudioManager.instance.PlaySE("Sugoroku", "Swap");
        yield return new WaitUntil(() => selectPartnerDice.GetIsRolling() == false);
        int swapPartnerNum = selectPartnerDice.GetDiceNum();
        if (playerNum <= swapPartnerNum) swapPartnerNum++;
        m_Partner = SugorokuManager.instance.GetPlayer(swapPartnerNum);
        yield return new WaitForSeconds(1.5f);
        Destroy(selectPartnerDice.gameObject);
        m_CurrentActionState = ActionState.SWAP;
    }
    private void Action_Swap()
    {
        CoroutineRunner.instance.RunOnce("Swap_Swap", Swap());
    }

    private IEnumerator Swap()
    {
        // プレイヤー情報取得
        Transform playerTransform = m_Player.transform;
        m_PlayerCellNum = m_Player.GetCurrentCellNum();
        Cell playerCell = SugorokuStage.instance.GetCell(m_PlayerCellNum);
        Vector3 playerOffsetPos = m_Player.GetOffsetPos();

        // パートナー情報取得
        Transform partnerTransform = m_Partner.transform;
        m_PartnerCellNum = m_Partner.GetCurrentCellNum();
        Cell partnerCell = SugorokuStage.instance.GetCell(m_PartnerCellNum);
        Vector3 partnerOffsetPos = m_Partner.GetOffsetPos();

        Vector3 playerAfterPos = partnerCell.transform.TransformPoint(playerOffsetPos);
        Vector3 partnerAfterPos = playerCell.transform.TransformPoint(partnerOffsetPos);

        yield return new WaitForSeconds(0.5f);

        // カメラをスワップに切り替え
        CameraManager.instance.SwitchToSugorokuCam(CameraManager.SugorokuCameraType.SWAP);
        SetVCamTarget(playerCell.transform);

        yield return new WaitForSeconds(0.5f);

        // プレイヤー → 上昇
        yield return MovePlayer(playerTransform, playerTransform.position, playerTransform.position + Vector3.up * JUMP_HEIGHT);

        yield return new WaitForSeconds(0.5f);

        // カメラの場所変更
        SetVCamTarget(partnerCell.transform);

        yield return new WaitForSeconds(0.5f);

        // プレイヤー → 降下
        playerTransform.position = playerAfterPos + Vector3.up * JUMP_HEIGHT;
        var playerEular = playerTransform.eulerAngles;
        playerEular.y = partnerCell.transform.eulerAngles.y;
        playerTransform.eulerAngles = playerEular;
        yield return MovePlayer(playerTransform, playerTransform.position, playerAfterPos);

        yield return new WaitForSeconds(1.0f);

        // パートナー → 上昇
        yield return MovePlayer(partnerTransform, partnerTransform.position, partnerTransform.position + Vector3.up * JUMP_HEIGHT);

        yield return new WaitForSeconds(0.5f);

        // カメラの場所変更
        SetVCamTarget(playerCell.transform);

        yield return new WaitForSeconds(0.5f);

        // パートナー → 降下
        partnerTransform.position = partnerAfterPos + Vector3.up * JUMP_HEIGHT;
        var partnerEular = partnerTransform.eulerAngles;
        partnerEular.y = playerCell.transform.eulerAngles.y;
        partnerTransform.eulerAngles = partnerEular;
        yield return MovePlayer(partnerTransform, partnerTransform.position, partnerAfterPos);

        yield return new WaitForSeconds(1.0f);

        m_CurrentActionState = ActionState.END;
    }
    private IEnumerator MovePlayer(Transform target, Vector3 startPos, Vector3 endPos)
    {
        float elapsed = 0f;
        while (elapsed < MOVE_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / MOVE_DURATION);
            target.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        target.position = endPos;
    }

    private void SetVCamTarget(Transform targetTransform)
    {
        if (m_SwapVCam == null)
        {
            m_SwapVCam = CameraManager.instance.GetSwapVCam();
        }
        m_SwapVCam.Follow = targetTransform;
        m_SwapVCam.LookAt = targetTransform;
    }

    private void Action_End()
    {
        m_Player.SetCurrentCell(m_PartnerCellNum);
        m_Partner.SetCurrentCell(m_PlayerCellNum);
        m_Partner = null;
        m_PlayerCellNum = -1;
        m_PartnerCellNum = -1;
        m_Player.EndCellAction();
        m_CurrentActionState = ActionState.TEXT;
    }
}

