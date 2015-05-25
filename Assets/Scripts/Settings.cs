using UnityEngine;
using System.Collections;

public class Settings
{
    private AudioSource m_MusicSource;
    private AudioSource m_SoundSource;
    private AudioSource m_ReactionSource;

    public float SoundVolume
    {
        get { return m_SoundSource.volume; }
        set { m_SoundSource.volume = value; m_ReactionSource.volume = value; }
    }

    public float MusicVolume
    {
        get { return m_MusicSource.volume; }
        set { m_MusicSource.volume = value; }
    }

    public int HighScore { get; set; }

    public void Load(AudioSource music, AudioSource sound, AudioSource reaction)
    {
        m_MusicSource = music;
        m_SoundSource = sound;
        m_ReactionSource = reaction;

        SoundVolume = PlayerPrefs.GetFloat("SoundVolume", 1.0f);
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    public void Save()
    {
        PlayerPrefs.SetFloat("SoundVolume", SoundVolume);
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        PlayerPrefs.SetFloat("HighScore", HighScore);
    }
}
