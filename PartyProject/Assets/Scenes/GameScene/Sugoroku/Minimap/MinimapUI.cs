using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MinimapUI : SingletonMonoBehaviour<MinimapUI>
{
    private RectTransform m_MinimapArea; // �~�j�}�b�v�S�̂�UI�̈�
    [SerializeField] private GameObject m_CellPrefab;     // �Z��UI�iImage�t���j
    [SerializeField] private GameObject m_PlayerIconPrefab; // �v���C���[UI�A�C�R��
    [SerializeField] private GameObject m_LinePrefab; // ���pImage
    private const int m_SegmentsPerLine = 3;  // �������炩�ɂ��镪����
    [SerializeField] private Sprite m_CircleSprite;

    private List<RectTransform> m_CellIcons = new();
    private List<RectTransform> m_PlayerIcons = new();

    private Vector2 min, max;
    private void Start()
    {
        base.Awake();
        m_MinimapArea = GetComponent<RectTransform>();
        Initialize(SugorokuStage.instance.GetCellTransform()); 
        DrawSmoothLines();
        HideMinimap();
    }

    private void Update()
    {
        UpdateAllPlayers();
    }
    public void ShowMinimap()
    {
        gameObject.SetActive(true);
    }

    public void HideMinimap()
    {
        gameObject.SetActive(false);
    }

    public void Initialize(List<Transform> boardCells)
    {
        var cells2D = boardCells.Select(c => new Vector2(c.position.x, c.position.z)).ToList();

        // ���W�̍ŏ��E�ő���擾
        min = new Vector2(cells2D.Min(v => v.x), cells2D.Min(v => v.y));
        max = new Vector2(cells2D.Max(v => v.x), cells2D.Max(v => v.y));

        // �Z��UI�z�u
        foreach (var cell in boardCells)
        {
            var icon = Instantiate(m_CellPrefab, m_MinimapArea).GetComponent<RectTransform>();
            icon.anchoredPosition = WorldToUI(new Vector2(cell.position.x, cell.position.z));

            // Y����]�𔽉f
            icon.localRotation = Quaternion.Euler(0, 0, -cell.eulerAngles.y); // 2D��Z��]�ŕ\��
            m_CellIcons.Add(icon);
        }

        // --- �ǉ������F�ŏ��ƍŌ�̃Z�������� ---
        if (m_CellIcons.Count > 0)
        {
            // �ŏ��̃Z��
            var first = m_CellIcons.First();
            first.localScale = new Vector3(2.5f, 2.5f, 1f);
            first.GetComponent<Image>().sprite = m_CircleSprite;

            // �Ō�̃Z��
            var last = m_CellIcons.Last();
            last.localScale = new Vector3(2.5f, 2.5f, 1f);
            last.GetComponent<Image>().sprite = m_CircleSprite;
        }

        // --- �v���C���[UI�z�u ---
        for (int i = 0; i < PlayerManager.PlayerMax; i++)
        {
            var pIcon = Instantiate(m_PlayerIconPrefab, m_MinimapArea).GetComponent<RectTransform>();
            pIcon.GetComponent<Image>().color = PlayerManager.instance.m_Characters[i].SkinColor;
            m_PlayerIcons.Add(pIcon);
        }
    }
    private void UpdateAllPlayers()
    {
        var players = SugorokuManager.instance.GetPlayers();
        for (int i = 0; i < players.Count && i < m_PlayerIcons.Count; i++)
        {
            Vector3 worldPos = players[i].gameObject.transform.position;
            Vector2 pos2D = new Vector2(worldPos.x, worldPos.z);
            m_PlayerIcons[i].anchoredPosition = WorldToUI(pos2D);
        }
    }

    private Vector2 WorldToUI(Vector2 world)
    {
        // 0-1 �ɐ��K��
        float nx = (world.x - min.x) / (max.x - min.x);
        float ny = (world.y - min.y) / (max.y - min.y);

        // MinimapArea�T�C�Y�ɕϊ��i���S��j
        Vector2 size = m_MinimapArea.rect.size;
        float px = nx * size.x - size.x * 0.5f;
        float py = ny * size.y - size.y * 0.5f;

        return new Vector2(px, py);
    }

    private void DrawSmoothLines()
    {
        for (int i = 0; i < m_CellIcons.Count - 1; i++)
        {
            var start = m_CellIcons[i].anchoredPosition;
            var end = m_CellIcons[i + 1].anchoredPosition;

            // ���ԓ_���쐬
            for (int s = 0; s < m_SegmentsPerLine; s++)
            {
                float t0 = (float)s / m_SegmentsPerLine;
                float t1 = (float)(s + 1) / m_SegmentsPerLine;

                Vector2 p0 = Vector2.Lerp(start, end, t0);
                Vector2 p1 = Vector2.Lerp(start, end, t1);

                var segment = Instantiate(m_LinePrefab, m_MinimapArea).GetComponent<RectTransform>();
                segment.gameObject.SetActive(true);

                // �����ɔz�u
                Vector2 mid = (p0 + p1) / 2f;
                segment.anchoredPosition = mid;

                // ����
                float distance = Vector2.Distance(p0, p1) * 1.05f;
                segment.sizeDelta = new Vector2(distance, segment.sizeDelta.y);

                // ��]
                Vector2 dir = p1 - p0;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                segment.localRotation = Quaternion.Euler(0, 0, angle);

                // �����~�j�}�b�v���w�ɔz�u
                segment.SetAsFirstSibling();
            }
        }
    }
}