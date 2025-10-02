using NUnit.Framework;
using System.Collections.Generic;
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
                    Vector3 direction = new Vector3(AI.targetPos.x, AI.targetPos.y, 0) - AI.obj.transform.position;
                    float angle = Vector2.SignedAngle(Vector2.up, direction);
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
            AI.obj.transform.Translate(AI.obj.transform.up * AI.velocity * Time.deltaTime);
            AI.velocity -= .05f;
        }
    }

    public void pathUpdate(PathFinderAI AI)
    {
        AI.path.Add(AI.targetPos);
        Debug.Log("Path updated");
    }
}
