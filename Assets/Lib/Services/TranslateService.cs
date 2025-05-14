using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Constants;
using Models;
using System.Collections.Generic;
using System;

public class TranslateService : MonoBehaviour
{
    public static TranslateService instance { get; private set; }
    public Locale locale;
    private Dictionary<string, Dictionary<string, string>> labels;

    void Start()
    {
        if (PlayerPrefs.HasKey("locale"))
        {
            Locale locale = ClientConstants.LOCALES.Find((locale) => locale.code == PlayerPrefs.GetString("locale"));
            this.SetLocale(locale);
        }
        else
        {
            string code = Application.systemLanguage == SystemLanguage.Italian ? "it" : "en";
            Locale locale = ClientConstants.LOCALES.Find((locale) => locale.code == code);

            this.SetLocale(locale);
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void SetLocale(Locale locale)
    {
        if (this.locale != locale)
        {
            this.locale = locale;

            PlayerPrefs.SetString("locale", this.locale.code);
            TextAsset json = Resources.Load<TextAsset>($"i18n/{locale.code}");
            this.labels = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json.text);
        }
    }

    public string Translate(string label, List<string> opts = null)
    {
        List<string> parsedLabel = label.Split(".").ToList();

        string context = parsedLabel.First();
        parsedLabel.RemoveAt(0);

        string dict = String.Join(".", parsedLabel);
        string translation = this.labels[context][dict];

        if (translation == null)
        {
            Debug.LogWarning("Failed translating " + label);
            translation = label;
        }

        if (opts != null)
        {
            opts.ForEach((opt) =>
            {
                translation = translation.Replace($"${{{opts.IndexOf(opt)}}}", opt);
            });
        }

        return translation;
    }

    public void TranslateScene(string scene)
    {
        Resources.FindObjectsOfTypeAll<Translatable>()
            .ToList<Translatable>()
            .FindAll((t) => t.gameObject.scene.name == scene)
            .ForEach((t) =>
            {
                t.Translate();
            });
    }
}