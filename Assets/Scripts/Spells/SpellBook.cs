using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SpellBook : MonoBehaviour
{
    [SerializeField]
    private Image castingBar;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private Text spellName;

    [SerializeField]
    private Text castTime;

    [SerializeField]
    private CanvasGroup canvasGroup;

    /// <summary>
    /// All spells in the spellbook
    /// </summary>
    [SerializeField]
    private Spell[] spells;

    private Coroutine spellRoutine;
    private Coroutine fadeRoutine;
    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Cast a spell at an enemy
    /// </summary>
    /// <param name="index">The index of the spell you would like to cast, the first spells is index 0</param>
    /// <returns></returns>
    public Spell CastSpell(int index)
    {
        castingBar.fillAmount = 0;
        castingBar.color = spells[index].BarColor;
        icon.sprite = spells[index].Icon;
        spellName.text = spells[index].Name;

        spellRoutine = StartCoroutine(ProgressCastRoutine(index));
        fadeRoutine = StartCoroutine(FadeBarRoutine());

        //Returns the spell that we just  cast.
        return spells[index];
    }

    private IEnumerator ProgressCastRoutine(int index)
    {
        float timePassed = Time.deltaTime;
        float rate = 1.0f / spells[index].CastTime;
        float progress = 0.0f;
        while (progress <= 1.0)
        {
            castingBar.fillAmount = Mathf.Lerp(0, 1, progress);
            progress += rate * Time.deltaTime;
            timePassed += Time.deltaTime;
            castTime.text = (spells[index].CastTime - timePassed).ToString("F1");
            if (spells[index].CastTime - timePassed < 0)
            {
                castTime.text = "0.0";
            }
            yield return null;
        }
        StopCasting();
    }

    public void StopCasting()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            canvasGroup.alpha = 0;
            fadeRoutine = null;
        }
        if (spellRoutine != null)
        {
            StopCoroutine(spellRoutine);
            spellRoutine = null;
        }
    }

    public IEnumerator FadeBarRoutine()
    {
        float rate = 1.0f / 0.5f;
        float progress = 0.0f;
        while (progress <= 1.0)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, progress);
            progress += rate * Time.deltaTime;
            yield return null;
        }
    }
}
