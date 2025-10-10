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

    // Player1��2�𔻕ʂ��āA����܂̃��X�g���擾����
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
            // ���̃v���C���[�̂����
            var leftDaruma = Instantiate(m_DarumaBodyprefab, this.transform);
            leftDaruma.transform.localPosition = new Vector3(-2.0f, (m_DarumaHeight * i) + m_DarumaHeight, 0.75f);
            m_LeftDarumaBodyList.Add(leftDaruma);
            Debug.Log(m_LeftDarumaBodyList.Count);

            // �E�̃v���C���[�̂����
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
                // �}�e���A���𕡐����ċ��L�}�e���A���̕ύX��h��
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
            // HSV�Ń����_���F������
            kv.Value.color = Color.HSVToRGB(Random.value, 0.6f, 1f);
        }
    }

    // ����܂�@��������
    public void HitDaruma(List<GameObject> darumaList,Player_DarumaOtoshi player)
    {
        if (darumaList.Count == 0) return;

        // ��ԉ������o��
        var bottom = darumaList[0];
        darumaList.RemoveAt(0);

        // �J�E���g�����Z
        player.AddHit();

        // ���̌Œ�ʒu�֔�΂��� �� ����
        StartCoroutine(FlyAndDestroy(bottom, player));

        // �c���1�i���փ��[�v
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
        float duration = 0.3f; // ��Ԏ���
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            daruma.transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        Destroy(daruma); // ���B������폜
        player.AddDarumaDestroyed();
    }
}
