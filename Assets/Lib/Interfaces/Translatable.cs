using UnityEngine;
using UnityEngine.UI;

public class Translatable : MonoBehaviour
{
    public string translateKey;
    public bool translateEnabled = true;

    public void Translate()
    {
        if (!this.translateEnabled) return;
        this.transform.GetComponent<Text>().text = TranslateService.instance.Translate(this.translateKey);
    }
}
