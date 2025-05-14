using UnityEngine;
using UnityEngine.UI;
using Models;

public class PoolComponent : MonoBehaviour
{
    public Pool pool;

    public void Set(Pool pool)
    {
        this.pool = pool;

        Texture2D iconTexture = Resources.Load($@"Pools/{pool.icon}") as Texture2D;
        this.transform.Find("Panel/Title").GetComponent<Text>().text = pool.title;
        this.transform.Find("Panel/Description").GetComponent<Text>().text = "";
        this.transform.Find("Panel/Cost/Text").GetComponent<Text>().text = pool.coins.ToString();
        this.transform.Find("Panel/Icon").GetComponent<RawImage>().texture = iconTexture;
        this.transform.Find("Panel/LevelContainer/Level").GetComponent<Text>().text = pool.min_level.ToString();
        this.SetEnabled();

    }

    void SetEnabled()
    {
        if (this.pool.enabled && ClientService.instance.user.level >= this.pool.min_level)
        {
            this.transform.GetComponent<Button>().onClick.AddListener(() => LobbyController.instance.SelectPoolMode(this.pool));
            this.transform.GetComponent<Button>().interactable = true;
            this.transform.Find("Panel/Locked").gameObject.SetActive(false);
        }
        else
        {
            this.transform.GetComponent<Button>().interactable = false;
            this.transform.Find("Panel/Locked").gameObject.SetActive(true);
        }
    }

    void Start() { }
}
