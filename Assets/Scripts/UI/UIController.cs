using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [System.Serializable]
    public class ControlPanels
    {
        public Button enablingBtn;
        public GameObject panel;
    }

    [SerializeField] private ControlPanels[] panels;
    [SerializeField] private DefaultSettingsSO[] settings;
    [SerializeField] private TMP_Dropdown dropdown;
    void Start()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            int index = i;
            panels[i].enablingBtn.onClick.AddListener(() =>
            {
                OnButtonClicked(index);
            });
        }

        OnDropdownValueChanged(0);
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

     private void OnDropdownValueChanged(int value)
     {
         var settingsToApply = settings[value];
         for (int i = 0; i < panels.Length; i++)
         {
             var configurable = panels[i].panel.GetComponent<IConfigurable>();
             configurable.Initialize(settingsToApply);
         }
     }

    private void OnButtonClicked(int i)
    {
        CheckOtherPanels(i);
        panels[i].panel.SetActive(!panels[i].panel.activeSelf);
    }

    private void CheckOtherPanels(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i].panel.activeSelf == false || index == i)
            {
                continue;
            }

            panels[i].panel.SetActive(false);
        }
    }
}
