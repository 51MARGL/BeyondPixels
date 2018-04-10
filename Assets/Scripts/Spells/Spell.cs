using System;
using UnityEngine;

[Serializable]
public class Spell
{
    /// <summary>
    ///     The spell's color
    /// </summary>
    [SerializeField] private Color barColor;

    /// <summary>
    ///     The spell's cast time
    /// </summary>
    [SerializeField] private float castTime;

    /// <summary>
    ///     The spell's damage
    /// </summary>
    [SerializeField] private int damage;

    /// <summary>
    ///     The spell's duration time
    /// </summary>
    [SerializeField] private float duration;

    /// <summary>
    ///     The spell's icon
    /// </summary>
    [SerializeField] private Sprite icon;

    /// <summary>
    ///     The Spell's name
    /// </summary>
    [SerializeField] private string name;

    /// <summary>
    ///     The spell's prefab
    /// </summary>
    [SerializeField] private GameObject spellPrefab;

    /// <summary>
    ///     Property for accessing the spell's name
    /// </summary>
    public string Name
    {
        get { return name; }
    }

    /// <summary>
    ///     Property for reading the damage
    /// </summary>
    public int Damage
    {
        get { return damage; }
    }

    /// <summary>
    ///     Property for reading the icon
    /// </summary>
    public Sprite Icon
    {
        get { return icon; }
    }

    /// <summary>
    ///     Property for reading the speed
    /// </summary>
    public float Duration
    {
        get { return duration; }
    }

    /// <summary>
    ///     Property for reading the cast time
    /// </summary>
    public float CastTime
    {
        get { return castTime; }
    }

    /// <summary>
    ///     Property for reading the spell's prefab
    /// </summary>
    public GameObject SpellPrefab
    {
        get { return spellPrefab; }
    }

    /// <summary>
    ///     Property for reading the color
    /// </summary>
    public Color BarColor
    {
        get { return barColor; }
    }
}