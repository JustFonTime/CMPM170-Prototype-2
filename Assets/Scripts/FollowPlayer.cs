using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;

    void Update()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;

            // quick fix, prevent enemy sinking
            direction.y = 0;
            
        
            transform.position += direction * speed * Time.deltaTime;
            


        }
    }
}
