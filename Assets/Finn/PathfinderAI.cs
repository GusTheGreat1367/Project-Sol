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
        Vector2 gridWorldOrigin = new Vector2(
        transform.position.x - halfWidth,
        transform.position.y - halfHeight
    );
        int startX = halfWidth;
        int startY = halfHeight;
        totalNodes = new Node[searchAreaWidth, searchAreaHeight];
        if (searchAreaWidth < Mathf.Abs(target.x - transform.position.x) || searchAreaHeight < Mathf.Abs(target.y - transform.position.y))
        {
            Debug.LogError("Start position is outside the defined search area bounds.");
            return null;
        }

        for (int x = 0; x < searchAreaWidth; x++)
        {
            for (int y = 0; y < searchAreaHeight; y++)
            {
                Node node = new Node
                {
                    position = new Vector2(gridWorldOrigin.x + x, gridWorldOrigin.y + y),
                    startCost = Mathf.Infinity,
                    targetDistance = Mathf.Infinity,
                    totalCost = Mathf.Infinity
                };

                totalNodes[x, y] = node;
            }
        }
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
            if (Vector2.Distance(currentNode.position, target) < 1f)
            {
                targetNode = currentNode;
                break;
            }
            for (int x = Mathf.RoundToInt(currentNode.position.x) - 1; x < Mathf.RoundToInt(currentNode.position.x) + 2; x++)
            {
                for (int y = Mathf.RoundToInt(currentNode.position.y) - 1; y < Mathf.RoundToInt(currentNode.position.y) + 2; y++)
                {
                    int gridX = Mathf.RoundToInt(x - gridWorldOrigin.x);
                    int gridY = Mathf.RoundToInt(y - gridWorldOrigin.y);

                    if (gridX < 0 || gridX >= searchAreaWidth || gridY < 0 || gridY >= searchAreaHeight)
                    {
                        continue;
                    }
                    if (new Vector2(x, y) != currentNode.position)
                    {
                        Node neigboringNode = totalNodes[gridX, gridY];
                        bool intersectingObj = false;
                        for(int i = 0; i < totalObjs.Length; i++)
                        {
                            SpriteRenderer spriteRenderer = (SpriteRenderer)totalObjs[i];
                            if (spriteRenderer != null)
                            {
                                if (spriteRenderer.bounds.Contains(new Vector3(neigboringNode.position.x, neigboringNode.position.y, transform.position.z)))
                                {
                                    neigboringNode.startCost = Mathf.Infinity;
                                    neigboringNode.targetDistance = Mathf.Infinity;
                                    neigboringNode.totalCost = Mathf.Infinity;
                                    totalNodes[gridX, gridY] = neigboringNode;
                                    intersectingObj = true;
                                    break;
                                }
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
                        }

                        totalNodes[gridX, gridY] = neigboringNode;

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
