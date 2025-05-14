using UnityEngine;

public class AudioService : MonoBehaviour
{
  public static AudioService instance { get; private set; }

  public bool isEnabled = false;


  void Awake()
  {
    if (instance != null && instance != this) Destroy(this);
    else instance = this;
  }

  void Start()
  {
    this.isEnabled = !PlayerPrefs.HasKey("audio_disabled");
    AudioListener.volume = this.isEnabled ? 1 : 0;
  }

  public void Enable()
  {
    this.isEnabled = true;
    PlayerPrefs.DeleteKey("audio_disabled");
    AudioListener.volume = 1;
  }

  public void Disable()
  {
    this.isEnabled = false;
    PlayerPrefs.SetInt("audio_disabled", 1);
    AudioListener.volume = 0;
  }

  public void Toggle()
  {
    if (this.isEnabled) { this.Disable(); } else { this.Enable(); }
  }
}