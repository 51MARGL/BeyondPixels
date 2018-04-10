using UnityEngine;
using UnityEngine.UI;

public class Stat : MonoBehaviour
{
    private Image content;
    private float currentFill;
    private float currentValue;
    private Vector3 defaultScale;

    public float lerpSpeed;
    public Text statValue;
    public float MaxValue { get; set; }

    public float CurrentValue
    {
        get { return currentValue; }

        set
        {
            if (value > MaxValue)
                currentValue = MaxValue;
            else if (value < 0)
                currentValue = 0;
            else
                currentValue = value;
            currentFill = currentValue / MaxValue;
            statValue.text = currentValue + " / " + MaxValue;
        }
    }

    // Use this for initialization
    private void Start()
    {
        content = GetComponent<Image>();
        content.fillAmount = 1;
        defaultScale = transform.localScale;
    }

    // Update is called once per frame
    private void Update()
    {
        if (currentFill != content.fillAmount)
            content.fillAmount = Mathf.Lerp(content.fillAmount, currentFill, Time.deltaTime * lerpSpeed);
        if (transform.parent.parent.localScale.x < 0)
            transform.localScale = new Vector3(-defaultScale.x, defaultScale.y, defaultScale.z);
        else
            transform.localScale = new Vector3(defaultScale.x, defaultScale.y, defaultScale.z);
    }

    public void Initialize(float currValue, float maxValue)
    {
        MaxValue = maxValue;
        CurrentValue = currValue;
    }
}