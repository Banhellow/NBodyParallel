using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputValue : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_InputField value;

    private string _cachedValue = "";

    void Start()
    {

    }

    public void Initialize<T>(string text, T defaultValue)
    {
        label.text = text;
        value.text = defaultValue.ToString();
    }

    public string GetValue()
    {
        return value.text;
    }
}