using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    [System.Serializable]
    public struct LocalizedString
    {
        public string Key;
        public string EN;
        public string RU; 
    }

    [SerializeField] private List<LocalizedString> localizedStrings;
    [SerializeField] private string currentLanguage = "RU";

    private Dictionary<string, string> _lookup;

    private void Awake()
    {

        _lookup = new Dictionary<string, string>();
        foreach (LocalizedString entry in localizedStrings)
        {
            string value = currentLanguage switch
            {
                "EN" => entry.EN,
                "RU" => entry.RU,
                _ => entry.RU
            };
            _lookup[entry.Key] = value;
        }
    }
    
    

    public string Get(string key)
    {
        if (_lookup.TryGetValue(key, out var value))
            return value;
        return key;
    }

    public void SetLanguage(string language)
    {
        currentLanguage = language;
        Awake(); 
    }
}