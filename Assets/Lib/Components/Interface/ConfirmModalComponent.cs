using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConfirmModalComponent : MonoBehaviour
{
    public void Set(string title, string text, System.Action onClose = null, System.Action onConfirm = null)
    {
        this.transform.Find("Panel/Title").GetComponent<Text>().text = TranslateService.instance.Translate(title);
        this.transform.Find("Panel/Content/Description").GetComponent<Text>().text = text;

        if (onConfirm != null)
        {
            this.transform.Find("Panel/Content/Footer/ConfirmBtn").GetComponent<Button>().onClick.AddListener(() =>
            {
                onConfirm();
                this.Close();
            });
        }
        else
        {
            this.transform.Find("Panel/Content/Footer/ConfirmBtn").gameObject.SetActive(false);
        }

        if (onClose != null)
        {
            this.transform.Find("Panel/Content/Footer/CancelBtn").GetComponent<Button>().onClick.AddListener(() =>
            {
                onClose();
                this.Close();
            });
            this.transform.Find("Panel/CloseBtn").GetComponent<Button>().onClick.AddListener(() =>
            {
                onClose();
                this.Close();
            });
        }
        else
        {
            this.transform.Find("Panel/Content/Footer/CancelBtn").gameObject.SetActive(false);
            this.transform.Find("Panel/CloseBtn").gameObject.SetActive(false);
        }

        TranslateService.instance.TranslateScene(SceneManager.GetActiveScene().name);
        this.gameObject.SetActive(true);
    }

    void Close()
    {
        this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }
}