using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Node
{
    public Vector2 position;
    public float startCost;
    public float targetDistance;
    public float totalCost;
    public bool evaluated;
    public Node parent;
}

public class PathfinderAI : MonoBehaviour
{
    public Vector2 target;

    public float speed;
    public float pathResolution = 0.5f;
    public int searchAreaWidth;
    public int searchAreaHeight;

    public List<Vector2> path = new List<Vector2>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        path = PathFind(target, searchAreaWidth, searchAreaHeight);
    }

    // Update is called once per frame
    void Update()
    {
        path = PathFind(target, searchAreaWidth, searchAreaHeight);
        for (int i = 1; i < path.Count; i++)
        {
            Debug.DrawLine(path[i - 1], path[i], Color.green);
        }

    }

    List<Vector2> PathFind(Vector2 target, int searchAreaWidth, int searchAreaHeight)
    {
        Node startNode;
        List<Node> OpenNodes = new List<Node>();
        Node[,] totalNodes;
        List<Vector2> path = new List<Vector2>();
        int halfWidth = searchAreaWidth / 2;
        int halfHeight = searchAreaHeight / 2;
        int nodeGridWidth = Mathf.RoundToInt(searchAreaWidth / pathResolution);
        int nodeGridHeight = Mathf.RoundToInt(searchAreaHeight / pathResolution);
        Vector2 gridWorldOrigin = new Vector2(
        transform.position.x - halfWidth,
        transform.position.y - halfHeight
    );
        totalNodes = new Node[nodeGridWidth,nodeGridHeight];
        if (searchAreaWidth < Mathf.Abs(target.x - transform.position.x) || searchAreaHeight < Mathf.Abs(target.y - transform.position.y))
        {
            Debug.LogError("Start position is outside the defined search area bounds.");
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
        int startX = Mathf.RoundToInt((transform.position.x - gridWorldOrigin.x) / pathResolution);
        int startY = Mathf.RoundToInt((transform.position.y - gridWorldOrigin.y) / pathResolution);
        startNode = totalNodes[startX, startY];
        startNode.position = transform.position;
        startNode.startCost = 0;
        startNode.targetDistance = Vector2.Distance(startNode.position, target);
        startNode.totalCost = startNode.targetDistance;
        OpenNodes.Add(startNode);
        object[] totalObjs = FindObjectsByType(typeof(SpriteRenderer), FindObjectsSortMode.None);
        Node targetNode = null;
        while (OpenNodes.Count > 0)
        {
            Node currentNode = OpenNodes.Aggregate((prev, current) => current.totalCost < prev.totalCost ? current : prev);
            if (Vector2.Distance(currentNode.position, target) < pathResolution)
            {
                targetNode = currentNode;
                break;
            }
            for (float x = currentNode.position.x - pathResolution; x <= currentNode.position.x + pathResolution; x += pathResolution)
            {
                for (float y = currentNode.position.y - pathResolution; y <= currentNode.position.y + pathResolution; y += pathResolution)
                {
                    int gridX = Mathf.RoundToInt((x - gridWorldOrigin.x) / pathResolution);
                    int gridY = Mathf.RoundToInt((y - gridWorldOrigin.y) / pathResolution);

                    if (gridX < 0 || gridX >= nodeGridWidth || gridY < 0 || gridY >= nodeGridHeight)
                    {
                        continue;
                    }
                    if (new Vector2(x, y) != currentNode.position)
                    {
                        Node neigboringNode = totalNodes[gridX, gridY];
                        bool intersectingObj = false;
                        for (int j = 0; j < 7; j++)
                        {
                            Vector2 currentBounds = gameObject.GetComponent<SpriteRenderer>().bounds.size * 0.9f;
                            if (Physics2D.OverlapBox(new Vector2(x,y), currentBounds, j * 45) != null)
                            {
                                neigboringNode.startCost = Mathf.Infinity;
                                neigboringNode.targetDistance = Mathf.Infinity;
                                neigboringNode.totalCost = Mathf.Infinity;
                                totalNodes[gridX, gridY] = neigboringNode;
                                intersectingObj = true;
                                break;
                            }
                        }
                        if (!intersectingObj)
                        {
                            float newStartCostScore = currentNode.startCost + Vector2.Distance(currentNode.position, neigboringNode.position);
                            neigboringNode.targetDistance = Vector2.Distance(neigboringNode.position, target);
                            if (newStartCostScore < neigboringNode.startCost)
                            {
                                neigboringNode.parent = currentNode;

                                neigboringNode.startCost = newStartCostScore;
                                neigboringNode.totalCost = neigboringNode.startCost + neigboringNode.targetDistance;
                                if (!OpenNodes.Contains(neigboringNode))
                                {
                                    OpenNodes.Add(neigboringNode);
                                }

                            }
                            totalNodes[gridX, gridY] = neigboringNode;
                        }



                    }

                }
            }
            OpenNodes.Remove(currentNode);
            currentNode.evaluated = true;
        }
        if (targetNode == null)
        {
            Debug.LogWarning("No path found to the target.");
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
