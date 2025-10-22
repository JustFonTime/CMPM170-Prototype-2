using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDetection : MonoBehaviour
{
    public enum RangeDetectionStage
    {
        Close,
        Normal,
        Far
    }

    public float offset;

    public float range;
    public TextMeshProUGUI rangeText;
    public Color color;

    public RangeDetectionStage stage;
    [Range(0, 100)] public float distance;  // 0: Close , 100: Far

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            stage = RangeDetectionStage.Close;
        }
    }
    private void Awake()
    {
        stage = RangeDetectionStage.Close;
        distance = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // color for text will be dependent on how far player is from enemy
        color = Color.Lerp(Color.red, Color.green, distance / 50);
    }

    private void FixedUpdate()
    {
        // Determine where the player is 
        RaycastHit hitInfo;

        Vector3 raycastOffset = new Vector3(transform.position.x, transform.position.y - offset, transform.position.z);

        Vector3 currPos = transform.position;
        currPos.y -= offset;

        Debug.DrawRay(raycastOffset, transform.TransformDirection(Vector3.forward) * range, Color.gold);
        Physics.Raycast(currPos, transform.TransformDirection(Vector3.forward), out hitInfo, range);

        if(hitInfo.collider != null)
        {
            //Debug.Log("Enemy is Detecting: " +  hitInfo.collider.gameObject.name);

            distance = hitInfo.distance;

            // EX: range of 45

            // if range >= 45 player is far, player is safe
            // if range >15 && <45 player is normal, player is safe
            // if range <= 15 player is close, player is in danger

      
            if (distance <= range / System.Enum.GetValues(typeof(RangeDetectionStage)).Length)
            {
                stage = RangeDetectionStage.Close;
                
            }
            else if(distance > (range / System.Enum.GetValues(typeof(RangeDetectionStage)).Length) && distance < range)
            {
                stage = RangeDetectionStage.Normal;
            }
        }
        else if (hitInfo.collider == null)
        {
            distance = range;
            stage = RangeDetectionStage.Far;
        }
        
        
        Debug.Log("Player is at range of: " + stage);
        rangeText.text = stage.ToString();
        rangeText.color = color;

     
        
    }
}
