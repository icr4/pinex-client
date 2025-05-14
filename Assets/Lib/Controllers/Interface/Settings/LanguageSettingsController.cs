using Constants;
using UnityEngine;
using UnityEngine.UI;
using Models;
using System.Linq;

public class LanguageSettingsController : MonoBehaviour
{
    public static LanguageSettingsController instance { get; private set; }
    public GameObject languageObj;
    private Dropdown languageBtn;

    void Awake()
    { 
        if (instance != null && instance != this) Destroy(this); 
        else instance = this; 
    }
    
    void Start() {
        this.languageBtn = this.languageObj.transform.Find("Value/LanguageBtn").GetComponent<Dropdown>();
        this.Set();
    }

    public void Set() {
        this.languageBtn.GetComponent<Dropdown>().onValueChanged.RemoveAllListeners();
        
        this.languageBtn.options = ClientConstants.LOCALES
            .ConvertAll(locale => new Dropdown.OptionData(locale.name));
        
        this.languageBtn.value = TranslateService.instance.locale.id;

        this.languageBtn.GetComponent<Dropdown>().onValueChanged.AddListener(Submit);
    }

    private void Submit(int localeId) {
        Locale locale = ClientConstants.LOCALES.ElementAt(localeId);
        string text = TranslateService.instance.Translate("lobby.settings.account.language.confirm", new(){locale.name});
        
        ConfirmService.instance.Show(text, () => {
            this.Set();
        }, () => {
            TranslateService.instance.SetLocale(locale);
            TranslateService.instance.TranslateScene("Lobby");
        });
    }
}