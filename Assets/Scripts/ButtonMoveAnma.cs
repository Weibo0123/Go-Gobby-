using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class ButtonMoveAnma : MonoBehaviour
{


   Animator animation;

    public GameObject ground;
    


   public void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Player"))
        {
            animation.SetTrigger("Press");

            if (ground != null)
            {
                ground.transform.position = new Vector2(-3,(float) -1.96);
            }
            
        }
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animation = GetComponent<Animator>();

    }

}
