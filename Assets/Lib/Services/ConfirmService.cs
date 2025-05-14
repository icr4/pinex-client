using UnityEngine;

public class ConfirmService : MonoBehaviour
{
    public static ConfirmService instance { get; private set; }
    private GameObject confirmModalRes;

    void Start()
    {
        this.confirmModalRes = Resources.Load("Prefabs/ConfirmModal") as GameObject;
    }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void Show(string text, System.Action onClose = null, System.Action onConfirm = null)
    {
        this.Show("confirm.title", text, onClose, onConfirm);
    }

    public GameObject Show(string title, string text, System.Action onClose = null, System.Action onConfirm = null)
    {
        Transform parent = ClientService.instance.MainCanvas().transform;
        GameObject modal = Instantiate(this.confirmModalRes, parent);
        modal.transform.SetParent(parent, false);
        modal.GetComponent<ConfirmModalComponent>().Set(title, text, onClose, onConfirm);
        return modal;
    }
}