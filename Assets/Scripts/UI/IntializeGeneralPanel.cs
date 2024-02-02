using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntializeGeneralPanel : MonoBehaviour, IConfigurable
{
    [SerializeField] private DefaultSettingsSO defaultSettings;
    [SerializeField] private InputValue gravityConstant;
    [SerializeField] private InputValue threadsCount;
    [SerializeField] private InputValue radiusConstraint;
    [SerializeField] private NBodyController bodyController;
    [SerializeField] private Button updateButton;
    [SerializeField] private Toggle toggle;
    
    private void OnConfirmButtonClicked()
    {
        float gravityConst = float.Parse(gravityConstant.GetValue());
        float constraint = float.Parse(radiusConstraint.GetValue());
        int threads = int.Parse(threadsCount.GetValue());
        bool gpuEnabled = toggle.isOn;
        bodyController.UpdateGeneralSettings(threads, gravityConst, constraint, gpuEnabled);
    }

    public void Initialize(DefaultSettingsSO settings)
    {
        updateButton.onClick.RemoveAllListeners();
        gravityConstant.Initialize("Gravity Constant: ", settings.gravityConstant);
        threadsCount.Initialize("Threads Count: ", settings.threadsCount);
        radiusConstraint.Initialize( "Radius Constraint: ", settings.distanceConstraint);
        toggle.isOn = bodyController.useComputeShader;
        updateButton.onClick.AddListener(OnConfirmButtonClicked);
        OnConfirmButtonClicked();
    }
}
