using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitializeRandomPanel : MonoBehaviour, IConfigurable
{
    [SerializeField] private DefaultSettingsSO settings;
    [SerializeField] private InputValue bodiesCountInput;
    [SerializeField] private InputValue spawnRangeInput;
    [SerializeField] private InputValue velocitiesRangeInput;
    [SerializeField] private RangeValue rangValue;
    [SerializeField] private Button confirmButton;
    [SerializeField] private NBodyController bodyController;
    
    public void Initialize(DefaultSettingsSO settings)
    {
        confirmButton.onClick.RemoveAllListeners();
        bodiesCountInput.Initialize("Bodies Count", settings.bodiesCount);
        spawnRangeInput.Initialize("Spawn range: ", settings.spawnRange);
        velocitiesRangeInput.Initialize("Velocities range: ", settings.velocitiesRange);
        rangValue.Initialize("Mass range: ", settings.minMass, settings.maxMass);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    private void OnConfirmButtonClicked()
    {
        int bodiesCount = int.Parse(bodiesCountInput.GetValue());
        float spawnRange = float.Parse(spawnRangeInput.GetValue());
        float velocitiesRange = float.Parse(velocitiesRangeInput.GetValue());
        var (minMass, maxMass) = rangValue.GetValue();
        float massMinValue = float.Parse(minMass);
        float massMaxValue = float.Parse(maxMass);
        bodyController.Initialize(bodiesCount, spawnRange, velocitiesRange, massMinValue, massMaxValue);
    }
}