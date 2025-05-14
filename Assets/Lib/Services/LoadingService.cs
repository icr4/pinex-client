using UnityEngine;

public class LoadingService : MonoBehaviour
{
    public static LoadingService instance { get; private set; }
    public GameObject loadingScreen;

    private void Awake()
    { 
        if (instance != null && instance != this) Destroy(this); 
        else instance = this; 
    }

    void Start()
    {
    }

    void Update()
    {
    }

    public void enable()
    {
        loadingScreen.gameObject.SetActive(true);
    }
    public void disable()
    {
        loadingScreen.gameObject.SetActive(false);
    }
}