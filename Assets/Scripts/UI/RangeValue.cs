using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RangeValue : MonoBehaviour
{

    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_InputField minValueInput;
    [SerializeField] private TMP_InputField maxValueInput;

    private string _cachedValue = "";
    

    public void Initialize<T>(string text, T minValue, T maxValue)
    {
        label.text = text;
        minValueInput.text = minValue.ToString();
        maxValueInput.text = maxValue.ToString();
    }

    public (string, string) GetValue()
    {
        return (minValueInput.text, maxValueInput.text);
    }
}
