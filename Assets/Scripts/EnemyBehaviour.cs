using TMPro;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public TextMeshProUGUI endText;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "Player")
        {
            endText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
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
