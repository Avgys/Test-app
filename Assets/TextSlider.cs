using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextSlider : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Value;

    public UnityEvent<float> OnValueChanged;

    public void SetValue(float value)
    {
        Value.text = Mathf.RoundToInt(value).ToString();
        OnValueChanged?.Invoke(value);
    }
}
