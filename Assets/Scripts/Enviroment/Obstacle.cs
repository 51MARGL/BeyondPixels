using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour, IComparable<Obstacle>
{
    /// <summary>
    /// The obstacles spriterenderer 
    /// </summary>
    public SpriteRenderer SpriteRenderer { get; set; }

    /// <summary>
    /// Color to use the the obstacle isn't faded
    /// </summary>
    private Color defaultColor;

    /// <summary>
    /// Color to use the the obstacle is faded out
    /// </summary>
    private Color fadedColor;

    /// <summary>
    /// Compare to, that is used for sorting the obstacles, so that we can pick the on with the lowest sortorder
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(Obstacle other)
    {
        if (SpriteRenderer.sortingOrder > other.SpriteRenderer.sortingOrder)
        {
            return 1; //If this obstacles has a higher sortorder 
        }
        else if (SpriteRenderer.sortingOrder < other.SpriteRenderer.sortingOrder)
        {
            return -1; //If this obstacles has a lower sortorder 
        }

        return 0; //If both obstacles ha an equal sortorder
    }

    // Use this for initialization
    void Start ()
    {
        //Creates a reference to the spriterendere
        SpriteRenderer = GetComponent<SpriteRenderer>();

        //Creates the colors
        defaultColor = SpriteRenderer.material.color;
        fadedColor = defaultColor;
        fadedColor.a = defaultColor.a / 2;
	}

    /// <summary>
    /// Fades out the obstacle
    /// </summary>
    public void FadeOut()
    {
        SpriteRenderer.color = fadedColor;
    }

    /// <summary>
    /// Fades in the obstacle
    /// </summary>
    public void FadeIn()
    {
        SpriteRenderer.color = defaultColor;
    }
	
}
