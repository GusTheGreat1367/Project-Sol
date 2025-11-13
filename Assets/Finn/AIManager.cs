
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PathRequest
{
    public PathFinderAI AI;
    public Task<List<Node>> PathfindingTask;
}
public class BoundsObj
{
    public Rect bounds;
    public int boundsGameObjID;
}
public class PathFinderAI
{
    public GameObject obj;
    public float velocity;
    public float maxVelocity = 5f;
    public List<Node> path = new List<Node>();
    public bool targetSet = false;
    public Vector2 targetPos;
    public int loops;
    public bool currentlyInAITask;
    public AIPathFindingData pathData = new AIPathFindingData();
    public int instanceID;
}
public class AIPathFindingData
{
    public Vector2 mapOrigin = new Vector2();
    public int mapWidth = new int();
    public int mapHeight = new int();
    public float pathResolution = new float();
}
public class ObstacleMapReturn
{
    public bool[,] obstacleMap;
    public PathFinderAI pathfinderAI;
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
    public float averageAITaskTime;
    private List<PathRequest> activePathRequests = new List<PathRequest>();
    private bool currentlyRunningObstacleMapTask;
    public Task<List<ObstacleMapReturn>> currentObstacleMapTask;
    public List<PathFinderAI> AIsWaitingForTask = new List<PathFinderAI>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        for (int i = 0; i < AINumber; i++)
        {
            PathFinderAI newAI = new PathFinderAI();
            newAI.obj = Instantiate(AIPrefab, new Vector2(UnityEngine.Random.Range(0, 40), UnityEngine.Random.Range(0, 40)), Quaternion.identity);
            newAI.obj.name = "AI " + i;
            newAI.instanceID = newAI.obj.gameObject.GetInstanceID();
            AIs.Add(newAI);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ProcessCompletedRequests();
        ProcessCompletedObstacleMapRequests();
        if (!currentlyRunningObstacleMapTask && AIsWaitingForTask.Count > 0)
        {
            currentObstacleMapTask = BeginObstacleDetection(AIsWaitingForTask);
            currentlyRunningObstacleMapTask = true;
            AIsWaitingForTask.Clear();

        }
        for (int i = 0; i < AIs.Count; i++)
        {
            PathFinderAI AI = AIs[i];
            AI.loops++;
            Vector2 AIPos = new Vector2(AI.obj.transform.position.x, AI.obj.transform.position.y);

            if (Vector2.Distance(AI.obj.transform.position, AI.targetPos) < 1f)
            {
                //Debug.Log("Target Reached!");
                AI.targetSet = false;
                AI.path = new List<Node>();
            }
            if (AI.targetSet == false)
            {
                //Debug.Log("AI " + i + " is pathing to a target");
                AI.targetPos = targets[Mathf.RoundToInt(UnityEngine.Random.Range(0, targets.Count))].transform.position;
                AI.targetSet = true;
                AI.currentlyInAITask = false;

            }
            else
            {

                if ((AI.loops > loopsBeforeUpdate || AI.path.Count == 0 || AI.path == null) && !AIsWaitingForTask.Contains(AI))
                {
                    AIsWaitingForTask.Add(AI);
                    float dx = Mathf.Abs(AI.targetPos.x - AIPos.x);
                    float dy = Mathf.Abs(AI.targetPos.y - AIPos.y);
                    int padding = 16;
                    int searchAreaWidth = Mathf.Max(1, Mathf.CeilToInt(dx * 2f) + padding);
                    int searchAreaHeight = Mathf.Max(1, Mathf.CeilToInt(dy * 2f) + padding);
                    int nodeGridWidth = Mathf.Max(1, Mathf.RoundToInt(searchAreaWidth / pathResolution));
                    int nodeGridHeight = Mathf.Max(1, Mathf.RoundToInt(searchAreaHeight / pathResolution));
                    AIs[i].pathData.mapOrigin = AIPos;
                    AIs[i].pathData.pathResolution = pathResolution;
                    AIs[i].pathData.mapWidth = searchAreaWidth;
                    AIs[i].pathData.mapHeight = searchAreaHeight;
                    AI.currentlyInAITask = true;
                    AI.loops = 0;
                }
                //if (AI.currentlyInAITask)
                //{
                //float dx = Mathf.Abs(AI.targetPos.x - AIPos.x);
                //float dy = Mathf.Abs(AI.targetPos.y - AIPos.y);
                //int padding = 8;
                //int searchAreaWidth = Mathf.Max(1, Mathf.CeilToInt(dx * 2f) + padding);
                //int searchAreaHeight = Mathf.Max(1, Mathf.CeilToInt(dy * 2f) + padding);
                //int nodeGridWidth = Mathf.Max(1, Mathf.RoundToInt(searchAreaWidth / pathResolution));
                //int nodeGridHeight = Mathf.Max(1, Mathf.RoundToInt(searchAreaHeight / pathResolution));
                //    if (currentObstacleMapTask != null && currentObstacleMapTask.IsCompleted)
                //    {
                //        if (currentObstacleMapTask.Result.pathfinderAIs.Find(x => AIs[i] == x) != null)
                //        {
                //            AStar.PathRequestData pathData = new AStar.PathRequestData();
                //            pathData.startPos = AIPos;
                //            pathData.targetPos = AI.targetPos;
                //            pathData.searchWidth = searchAreaWidth;
                //            pathData.searchHeight = searchAreaHeight;
                //            pathData.pathResolution = pathResolution;
                //            pathData.AIPos = AIPos;
                //            bool[,] currentObstacleMapTaskResult = currentObstacleMapTask.Result.obstacleMaps[currentObstacleMapTask.Result.pathfinderAIs.FindIndex(x => AIs[i] == x)];
                //            pathData.obstacleMap = currentObstacleMapTaskResult;
                //            PathRequest request = new PathRequest
                //            {
                //                AI = AI,
                //                PathfindingTask = Task.Run(() => AStar.FindPathAStar(pathData))
                //            };

                //            activePathRequests.Add(request);
                //            //AI.path = PathFind(AI.targetPos, searchAreaWidth, searchAreaHeight, AI.obj, pathResolution);
                //            AI.currentlyInAITask = false;
                //        }
                //    }
                //    else if (currentObstacleMapTask == null)
                //    {
                //        AI.currentlyInAITask = false;
                //    }
                //}
                //else if (!AI.currentlyInAITask && AI.loops > loopsBeforeUpdate)
                //{
                //    float dx = Mathf.Abs(AI.targetPos.x - AIPos.x);
                //    float dy = Mathf.Abs(AI.targetPos.y - AIPos.y);
                //    int padding = 8;
                //    int searchAreaWidth = Mathf.Max(1, Mathf.CeilToInt(dx * 2f) + padding);
                //    int searchAreaHeight = Mathf.Max(1, Mathf.CeilToInt(dy * 2f) + padding);
                //    AIsWaitingForTask.Add(AIs[i]);
                //    AIs[i].currentlyInAITask = true;
                //    AIs[i].loops = 0;
                //    Debug.Log(AIPos);

                //    Debug.Log("prepared for pathfinding " + AI.pathData);
                //}



                //Debug.Log(AI.path.Count);
                if (AI.path != null && AI.path.Count > 0)
                {
                    /*
                    for (int j = 1; j < AI.path.Count; j++)
                    {
                        Debug.DrawLine(AI.path[j - 1].position, AI.path[j].position);
                    }
                    */
                    Vector2 moveDir = AI.path[0].dir;
                    //Debug.Log("Moved AI " + i + " from position " + AIPos + " to position " + (AI.obj.transform.position + (Vector3)(moveDir * AISpeed * Time.deltaTime)) + ". AI Move Direction is " + moveDir + ". Current AI Path node is at " + AI.path[0] + ". AI Distance from Node is " + Vector2.Distance(AIPos, AI.path[0]));
                    //AI.obj.transform.position += (Vector3)(moveDir * AISpeed * Time.deltaTime);
                    AI.obj.transform.position = Vector2.MoveTowards(AI.obj.transform.position, AI.path[0].position, AISpeed * Time.deltaTime);
                    //float angle = Vector2.SignedAngle(Vector2.down, moveDir);
                    //AI.obj.transform.eulerAngles = new Vector3(0, 0, angle);
                    //Debug.Log(AI.obj.transform.position);
                    //AI.obj.transform.Translate(moveDir * AISpeed * Time.deltaTime, Space.World);
                    //AI.obj.transform.position = Vector2.MoveTowards(AI.obj.transform.position, nextNode, AISpeed * Time.deltaTime);


                    if (Vector2.Distance(AI.obj.transform.position, AI.path[0].position) < 0.1f)
                    {
                        //Debug.Log(AI.path.Count);
                        AI.path.RemoveAt(0);
                        //Debug.Log("rebuilt list");
                    }
                }
                AI.loops++;
            }
            AIs[i] = AI;
        }
    }
    public bool PathUpdate(List<Node> Path, PathFinderAI AI)
    {
        float sizeX = AI.obj.GetComponent<Renderer>().localBounds.size.x * AI.obj.transform.localScale.x;
        float sizeY = AI.obj.GetComponent<Renderer>().localBounds.size.y * AI.obj.transform.localScale.y;
        for (int i = 0; i < Path.Count; i++)
        {
            bool hitObj = false;

            for (int j = 0; j < 7; j++)
            {
                Collider2D hit = Physics2D.OverlapBox(Path[i].position, new Vector2(sizeX, sizeY), j * 45);
                if (hit != null)
                {
                    hitObj = true;
                    break;
                }
            }
            if (hitObj)
            {
                return true;
            }
        }
        return false;
    }
    private void ProcessCompletedObstacleMapRequests()
    {
        if (currentObstacleMapTask != null && currentObstacleMapTask.IsCompleted)
        {
            if (currentObstacleMapTask.Status == TaskStatus.RanToCompletion)
            {
                Debug.Log("Preparing to pathfind");
                for (int i = 0; i < currentObstacleMapTask.Result.Count; i++)
                {
                    PathFinderAI pathFinderAI = currentObstacleMapTask.Result[i].pathfinderAI;
                    AStar.PathRequestData pathData = new();

                    pathData.AIPos = pathFinderAI.obj.transform.position;
                    pathData.targetPos = pathFinderAI.targetPos;
                    pathData.obstacleMap = currentObstacleMapTask.Result[i].obstacleMap;
                    pathData.pathResolution = pathResolution;
                    int nodeGridWidth = Mathf.Max(1, Mathf.RoundToInt(pathData.searchWidth / pathData.pathResolution));
                    int nodeGridHeight = Mathf.Max(1, Mathf.RoundToInt(pathData.searchHeight / pathData.pathResolution));
                    pathData.searchHeight = currentObstacleMapTask.Result[i].obstacleMap.GetLength(1);
                    pathData.searchWidth = currentObstacleMapTask.Result[i].obstacleMap.GetLength(0);
                    PathRequest request = new PathRequest
                    {
                        AI = pathFinderAI,
                        PathfindingTask = Task.Run(() => AStar.FindPathAStar(pathData))
                    };
                    Debug.Log("Begun Pathfinding!");
                    activePathRequests.Add(request);
                }
                currentObstacleMapTask = null;
                currentlyRunningObstacleMapTask = false;


            }
            else if (currentObstacleMapTask.Status == TaskStatus.Faulted)
            {
                Debug.LogError($"Obstacle Task is faulted: {currentObstacleMapTask.Exception.InnerException}");
            }
        }
    }
    private void ProcessCompletedRequests()
    {
        // Use ToList() to allow modification of activePathRequests inside the loop (via RemoveAll)
        List<PathRequest> completed = activePathRequests.Where(r => r.PathfindingTask.IsCompleted).ToList();

        foreach (var request in completed)
        {
            if (request.PathfindingTask.Status == TaskStatus.RanToCompletion)
            {
                request.AI.path = request.PathfindingTask.Result;
            }
            else if (request.PathfindingTask.IsFaulted)
            {
                Debug.LogError($"Pathfinding Task for {request.AI.obj.name} failed: {request.PathfindingTask.Exception.InnerException}");
                request.AI.path = null;
            }
        }

        activePathRequests.RemoveAll(r => r.PathfindingTask.IsCompleted);
    }

    public static void DrawRectangle(Vector3 position, Vector2 extent, Color color)
    {
        // Calculate the four corner points relative to the center
        Vector3 rightOffset = Vector3.right * extent.x;
        Vector3 upOffset = Vector3.up * extent.y;

        Vector3 p1 = position + rightOffset + upOffset;  // Top-Right
        Vector3 p2 = position - rightOffset + upOffset;  // Top-Left
        Vector3 p3 = position - rightOffset - upOffset;  // Bottom-Left
        Vector3 p4 = position + rightOffset - upOffset;  // Bottom-Right

        // Draw the four lines connecting the corners
        Debug.DrawLine(p1, p2, color); // Top
        Debug.DrawLine(p2, p3, color); // Left
        Debug.DrawLine(p3, p4, color); // Bottom
        Debug.DrawLine(p4, p1, color); // Right
    }
    public async Task<List<ObstacleMapReturn>> ReturnObstacleMap(List<Vector2> mapOrigins, List<int> mapWidths, List<int> mapHeights, List<PathFinderAI> AIs, List<float> pathResolutions)
    {
        const int AIsPerFrame = 5;
        int AIsDoneInBatch = 0;
        List<ObstacleMapReturn> obstacleMapReturn = new List<ObstacleMapReturn>();
        Debug.Log(mapWidths.Count + " Mapwidths count");
        List<float> sizeX = new List<float>();
        List<float> sizeY = new List<float>();
        List<int> halfWidth = new List<int>();
        List<int> halfHeight = new List<int>();
        List<int> nodeGridWidth = new List<int>();
        List<int> nodeGridHeight = new List<int>();
        List<bool[,]> obstacleMap = new List<bool[,]>();
        List<Vector2> gridWorldOrigin = new();
        int AICount = AIs.Count;
        Debug.Log("AIs count " + AICount);
        for (int i = 0; i < AICount; i++)
        {
            sizeX.Add(AIs[i].obj.GetComponent<Renderer>().localBounds.size.x * AIs[i].obj.transform.localScale.x);
            sizeY.Add(AIs[i].obj.GetComponent<Renderer>().localBounds.size.y * AIs[i].obj.transform.localScale.y);
            Debug.Log(i);
            halfWidth.Add(mapWidths[i] / 2);
            halfHeight.Add(mapHeights[i] / 2);
            nodeGridWidth.Add(Mathf.Max(1, Mathf.RoundToInt(mapWidths[i] / pathResolutions[i])));
            nodeGridHeight.Add(Mathf.Max(1, Mathf.RoundToInt(mapHeights[i] / pathResolutions[i])));
            obstacleMap.Add(new bool[nodeGridWidth[i], nodeGridHeight[i]]);
            gridWorldOrigin.Add(new Vector2(AIs[i].obj.transform.position.x - halfWidth[i], AIs[i].obj.transform.position.y - halfHeight[i]));
            for (int x = 0; x < nodeGridWidth[i]; x++)
            {
                for (int y = 0; y < nodeGridHeight[i]; y++)
                {
                    Vector2 nodePos = new Vector2(
                        gridWorldOrigin[i].x + x * pathResolutions[i],
                        gridWorldOrigin[i].y + y * pathResolutions[i]
                    );
                    bool hitObj = false;
                    for (int r = 0; r < 7; r++)
                    {

                        Collider2D hit = Physics2D.OverlapBox(nodePos, new Vector2(sizeX[i], sizeY[i]), r * 45);

                        if (hit != null)
                        {
                            hitObj = true;
                            break;
                        }
                    }
                    obstacleMap[i][x, y] = hitObj;
                }

            }
            obstacleMapReturn.Add(new ObstacleMapReturn
            {
                obstacleMap = obstacleMap[i],
                pathfinderAI = AIs[i]
            });
            obstacleMapReturn[i].obstacleMap = obstacleMap[i];
            obstacleMapReturn[i].pathfinderAI = AIs[i];
            AIsDoneInBatch++;
            if (AIsDoneInBatch > AIsPerFrame)
            {
                AIsDoneInBatch = 0;

                await Awaitable.NextFrameAsync();
            }
        }

        return obstacleMapReturn;
    }
    public bool Contains(Rect outerRect, Rect innerRect)
    {
        bool minContained = innerRect.xMin >= outerRect.xMin && innerRect.yMin >= outerRect.yMin;
        bool maxContained = innerRect.xMax <= outerRect.xMax && innerRect.yMax <= outerRect.yMax;

        return minContained && maxContained;
    }
    public Task<List<ObstacleMapReturn>> BeginObstacleDetection(List<PathFinderAI> AI)
    {
        List<int> mapWidth = new List<int>();
        List<int> mapHeight = new List<int>();
        List<float> pathResolution = new List<float>();
        List<Vector2> mapOrigin = new List<Vector2>();
        for (int i = 0; i < AI.Count; i++)
        {
            try
            {
                mapWidth.Add(AI[i].pathData.mapWidth);
                mapHeight.Add(AI[i].pathData.mapHeight);
                pathResolution.Add(AI[i].pathData.pathResolution);
                mapOrigin.Add(AI[i].pathData.mapOrigin);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error collecting path data for AI {AI[i].obj.name} at index {i}. The lists may now be mismatched. Error: {e.Message}");
                // Optional: Break the loop or return null here to prevent the mismatched lists from causing the later error.
                return null; // Return a null task if data collection fails for safety.
            }
        }
        return ReturnObstacleMap(mapOrigin, mapWidth, mapHeight, AI, pathResolution);
    }
}

public static class AStar
{
    public class PathRequestData
    {
        public Vector2 targetPos;
        public int searchWidth;
        public int searchHeight;
        public float pathResolution;
        public Vector2 AIPos;
        public bool[,] obstacleMap;
        public Task<List<Node>> pathFindingTask;
    }
    public static List<Node> FindPathAStar(PathRequestData pathData)
    {
        // basic guards
        if (pathData.pathResolution <= 0) pathData.pathResolution = 1f;
        Node startNode;
        List<Node> OpenNodes = new List<Node>();
        Node[,] totalNodes;
        List<Node> path = new List<Node>();
        int halfWidth = pathData.searchWidth / 2;
        int halfHeight = pathData.searchHeight / 2;
        int nodeGridWidth = Mathf.Max(1, Mathf.RoundToInt(pathData.searchWidth / pathData.pathResolution));
        int nodeGridHeight = Mathf.Max(1, Mathf.RoundToInt(pathData.searchHeight / pathData.pathResolution));
        Vector2 gridWorldOrigin = new Vector2(
        pathData.AIPos.x - pathData.searchWidth / 2f,
        pathData.AIPos.y - pathData.searchHeight / 2f
        );
        totalNodes = new Node[nodeGridWidth, nodeGridHeight];

        // ensure start is in bounds (recalculate origin if needed)
        if (pathData.AIPos.x < gridWorldOrigin.x || pathData.AIPos.x > gridWorldOrigin.x + pathData.searchWidth ||
            pathData.AIPos.y < gridWorldOrigin.y || pathData.AIPos.y > gridWorldOrigin.y + pathData.searchHeight)
        {
            Debug.LogError($"Start position {pathData.AIPos} is outside search area origin {gridWorldOrigin} size ({pathData.searchWidth},{pathData.searchHeight}). Expand search area or recenter.");
            return null;
        }
        for (int x = 0; x < nodeGridWidth; x++)
        {
            for (int y = 0; y < nodeGridHeight; y++)
            {
                Node node = new Node
                {
                    position = new Vector2(gridWorldOrigin.x + x * pathData.pathResolution, gridWorldOrigin.y + y * pathData.pathResolution),
                    startCost = Mathf.Infinity,
                    targetDistance = Mathf.Infinity,
                    totalCost = Mathf.Infinity,
                    parent = null,
                    evaluated = false
                };

                totalNodes[x, y] = node;
            }
        }
        int startX = Mathf.Clamp(Mathf.RoundToInt((pathData.AIPos.x - gridWorldOrigin.x) / pathData.pathResolution), 0, nodeGridWidth - 1);
        int startY = Mathf.Clamp(Mathf.RoundToInt((pathData.AIPos.y - gridWorldOrigin.y) / pathData.pathResolution), 0, nodeGridHeight - 1);
        startNode = totalNodes[startX, startY];
        startNode.position = pathData.AIPos;
        startNode.startCost = 0;
        startNode.targetDistance = Vector2.Distance(startNode.position, pathData.targetPos);
        startNode.totalCost = startNode.targetDistance;
        OpenNodes.Add(startNode);
        Node targetNode = null;

        // prepare obstacle mask (use "Obstacles" layer if exists, otherwise use all layers)
        //int obstacleMask = LayerMask.GetMask("Obstacles");
        //if (obstacleMask == 0) obstacleMask = ~0;

        while (OpenNodes.Count > 0)
        {
            Node currentNode = OpenNodes.Aggregate((prev, current) => current.totalCost < prev.totalCost ? current : prev);

            // If close enough to target we are done
            if (Vector2.Distance(currentNode.position, pathData.targetPos) < pathData.pathResolution * 0.9f)
            {
                targetNode = currentNode;
                break;
            }

            // get current node indices
            int currentNodeGridX = Mathf.Clamp(Mathf.RoundToInt((currentNode.position.x - gridWorldOrigin.x) / pathData.pathResolution), 0, nodeGridWidth - 1);
            int currentNodeGridY = Mathf.Clamp(Mathf.RoundToInt((currentNode.position.y - gridWorldOrigin.y) / pathData.pathResolution), 0, nodeGridHeight - 1);

            // iterate neighbors using grid offsets (avoids floating rounding issues)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    int gridX = currentNodeGridX + dx;
                    int gridY = currentNodeGridY + dy;

                    if (gridX < 0 || gridX >= nodeGridWidth || gridY < 0 || gridY >= nodeGridHeight)
                    {
                        continue;
                    }


                    Node neighboringNode = totalNodes[gridX, gridY];
                    Vector2 neighborPos = neighboringNode.position;
                    bool hitObj = false;
                    hitObj = pathData.obstacleMap[gridX, gridY];
                    if (hitObj == true)
                    {
                        neighboringNode.startCost = Mathf.Infinity;
                        neighboringNode.targetDistance = Mathf.Infinity;
                        neighboringNode.totalCost = Mathf.Infinity;
                        totalNodes[gridX, gridY] = neighboringNode;
                        continue;
                    }


                    float newStartCostScore = currentNode.startCost + Vector2.Distance(currentNode.position, neighboringNode.position);
                    neighboringNode.targetDistance = Vector2.Distance(neighboringNode.position, pathData.targetPos);
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

        if (targetNode == null)
        {
            Debug.LogWarning("No path found to the target. Possible causes: search area too small, obstacles block the route, or pathResolution is too large.");
            return null;
        }

        Node pathNode = targetNode;
        int i = 0;
        while (pathNode != null)
        {
            if (pathNode.parent != null)
            {
                pathNode.dir = (pathNode.parent.position - pathNode.position).normalized;
            }
            else
            {
                pathNode.dir = new Vector2(0, 0);
            }
            path.Add(pathNode);
            pathNode = pathNode.parent;
            i++;
        }
        //Debug.Log("Path found");
        path.Reverse();
        return path;
    }
}
