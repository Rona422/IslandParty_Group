using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
    [System.Serializable]
    public class Clip
    {
        public AudioClip m_Clip;
        public float m_Volume = 1.0f;
    }
    [System.Serializable]
    public class ClipsByField
    {
        public string m_Filde;
        public List<Clip> m_Clips = new ();
        private Clip m_ResultClip = new ();
        public void SetResultClip(Clip _Cilp)
        {
            m_ResultClip = _Cilp;
        }
        public Clip GetResultClip()
        {
            return m_ResultClip;
        }
    }
    //SE�f�[�^�Q
    [SerializeField]
    private List<ClipsByField> m_SeClipsByField = new();
    //BGM�f�[�^�Q
    [SerializeField] 
    private List<ClipsByField> m_BgmClipsByField = new();
    //�Đ����ł���Audio�g
    private AudioSource m_BGM;
    private AudioSource m_SE;
    protected override void Awake()
    {
        base.Awake();
        //bgm��AudioSource��ǉ����ݒ肷��
        m_BGM = this.gameObject.AddComponent<AudioSource>();
        m_BGM.loop = true;
        //se��AudioSource��ǉ����ݒ肷��
        m_SE = this.gameObject.AddComponent<AudioSource>();
    }
    // se�𗬂����\�b�h
    public void PlaySE(string _SeField, string _SeName)
    {
        Clip clip = SearchClip(m_SeClipsByField, _SeField, _SeName);
        if (clip == null) return;
        // ���ʂ� SEData �̐ݒ���g�p
        m_SE.volume = clip.m_Volume;
        m_SE.PlayOneShot(clip.m_Clip);
    }
    // �w�肵��se���~�߂郁�\�b�h
    // ������
    // bgm�𗬂����\�b�h
    public void PlayBGM(string _BgmField, string _BgmName)
    {
        Clip clip = SearchClip(m_BgmClipsByField, _BgmField, _BgmName);
        if (clip == null) return;
        // ���ʂ� Clip �̐ݒ���g�p
        m_BGM.volume = clip.m_Volume;
        m_BGM.clip = clip.m_Clip;
        m_BGM.Play();
    }
    // bgm���~�߂郁�\�b�h
    public void StopBGM()
    {
        m_BGM.Stop();
    }
    // �����ɂ���bgm�𗬂����\�b�h
    public void PleyResultBGM(string _BgmField)
    {
        var ClipsData = m_BgmClipsByField.Find(data => data.m_Filde == _BgmField);
        var clip = ClipsData.GetResultClip();
        if (clip == null) 
        {
            Debug.LogWarning("�������Ȃ��̂ɑO���BGM�������Ƃ��Ă�");
            return;
        }
        // ���ʂ� Clip �̐ݒ���g�p
        m_BGM.volume = clip.m_Volume;
        m_BGM.clip = clip.m_Clip;
        m_BGM.Play();
    }
    // �N���b�v��Ԃ����\�b�h
    public AudioClip GetClip(bool _IsSE, string _Field, string _Name)
    {
        List<ClipsByField> ClipsByField = (_IsSE) ? m_SeClipsByField : m_BgmClipsByField;
        return SearchClip(ClipsByField, _Field, _Name).m_Clip;
    }
    private Clip SearchClip(List<ClipsByField> _SeClipsByField, string _Field, string _Name)
    {
        // _SeClipsByField.m_SeField == _SeField�ŒT��
        var ClipsData = _SeClipsByField.Find(data => data.m_Filde == _Field);
        if (ClipsData == null)
        {
            Debug.LogWarning($"ClipField '{_Field}' ��������܂���");
            return null;
        }
        // AudioClip.name == _SeName�ŒT��
        var clip = ClipsData.m_Clips.Find(c => c.m_Clip?.name == _Name);
        if (clip == null)
        {
            Debug.LogWarning($"CLIP '{_Name}' �� '{_Field}' �Ɍ�����܂���");
            return null;
        }
        ClipsData.SetResultClip (clip);
        return clip;
    }
}
