using System.Collections.Generic;
using System.Linq;
using UnityEditor.TerrainTools;
using UnityEngine;

public class Node
{
    public Vector2 position;
    public float startCost;
    public float targetDistance;
    public float totalCost;
    public bool evaluated;
    public Node parent; 
    public List<Node> children;
}

public class PathfinderAI : MonoBehaviour
{
    public Vector2 target;
    Node startNode;
    public Rect searchArea;
    List<Node> OpenNodes = new List<Node>();
    Node[,] totalNodes;
    List<Vector2> path;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PathFind();
        }
    }

    void PathFind()
    {
        if (searchArea.x < target.x - transform.position.x)
        {
            Debug.LogWarning("Search area X was smaller than required, setting search area X to minimum required area");
            searchArea.x = target.x - transform.position.x;
        }
        if (searchArea.y < target.y - transform.position.y)
        {
            Debug.LogWarning("Search area Y was smaller than required, setting search area Y to minimum required area");
            searchArea.y = target.y - transform.position.y;
        }
        totalNodes = new Node[Mathf.RoundToInt(searchArea.size.x), Mathf.RoundToInt(searchArea.size.y)];

        for (int x = 0; x < searchArea.x; x++)
        {
            for (int y = 0; y < searchArea.y; y++)
            {
                Node node = new Node();
                node.position = new Vector2(x, y);
                node.startCost = Mathf.Infinity;
                node.targetDistance = Mathf.Infinity;
                node.totalCost = Mathf.Infinity;
                totalNodes[x, y] = node;
            }
        }
        startNode = totalNodes[0, 0];
        startNode.position = transform.position;
        startNode.startCost = 0;
        startNode.targetDistance = Vector2.Distance(startNode.position, target);
        startNode.totalCost = startNode.targetDistance;
        OpenNodes.Add(startNode);
        totalNodes[0, 0] = startNode;
        while (OpenNodes.Count > 0)
        {
            Node currentNode = OpenNodes.Aggregate((prev, current) => current.totalCost < prev.totalCost ? current : prev);
            for (int x = Mathf.RoundToInt(currentNode.position.x) - 1; x < Mathf.RoundToInt(currentNode.position.x) + 2; x++)
            {
                for (int y = Mathf.RoundToInt(currentNode.position.y) - 1; y < Mathf.RoundToInt(currentNode.position.y) + 2; y++)
                {
                    if (searchArea.Contains(new Vector2(x, y)))
                    {
                        if (new Vector2(x, y) != currentNode.position)
                        {
                            Node neigboringNode = totalNodes[x, y];
                            currentNode.children.Add(neigboringNode); 
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
                            totalNodes[x, y] = neigboringNode;

                        }
                    }

                }
            }
            OpenNodes.Remove(currentNode);
            currentNode.evaluated = true;
        }
        bool pathReconstructionComplete = false;
        while (!pathReconstructionComplete)
        {
            for(int i = 0; i < OpenNodes.Count; i++)
            {
                
            }
        }
    }
}
