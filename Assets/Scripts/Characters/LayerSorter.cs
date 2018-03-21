using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerSorter : MonoBehaviour
{
    /// <summary>
    /// A reference to the players spriteRenderer
    /// </summary>
    private SpriteRenderer parentRenderer;

    //A list of all obstacles that the player is colliding with
    private List<Obstacle> obstacles = new List<Obstacle>();

    // Use this for initialization
    void Start()
    {
        //Creates the reference to the players spriterenderer
        parentRenderer = transform.parent.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// When the player hits an obstacle
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If we hit a wall
        if (collision.tag == "Wall") //If we hit an obstacle
        {
            //Creates a reference to the obstacle
            Obstacle o = collision.GetComponent<Obstacle>();

            if (o != null)
            {
                //Fades out the tree, so that we can see the player beheind it
                o.FadeOut();
            }
        }
        //If we hit an obstacle
        else if (collision.tag == "Obstacle")
        {
            //Creates a reference to the obstacle
            Obstacle o = collision.GetComponent<Obstacle>();

            if (o != null)
            {
                //Fades out the tree, so that we can see the player beheind it
                o.FadeOut();
                //If we aren't colliding with anything else or we are colliding with something with a less sort order
                if (obstacles.Count == 0 || o.SpriteRenderer.sortingOrder - 1 < parentRenderer.sortingOrder)
                {
                    //Change the sortorder to be beheind what we just hit
                    parentRenderer.sortingOrder = o.SpriteRenderer.sortingOrder - 1;
                }

                //Adds the obstacle to the list, so that we can keep track of it
                obstacles.Add(o);
            }
        }

    }

    /// <summary>
    /// When we stop colliding with an obstacle
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        //If we stopped colliding with a wall
        if (collision.tag == "Wall")
        {
            //Creates a reference to the obstacle
            Obstacle o = collision.GetComponent<Obstacle>();

            if (o != null)
            {
                //Fades in the obstacle so that we can't see through it
                o.FadeIn();
            }
        }
        //If we stopped colliding with an obstacle
        else if (collision.tag == "Obstacle")
        {
            //Creates a reference to the obstacle
            Obstacle o = collision.GetComponent<Obstacle>();

            if (o != null)
            {
                //Fades in the obstacle so that we can't see through it
                o.FadeIn();
                //Removes the obstacle from the list
                obstacles.Remove(o);

                //We don't have any other obstacles
                if (obstacles.Count == 0)
                {
                    parentRenderer.sortingOrder = 200;
                }
                else//We have other obstacles and we need to change the sortorder based on those obstacles.
                {
                    obstacles.Sort();
                    parentRenderer.sortingOrder = obstacles[0].SpriteRenderer.sortingOrder - 1;
                }
            }

        }
    }
}
