using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stat : MonoBehaviour {

    public float lerpSpeed;
    private Image content;
    public Text statValue;
    private float currentValue;
    private float currentFill;
    private Vector3 defaultScale;
    public float MaxValue { get; set; }
    public float CurrentValue {
        get {
            return currentValue;
        }

        set {
            if (value > MaxValue) {
                currentValue = MaxValue;
            } else if (value < 0) {
                currentValue = 0;
            } else {
                currentValue = value;
            }
            currentFill = currentValue / MaxValue;
            statValue.text = currentValue + " / " + MaxValue;
        }
    }

    // Use this for initialization
    void Start () {
        content = GetComponent<Image>();
        content.fillAmount = 1;
        defaultScale = transform.localScale;
    }

    // Update is called once per frame
    void Update () {
        if (currentFill != content.fillAmount) {
            content.fillAmount = Mathf.Lerp(content.fillAmount, currentFill, Time.deltaTime * lerpSpeed);
        }
        if (transform.parent.parent.localScale.x < 0) {
            transform.localScale = new Vector3(-defaultScale.x, defaultScale.y, defaultScale.z);
        } else {
            transform.localScale = new Vector3(defaultScale.x, defaultScale.y, defaultScale.z);
        }
    }

    public void Initialize (float currValue, float maxValue) {
        MaxValue = maxValue;
        CurrentValue = currValue;
    }
}
