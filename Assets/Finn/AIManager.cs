using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinderAI
{
    public GameObject obj;
    public float velocity;
    public float maxVelocity = 5f;
    public List<Vector2> path = new List<Vector2>();
    public bool targetSet = false;
    public Vector2 targetPos;
    public int loops;
}
public class AIManager : MonoBehaviour
{
    public List<PathFinderAI> AIs = new List<PathFinderAI>();
    public int AINumber;
    public GameObject AIPrefab;
    public List<GameObject> targets;
    public float pathResolution;
    public int loopsBeforeUpdate;
    public float AISpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < AINumber; i++)
        {
            PathFinderAI newAI = new PathFinderAI();
            newAI.obj = Instantiate(AIPrefab, new Vector2(Random.Range(0, 40), Random.Range(0, 40)), Quaternion.identity);
            newAI.obj.name = "AI " + i;
            AIs.Add(newAI);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < AIs.Count; i++)
        {
            PathFinderAI AI = AIs[i];
            AI.loops++;
            Vector2 AIPos = new Vector2(AI.obj.transform.position.x, AI.obj.transform.position.y);

            if (Vector2.Distance(AI.obj.transform.position, AI.targetPos) < 1)
            {
                Debug.Log("Target Reached!");
                AI.targetSet = false;
                AI.path = new List<Vector2>();
            }
            if (AI.targetSet == false)
            {
                Debug.Log("AI " + i + " is pathing to a target");
                AI.targetPos = targets[Mathf.RoundToInt(Random.Range(0, targets.Count))].transform.position;
                AI.targetSet = true;
            }
            else
            {
                if (AI.loops > loopsBeforeUpdate)
                {
                    AI.loops = 0;
                    float dx = Mathf.Abs(AI.targetPos.x - AIPos.x);
                    float dy = Mathf.Abs(AI.targetPos.y - AIPos.y);
                    int padding = 8;
                    int searchAreaWidth = Mathf.Max(1, Mathf.CeilToInt(dx * 2f) + padding);
                    int searchAreaHeight = Mathf.Max(1, Mathf.CeilToInt(dy * 2f) + padding);
                    AI.path = PathFind(AI.targetPos, searchAreaWidth, searchAreaHeight, AI.obj, pathResolution);
                    //AI.path.Add(AI.obj.transform.position);
                    //AI.path.Add(AI.targetPos);

                    if (AI.path == null)
                    {
                        Debug.LogError("AI " + i + " was unable to path to target");
                    }
                }

                if (AI.path != null)
                {
                    Vector2 nextNode = AI.path[0];
                    Vector2 currentPos = AI.obj.transform.position;
                    for (int j = 1; j < AI.path.Count; j++)
                    {
                        Debug.DrawLine(AI.path[j - 1], AI.path[j]);
                    }
                    Vector2 moveDir = (nextNode - currentPos).normalized;
                    //Debug.Log("Moved AI " + i + " from position " + AIPos + " to position " + (AI.obj.transform.position + (Vector3)(moveDir * AISpeed * Time.deltaTime)) + ". AI Move Direction is " + moveDir + ". Current AI Path node is at " + AI.path[0] + ". AI Distance from Node is " + Vector2.Distance(AIPos, AI.path[0]));
                    //AI.obj.transform.position += (Vector3)(moveDir * AISpeed * Time.deltaTime);
                    float angle = Vector2.SignedAngle(Vector2.up, moveDir);
                    AI.obj.transform.eulerAngles = new Vector3(0, 0, angle);
                    Debug.Log(AI.obj.transform.position);
                    AI.obj.transform.Translate(AI.obj.transform.up * AISpeed * Time.deltaTime, Space.World);
                    //AI.obj.transform.position = Vector2.MoveTowards(AI.obj.transform.position, nextNode, AISpeed * Time.deltaTime);


                    if (Vector2.Distance(AI.obj.transform.position, nextNode) < 0.1f)
                    {
                        Debug.Log(AI.path.Count);
                        AI.path.RemoveAt(0);
                        //Debug.Log("rebuilt list");
                    }
                }
            }
            AIs[i] = AI;
        }

    }

    List<Vector2> PathFind(Vector2 target, int searchAreaWidth, int searchAreaHeight, GameObject AI, float pathResolution)
    {
        // basic guards
        if (pathResolution <= 0) pathResolution = 1f;
        Node startNode;
        List<Node> OpenNodes = new List<Node>();
        Node[,] totalNodes;
        List<Vector2> path = new List<Vector2>();
        int halfWidth = searchAreaWidth / 2;
        int halfHeight = searchAreaHeight / 2;
        int nodeGridWidth = Mathf.Max(1, Mathf.RoundToInt(searchAreaWidth / pathResolution));
        int nodeGridHeight = Mathf.Max(1, Mathf.RoundToInt(searchAreaHeight / pathResolution));
        Vector2 gridWorldOrigin = new Vector2(
        AI.transform.position.x - halfWidth,
        AI.transform.position.y - halfHeight
    );
        totalNodes = new Node[nodeGridWidth, nodeGridHeight];

        // ensure start is in bounds (recalculate origin if needed)
        if (AI.transform.position.x < gridWorldOrigin.x || AI.transform.position.x > gridWorldOrigin.x + searchAreaWidth ||
            AI.transform.position.y < gridWorldOrigin.y || AI.transform.position.y > gridWorldOrigin.y + searchAreaHeight)
        {
            Debug.LogError($"Start position {AI.transform.position} is outside search area origin {gridWorldOrigin} size ({searchAreaWidth},{searchAreaHeight}). Expand search area or recenter.");
            return null;
        }

        for (int x = 0; x < nodeGridWidth; x++)
        {
            for (int y = 0; y < nodeGridHeight; y++)
            {
                Node node = new Node
                {
                    position = new Vector2(gridWorldOrigin.x + x * pathResolution, gridWorldOrigin.y + y * pathResolution),
                    startCost = Mathf.Infinity,
                    targetDistance = Mathf.Infinity,
                    totalCost = Mathf.Infinity
                };

                totalNodes[x, y] = node;
            }
        }
        int startX = Mathf.Clamp(Mathf.RoundToInt((AI.transform.position.x - gridWorldOrigin.x) / pathResolution), 0, nodeGridWidth - 1);
        int startY = Mathf.Clamp(Mathf.RoundToInt((AI.transform.position.y - gridWorldOrigin.y) / pathResolution), 0, nodeGridHeight - 1);
        startNode = totalNodes[startX, startY];
        startNode.position = AI.transform.position;
        startNode.startCost = 0;
        startNode.targetDistance = Vector2.Distance(startNode.position, target);
        startNode.totalCost = startNode.targetDistance;
        OpenNodes.Add(startNode);
        object[] totalObjs = FindObjectsByType(typeof(SpriteRenderer), FindObjectsSortMode.None);
        Node targetNode = null;

        // prepare obstacle mask (use "Obstacles" layer if exists, otherwise use all layers)
        int obstacleMask = LayerMask.GetMask("Obstacles");
        if (obstacleMask == 0) obstacleMask = ~0;

        // temporarily disable AI collider to avoid self-blocking
        Collider2D aiCollider = AI.GetComponent<Collider2D>();
        bool aiColliderWasEnabled = false;
        if (aiCollider != null)
        {
            aiColliderWasEnabled = aiCollider.enabled;
            aiCollider.enabled = false;
        }

        while (OpenNodes.Count > 0)
        {
            Node currentNode = OpenNodes.Aggregate((prev, current) => current.totalCost < prev.totalCost ? current : prev);

            // If close enough to target we are done
            if (Vector2.Distance(currentNode.position, target) < pathResolution * 0.6f)
            {
                targetNode = currentNode;
                break;
            }

            // get current node indices
            int currentNodeGridX = Mathf.Clamp(Mathf.RoundToInt((currentNode.position.x - gridWorldOrigin.x) / pathResolution), 0, nodeGridWidth - 1);
            int currentNodeGridY = Mathf.Clamp(Mathf.RoundToInt((currentNode.position.y - gridWorldOrigin.y) / pathResolution), 0, nodeGridHeight - 1);

            // iterate neighbors using grid offsets (avoids floating rounding issues)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int gridX = currentNodeGridX + dx;
                    int gridY = currentNodeGridY + dy;

                    if (gridX < 0 || gridX >= nodeGridWidth || gridY < 0 || gridY >= nodeGridHeight)
                        continue;

                    Node neighboringNode = totalNodes[gridX, gridY];
                    Vector2 neighborPos = neighboringNode.position;

                    // Use a small overlap test at the neighbor position to detect obstacles.
                    // OverlapCircle is simpler and robust for grid-based checks.
                    float checkRadius = Mathf.Max(0.1f, pathResolution * 0.45f);
                    Collider2D hit = Physics2D.OverlapCircle(neighborPos, checkRadius, obstacleMask);

                    if (hit != null)
                    {
                        // blocked by obstacle
                        neighboringNode.startCost = Mathf.Infinity;
                        neighboringNode.targetDistance = Mathf.Infinity;
                        neighboringNode.totalCost = Mathf.Infinity;
                        totalNodes[gridX, gridY] = neighboringNode;
                        // optional debug:
                        // Debug.DrawLine(neighborPos - Vector2.one*checkRadius, neighborPos + Vector2.one*checkRadius, Color.red, 1f);
                        continue;
                    }

                    float newStartCostScore = currentNode.startCost + Vector2.Distance(currentNode.position, neighboringNode.position);
                    neighboringNode.targetDistance = Vector2.Distance(neighboringNode.position, target);
                    if (newStartCostScore < neighboringNode.startCost)
                    {
                        neighboringNode.parent = currentNode;
                        neighboringNode.startCost = newStartCostScore;
                        neighboringNode.totalCost = neighboringNode.startCost + neighboringNode.targetDistance;
                        if (!OpenNodes.Contains(neighboringNode))
                        {
                            OpenNodes.Add(neighboringNode);
                        }
                    }
                    totalNodes[gridX, gridY] = neighboringNode;
                }
            }

            OpenNodes.Remove(currentNode);
            currentNode.evaluated = true;
        }

        // restore AI collider
        if (aiCollider != null)
        {
            aiCollider.enabled = aiColliderWasEnabled;
        }

        if (targetNode == null)
        {
            Debug.LogWarning("No path found to the target. Possible causes: search area too small, obstacles block the route, or pathResolution is too large.");
            return null;
        }

        Node pathNode = targetNode;
        while (pathNode != null)
        {
            path.Add(pathNode.position);
            pathNode = pathNode.parent;
        }
        Debug.Log("Path found");
        path.Reverse();
        return path;
    }
}
