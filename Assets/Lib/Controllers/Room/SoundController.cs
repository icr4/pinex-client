using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController instance { get; private set; }
    public AudioSource cardAudioSource, clockAudioSource, ringAudioSource, victoryAudioSource;

    void Start()
    {
        this.cardAudioSource = gameObject.AddComponent<AudioSource>();
        cardAudioSource.clip = (AudioClip)Resources.Load("Sounds/CardClip");

        this.clockAudioSource = gameObject.AddComponent<AudioSource>();
        clockAudioSource.clip = (AudioClip)Resources.Load("Sounds/FastClock");

        this.ringAudioSource = gameObject.AddComponent<AudioSource>();
        ringAudioSource.clip = (AudioClip)Resources.Load("Sounds/RingBell");

        this.victoryAudioSource = gameObject.AddComponent<AudioSource>();
        victoryAudioSource.clip = (AudioClip)Resources.Load("Sounds/Victory");
    }

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }
}