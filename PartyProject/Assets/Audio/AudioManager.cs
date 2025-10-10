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
    //SEデータ群
    [SerializeField]
    private List<ClipsByField> m_SeClipsByField = new();
    //BGMデータ群
    [SerializeField] 
    private List<ClipsByField> m_BgmClipsByField = new();
    //再生中できるAudio枠
    private AudioSource m_BGM;
    private AudioSource m_SE;
    protected override void Awake()
    {
        base.Awake();
        //bgmのAudioSourceを追加し設定する
        m_BGM = this.gameObject.AddComponent<AudioSource>();
        m_BGM.loop = true;
        //seのAudioSourceを追加し設定する
        m_SE = this.gameObject.AddComponent<AudioSource>();
    }
    // seを流すメソッド
    public void PlaySE(string _SeField, string _SeName)
    {
        Clip clip = SearchClip(m_SeClipsByField, _SeField, _SeName);
        if (clip == null) return;
        // 音量は SEData の設定を使用
        m_SE.volume = clip.m_Volume;
        m_SE.PlayOneShot(clip.m_Clip);
    }
    // 指定したseを止めるメソッド
    // 未実装
    // bgmを流すメソッド
    public void PlayBGM(string _BgmField, string _BgmName)
    {
        Clip clip = SearchClip(m_BgmClipsByField, _BgmField, _BgmName);
        if (clip == null) return;
        // 音量は Clip の設定を使用
        m_BGM.volume = clip.m_Volume;
        m_BGM.clip = clip.m_Clip;
        m_BGM.Play();
    }
    // bgmを止めるメソッド
    public void StopBGM()
    {
        m_BGM.Stop();
    }
    // 履歴にあるbgmを流すメソッド
    public void PleyResultBGM(string _BgmField)
    {
        var ClipsData = m_BgmClipsByField.Find(data => data.m_Filde == _BgmField);
        var clip = ClipsData.GetResultClip();
        if (clip == null) 
        {
            Debug.LogWarning("履歴がないのに前回のBGM流そうとしてる");
            return;
        }
        // 音量は Clip の設定を使用
        m_BGM.volume = clip.m_Volume;
        m_BGM.clip = clip.m_Clip;
        m_BGM.Play();
    }
    // クリップを返すメソッド
    public AudioClip GetClip(bool _IsSE, string _Field, string _Name)
    {
        List<ClipsByField> ClipsByField = (_IsSE) ? m_SeClipsByField : m_BgmClipsByField;
        return SearchClip(ClipsByField, _Field, _Name).m_Clip;
    }
    private Clip SearchClip(List<ClipsByField> _SeClipsByField, string _Field, string _Name)
    {
        // _SeClipsByField.m_SeField == _SeFieldで探す
        var ClipsData = _SeClipsByField.Find(data => data.m_Filde == _Field);
        if (ClipsData == null)
        {
            Debug.LogWarning($"ClipField '{_Field}' が見つかりません");
            return null;
        }
        // AudioClip.name == _SeNameで探す
        var clip = ClipsData.m_Clips.Find(c => c.m_Clip?.name == _Name);
        if (clip == null)
        {
            Debug.LogWarning($"CLIP '{_Name}' が '{_Field}' に見つかりません");
            return null;
        }
        ClipsData.SetResultClip (clip);
        return clip;
    }
}
