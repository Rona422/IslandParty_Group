using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DarumaManager : MonoBehaviour
{
    private readonly float m_DarumaHeight = 1.5f;

    [SerializeField] GameObject m_DarumaBodyprefab;
    [SerializeField] MiniGameBase m_MiniGameBase;

    private List<GameObject> m_LeftDarumaBodyList = new();
    private List<GameObject> m_RightDarumaBodyList = new();

    private Dictionary<Renderer, Material> m_MaterialCache = new();

    // Player1か2を判別して、だるまのリストを取得する
    public List<GameObject> GetDarumaBodyList(PlayerBase player)
    {
        return (m_MiniGameBase.GetPlayers()[0] == player) ? m_LeftDarumaBodyList : m_RightDarumaBodyList;
    }

    void Start()
    {
        DarumaSpawn();
    }

    private void DarumaSpawn()
    {
        for (int i = 0; i < 100; i++)
        {
            // 左のプレイヤーのだるま
            var leftDaruma = Instantiate(m_DarumaBodyprefab, this.transform);
            leftDaruma.transform.localPosition = new Vector3(-2.0f, (m_DarumaHeight * i) + m_DarumaHeight, 0.75f);
            m_LeftDarumaBodyList.Add(leftDaruma);
            Debug.Log(m_LeftDarumaBodyList.Count);

            // 右のプレイヤーのだるま
            var rightDaruma = Instantiate(m_DarumaBodyprefab, this.transform);
            rightDaruma.transform.localPosition = new Vector3(4.0f, (m_DarumaHeight * i) + m_DarumaHeight, 0.75f);
            m_RightDarumaBodyList.Add(rightDaruma);
            Debug.Log(m_RightDarumaBodyList.Count);
        }

        CacheMaterials();
        ApplyRandomColors();
    }

    private void CacheMaterials()
    {
        var allDarumaRenderers = m_LeftDarumaBodyList
            .Concat(m_RightDarumaBodyList)
            .Select(obj => obj.GetComponent<Renderer>())
            .Where(r => r != null);

        foreach (Renderer rend in allDarumaRenderers)
        {
            if (!m_MaterialCache.ContainsKey(rend))
            {
                // マテリアルを複製して共有マテリアルの変更を防ぐ
                Material newMat = new(rend.material);
                m_MaterialCache[rend] = newMat;
                rend.material = newMat;
            }
        }
    }

    private void ApplyRandomColors()
    {
        foreach (var kv in m_MaterialCache)
        {
            // HSVでランダム色をつける
            kv.Value.color = Color.HSVToRGB(Random.value, 0.6f, 1f);
        }
    }

    // だるまを叩いた処理
    public void HitDaruma(List<GameObject> darumaList,Player_DarumaOtoshi player)
    {
        if (darumaList.Count == 0) return;

        // 一番下を取り出す
        var bottom = darumaList[0];
        darumaList.RemoveAt(0);

        // カウントを加算
        player.AddHit();

        // 奥の固定位置へ飛ばして → 消す
        StartCoroutine(FlyAndDestroy(bottom, player));

        // 残りを1段下へワープ
        for (int i = 0; i < darumaList.Count; i++)
        {
            var pos = darumaList[i].transform.localPosition;
            pos.y -= m_DarumaHeight;
            darumaList[i].transform.localPosition = pos;
        }
    }

    private IEnumerator FlyAndDestroy(GameObject daruma,Player_DarumaOtoshi player)
    {
        Vector3 start = daruma.transform.localPosition;
        Vector3 target = start + new Vector3(0f, 0f, 10f);
        float duration = 0.3f; // 飛ぶ時間
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            daruma.transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        Destroy(daruma); // 到達したら削除
        player.AddDarumaDestroyed();
    }
}
