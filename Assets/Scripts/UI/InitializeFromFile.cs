using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class InitializeFromFile : MonoBehaviour, IConfigurable
{

    [SerializeField] private TMP_InputField fileInput;
    [SerializeField] private NBodyController bodyController;
    [SerializeField] private Button confirmButton;
    private DataHandler _dataHandler;
    void Start()
    {
        _dataHandler = new DataHandler();
    }
    
    private void OnConfirmButtonClicked()
    {
        var parsed = _dataHandler.ReadCSV(fileInput.text);
        bodyController.InitializeFromFile(parsed);
    }

    public void Initialize(DefaultSettingsSO settings)
    {
        confirmButton.onClick.RemoveAllListeners();
        fileInput.text = settings.filePath;
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }
}
