using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SoundEffectManager
{
    public SoundEffect Sound { get; set; }
    public SoundEffectInstance Instance { get; set; }

    public SoundEffectManager()
    {
    }

    public void Load(SoundEffect sound)
    {
        Sound = sound;
        Instance = sound.CreateInstance();
    }
    public void Load(SoundEffect sound, float volume)
    {
        Load(sound);

        Instance.Volume = volume;
    }
    public void Load(SoundEffect sound, float volume, float pitch)
    {
        Load(sound, volume);

        Instance.Pitch = pitch;
    }
    public void Load(SoundEffect sound, float volume, float pitch, float pan)
    {
        Load(sound, volume, pitch);

        Instance.Pan = pan;
    }
    public void Play(bool isLooped)
    {
        Instance.IsLooped = isLooped;
        Instance.Play();
    }
    public void Stop()
    {
        Instance.Stop();
    }
}