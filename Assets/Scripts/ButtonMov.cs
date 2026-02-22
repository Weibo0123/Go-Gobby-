using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class ButtonMoveAnma : MonoBehaviour
{
    public GameObject ground;
    
    public int moveDistance = 2;
    bool isMoved = false;


   public void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Player"))
        {
            isMoved = !isMoved;
            if (ground != null && isMoved)
            {
                ground.transform.position = new Vector2(ground.transform.position.x + moveDistance, ground.transform.position.y);
            }
            else if (ground != null && !isMoved)
            {
                ground.transform.position = new Vector2(ground.transform.position.x - moveDistance, ground.transform.position.y);
            }
            
        }
    }

}
