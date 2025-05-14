using UnityEngine;
using UnityEngine.UI;
using Models;
using System.Linq;

using System.Collections.Generic;

public class AchievementModalController : MonoBehaviour
{
  public static AchievementModalController instance { get; private set; }
  private GameObject achievementRow;
  public GameObject modalObj;

  void Start()
  {
    this.achievementRow = Resources.Load("Prefabs/AchievementRow", typeof(GameObject)) as GameObject;
    this.modalObj.transform.Find("Modal/CloseBtn").GetComponent<Button>().onClick.AddListener(Hide);
  }

  void Awake()
  {
    if (instance != null && instance != this) Destroy(this);
    else instance = this;
  }

  public void Hide()
  {
    this.Unload();
    this.modalObj.SetActive(false);
  }

  public void Show(List<Achievement> achievements, int level)
  {
    this.Unload();

    if (this.ShowLevelUp(level) || achievements.Count > 0)
    {
      this.modalObj.SetActive(true);
      this.LoadAchievements(achievements);
    }
    else
    {
      this.modalObj.SetActive(false);
    }
  }

  private void Unload()
  {
    Transform parent = this.modalObj.transform.Find("Modal/Body/Content/List").transform;

    // Purge previous if present
    foreach (Transform child in parent.transform)
    {
      if (child.GetComponent<AchievementRowComponent>())
      {
        Destroy(child.gameObject);
      }
    }
  }

  private void LoadAchievements(List<Achievement> achievements)
  {
    Transform parent = this.modalObj.transform.Find("Modal/Body/Content/List").transform;

    // Load newers
    achievements.ForEach((achievement) =>
    {
      GameObject obj = Instantiate(achievementRow, parent);
      obj.transform.SetParent(parent);
      obj.SetActive(true);
      obj.GetComponent<AchievementRowComponent>().Set(achievement);
    });
  }

  private bool ShowLevelUp(int level)
  {
    if (level > this.GetLastLevel(level))
    {
      this.modalObj.transform.Find("Modal/Body/Content/List/LevelRow/Icon/Value").GetComponent<Text>().text = level.ToString();
      this.modalObj.transform.Find("Modal/Body/Content/List/LevelRow").gameObject.SetActive(true);
      return true;
    }
    else
    {
      this.modalObj.transform.Find("Modal/Body/Content/List/LevelRow").gameObject.SetActive(false);
      return false;
    }
  }

  private int GetLastLevel(int level)
  {
    if (!PlayerPrefs.HasKey("last_user_level"))
    {
      PlayerPrefs.SetInt("last_user_level", level);
    }

    int last_level = PlayerPrefs.GetInt("last_user_level");

    PlayerPrefs.SetInt("last_user_level", level);
    return last_level;
  }
}