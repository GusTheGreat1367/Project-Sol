using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PathFinderAI
{
    public GameObject obj;
    public float velocity;
    public float maxVelocity = 5f;
    public List<Vector2> path = new List<Vector2>();
    public bool targetSet = false;
    public Vector2 targetPos;
}
public class AIManager : MonoBehaviour
{
    public List<PathFinderAI> AIs = new List<PathFinderAI>();
    public int AINumber;
    public GameObject AIPrefab;
    public List<GameObject> targets;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i = 0; i < AINumber; i++)
        {
            PathFinderAI newAI = new PathFinderAI();
            newAI.obj = Instantiate(AIPrefab, Vector2.zero, Quaternion.identity);
            newAI.obj.name = "AI " + i;
            AIs.Add(newAI);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < AIs.Count; i++)
        {
            PathFinderAI AI = AIs[i];
            Vector2 AIPos = new Vector2(AI.obj.transform.position.x, AI.obj.transform.position.y);

            if (Vector2.Distance(AIPos, AI.targetPos) < 1)
            {
                Debug.Log("Target Reached!");
                AI.targetSet = false;
                AI.path = new List<Vector2>();
            }
            if(AI.targetSet == false)
            {
                Debug.Log("AI " + i + " is pathing to a target");
                AI.targetPos = targets[Mathf.RoundToInt(Random.Range(0, targets.Count))].transform.position;
                AI.targetSet = true;
            }
            else
            {
                if(AI.path.Count != 0)
                {
                   
                    LayerMask mask = LayerMask.GetMask("AI");
                    //Entirely for debugging purposes, if script becomes a multithreaded script, this won't work
                    AI.obj.GetComponent<Collider2D>().enabled = false;
                    RaycastHit2D hit = Physics2D.Raycast(AI.obj.transform.position, (AI.targetPos - AIPos).normalized, Mathf.Infinity, mask);
                    if (hit.collider != null)
                    {
                        Debug.DrawLine(AI.obj.transform.position, hit.point, Color.green);
                        Debug.Log(AI.obj.name + " hit " + hit.collider.name + " at pos " + hit.point);
                    }
                    AI.obj.GetComponent<Collider2D>().enabled = true;

                    Vector2 moveDir = (AI.path[0] - AIPos).normalized;
                    AI.obj.transform.position += (Vector3)(moveDir * AI.velocity * Time.deltaTime);
                    float angle = Vector2.SignedAngle(Vector2.up, moveDir);
                    AI.obj.transform.rotation = Quaternion.Euler(0, 0, angle);
                    if (Vector2.Distance(AIPos, AI.path[0]) < 1)
                    {

                        List<Vector2> newPathingList = new List<Vector2>();
                        for (int j = 1; j < AI.path.Count; j++)
                        {
                            newPathingList.Add(AI.path[j]);
                        }
                        AI.path = newPathingList;

                    }
                }
                else
                {
                    pathUpdate(AI);
                }
                if (AI.velocity < AI.maxVelocity)
                {
                    AI.velocity += .1f;
                }
            }

            AI.velocity -= .05f;
        }
    }

    public void pathUpdate(PathFinderAI AI)
    {
        AI.path = new List<Vector2>();
        //Disableing and renabling the collider won't work if AI is multithreaded, this is a temporary solution
        AI.obj.GetComponent<Collider2D>().enabled = false;
        bool finishedPathing = false;
        
        Vector2 AIPos = new Vector2(AI.obj.transform.position.x, AI.obj.transform.position.y);
        LayerMask mask = LayerMask.GetMask("AI");

        //First pathing point is based off of the object
        RaycastHit2D hit = Physics2D.Raycast(AI.obj.transform.position, (AI.targetPos-AIPos).normalized, Mathf.Infinity, mask);

        if (hit.collider != null)
        {
            AI.path.Add(hit.point -= (AI.targetPos - AIPos).normalized * (AI.obj.GetComponent<SpriteRenderer>().sprite.bounds.size.y*transform.lossyScale.y));
            RaycastHit2D leftHit = Physics2D.Raycast(AI.path[0], (AI.targetPos - AIPos).normalized + new Vector2(-1, 0).normalized, Mathf.Infinity, mask);
            RaycastHit2D rightHit = Physics2D.Raycast(AI.path[0], (AI.targetPos - AIPos).normalized + new Vector2(1, 0).normalized, Mathf.Infinity, mask);
            if (leftHit.collider != null)
            {
                if(rightHit.collider != null)
                {
                    AI.path.Add(AI.path[0] + new Vector2(0, -1).normalized * 5);
                }
                else
                {
                    AI.path.Add(AI.path[0] + new Vector2(1, 0).normalized);
                }
            }
            else
            {
                AI.path.Add(AIPos + new Vector2(-1, 0).normalized);
            }

            Debug.DrawLine(AI.obj.transform.position, hit.point, Color.green);
            Debug.Log(AI.obj.name + " hit " + hit.collider.name);
        }
        else
        {
            finishedPathing = true;
            AI.path.Add(AI.targetPos);
        }
        while (!finishedPathing)
        {
            int AIPathLength = AI.path.Count - 1;
            hit = new RaycastHit2D();
            hit = Physics2D.Raycast(AI.path[AIPathLength], (AI.targetPos - AI.path[AIPathLength]).normalized, Mathf.Infinity, mask);

            if (hit.collider != null)
            {
                AI.path.Add(hit.point - (AI.targetPos - AI.path[AIPathLength]).normalized * (AI.obj.GetComponent<SpriteRenderer>().sprite.bounds.size.y * transform.lossyScale.y));
                RaycastHit2D leftHit = Physics2D.Raycast(AI.path[AIPathLength], (AI.targetPos - AI.path[AIPathLength]).normalized + new Vector2(-1, 0).normalized, Mathf.Infinity, mask);
                RaycastHit2D rightHit = Physics2D.Raycast(AI.path[AIPathLength], (AI.targetPos - AI.path[AIPathLength]).normalized + new Vector2(1, 0).normalized, Mathf.Infinity, mask);
                if (leftHit.collider != null)
                {
                    if (rightHit.collider != null)
                    {
                        AI.path.Add(AI.path[AIPathLength] + new Vector2(0, -1).normalized * 5);
                    }
                    else
                    {
                        AI.path.Add(AI.path[AIPathLength] + new Vector2(1, 0).normalized);
                    }
                }
                else
                {
                    AI.path.Add(AIPos + new Vector2(-1, 0).normalized);
                }

                Debug.DrawLine(AI.obj.transform.position, hit.point, Color.green);
                Debug.Log(AI.obj.name + " hit " + hit.collider.name);
            }
            else
            {
                finishedPathing = true;
            }
        }

        Debug.Log("Path updated");
        AI.obj.GetComponent<Collider2D>().enabled = true;
    }

    public Vector2 GetVector2Dir(float dirCW)
    {
        float radians = dirCW * Mathf.Deg2Rad;
        float xComponent = Mathf.Cos(radians);
        float yComponent = Mathf.Sin(radians);
        Vector2 directionV2 = new Vector2(xComponent, yComponent);
        return directionV2.normalized;
    }
}
