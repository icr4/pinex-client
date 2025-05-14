using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class SideModalController : MonoBehaviour
{
    void Start() {
        this.transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(() => Hide());
    }

    public void Show() {
        Resources.FindObjectsOfTypeAll<SideModalController>()
            .ToList<SideModalController>()
            .ForEach((s) => {
                s.Hide();
            });

        this.gameObject.SetActive(true);
    }

    public void Hide() {
        this.gameObject.SetActive(false);
    }
}