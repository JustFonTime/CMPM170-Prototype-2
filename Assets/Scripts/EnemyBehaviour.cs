using TMPro;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public TextMeshProUGUI endText;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            endText.gameObject.SetActive(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        endText.gameObject.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        endText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
