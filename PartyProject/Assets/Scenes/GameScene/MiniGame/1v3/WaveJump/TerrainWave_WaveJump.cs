using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class TerrainWave_WaveJump : MonoBehaviour
{
    public GameObject Collider;
    //���₩�Ȕg
    private float baseWaveSpeed = 1.5f;
    private float baseWaveHeight = 0.003f;
    private float baseWaveFrequency = 0.1f;
    //�傫�Ȕg
    private float pulseSpeed = 65.0f;
    private float pulseHeight = 0.010f;
    private float pulseWidth = 10.0f;
    private float pulseDuration = 6.0f;
    // �g�p���̃e���C��
    private Terrain terrain;
    // �e���C���̃f�[�^�i�������Ȃǁj
    private TerrainData terrainData;
    // �����}�b�v�̉𑜓x
    private int heightmapResolution;
    //�X�V�����鍂�����
    float[,] heights;
    // ���Ԃ̒~�ρi�g�̐i�s�Ɏg�p�j
    private float baseTime;
    //���S����̋����ꗗ
    float[,] distFromCenter;
    //�����̃p���X���L�^����\���̂ƃ��X�g
    public class Pulse
    {
        //�J�n���Ԃ̕ۑ���
        public float startTime;
        //�R���C�_�[�p�̃I�u�W�F�N�g�̕ۑ���
        public GameObject Collider;
        //�R���X�g���N�^
        public Pulse(float _startTime, GameObject _collider)
        {
            this.startTime = _startTime;
            this.Collider = _collider;
        }
    }
    private List<Pulse> activePulses = new List<Pulse>();
    void Awake()
    {
        // �������ԃ��Z�b�g
        baseTime = 0.0f;
        // �V�[�����̃e���C�����擾
        terrain = Terrain.activeTerrain;
        // �e���C���̃f�[�^���擾
        terrainData = terrain.terrainData;
        // �𑜓x�擾
        heightmapResolution = terrainData.heightmapResolution;
        //�t�B�[���h���X�V�����鍂��
        heights = new float[heightmapResolution, heightmapResolution];
        //�t�B�[���h�̃��Z�b�g���鍂��
        float[,] flatHeights = new float[heightmapResolution, heightmapResolution];
        //�t�B�[���h�̒��S����̋���
        distFromCenter = new float[heightmapResolution, heightmapResolution];
        {
            int _center = heightmapResolution / 2;
            for (int y = 0; y < heightmapResolution; y++)
            {
                for (int x = 0; x < heightmapResolution; x++)
                {
                    //�S�Ă�0����
                    flatHeights[y, x] = 0.0f;
                    //���S����̋��������炩���ߌv�Z
                    float dx = x - _center;
                    float dy = y - _center;
                    distFromCenter[y, x] = Mathf.Sqrt(dx * dx + dy * dy);
                }
            }
        }
        terrainData.SetHeights(0, 0, flatHeights);
    }
    void Update()
    {
        //�N���b�N�Ŕg�ǉ�
        if (Input.GetMouseButtonDown(0))
        {
            SetPulses();
        }
        baseTime += Time.deltaTime;
        int center = heightmapResolution / 2;
        //�S�|���S�������
        for (int y = 0; y < heightmapResolution; y++)
        {
            for (int x = 0; x < heightmapResolution; x++)
            {
                //���₩�Ȕg�̐���
                {
                    float wave = Mathf.Sin(distFromCenter[y,x] * baseWaveFrequency - baseTime * baseWaveSpeed) * baseWaveHeight;
                    heights[y, x] = wave;
                }
            }
        }
        //�S�p���X�𔽉f(���������̂��ߋt����)
        for (int i = activePulses.Count - 1; i >= 0; i--)
        {
            float elapsed = Time.time - activePulses[i].startTime;
            //�폜
            if (elapsed > pulseDuration)
            {
                //�R���C�_�[���폜
                Destroy(activePulses[i].Collider);
                //Pulse���폜
                activePulses.RemoveAt(i);
                continue;
            }
            float currentRadius = elapsed * pulseSpeed;

            float _maxHeights = 0.0f;
            for (int y = 0; y < heightmapResolution; y++)
            {
                for (int x = 0; x < heightmapResolution; x++)
                {
                    //���݂̔g�̒��S�Ƃ̋�����
                    float delta = distFromCenter[y, x] - currentRadius;
                    //����オ��
                    float pulse = Mathf.Exp(-delta * delta / (2 * pulseWidth * pulseWidth)) * pulseHeight;
                    //���̍����ɉ��Z
                    heights[y, x] += pulse;
                    _maxHeights = (heights[y,x]>_maxHeights)? heights[y,x]:_maxHeights;
                }
            }
            // �R���C�_�[���g��
            float diameter = currentRadius * 0.12f;
            activePulses[i].Collider.transform.localScale = new Vector3(diameter,_maxHeights*2700f, diameter);
        }
        terrainData.SetHeights(0, 0, heights);
    }
    public void SetPulses()
    {
        //�J�n���Ԃ̕ۑ��ƃR���C�_�[�̐���
        activePulses.Add(new Pulse(Time.time, Instantiate(Collider)));
        activePulses[activePulses.Count - 1].Collider.SetActive(true);
        // ���S���W�i�n�`�̒��S����j
        Vector3 centerPos = terrain.transform.position + new Vector3(terrainData.size.x * 0.5f, 0f, terrainData.size.z * 0.5f);
        activePulses[activePulses.Count - 1].Collider.transform.position = centerPos;
    }
}