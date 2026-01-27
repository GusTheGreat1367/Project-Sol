using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


public class AIManager : MonoBehaviour
{
    [HideInInspector]
    public List<PathFinderAI> AIs = new List<PathFinderAI>();
    public int AINumber;
    public GameObject AIPrefab;
    public List<GameObject> targets;
    public float nodeSize = 1;
    public int loopsBeforeUpdate;
    public float AISpeed;
    public float averageAITaskTime;
    [HideInInspector]
    public Task<CompleteObstacleMapReturn> currentObstacleMapTask;
    [HideInInspector]
    public List<PathFinderAI> AIsWaitingForTask = new List<PathFinderAI>();
    ObstacleManager obstacleManager;
    SelectTest selector;
    public bool debugObstacleMap = false;
    public int flowFieldSize = 20;
    [HideInInspector]
    public List<FlowFieldReturn>[,] flowFields;
    [HideInInspector]
    public CompleteObstacleMapReturn[,] flowFieldObstacleMapReturns;
    [HideInInspector]
    public List<FlowFieldReturn> destinationFlowFields = new List<FlowFieldReturn>();

    private Float2Bounds mapBounds;

    private CompleteObstacleMapReturn highQualityMapReturn;

    //this is actually just a blank list for AStar flow field pathfinding to use because I didn't feel like making another pathfinding function
    private CompleteObstacleMapReturn lowQualityMapReturn;

    private int loops;

    public int loopsPerObstacleMap = 1000;


    private void Start()
    {
        selector = gameObject.GetComponent<SelectTest>();
        for (int i = 0; i < AINumber; i++)
        {
            PathFinderAI newAI = new PathFinderAI();

            newAI.obj = Instantiate(AIPrefab, new Vector2(UnityEngine.Random.Range(0, 40), UnityEngine.Random.Range(0, 40)), Quaternion.identity);
            //No need for obstacle on AI anymore, we can just use vector pushing
            //newAI.obj.gameObject.AddComponent<ObstacleObj>();
            newAI.obj.AddComponent<ObjectMono>();
            newAI.obj.name = "AI " + i;
            newAI.instanceID = newAI.obj.gameObject.GetInstanceID();
            selector.selectableGameObjs.Add(newAI.obj);
            selector.selectableObjs.Add(newAI);
            newAI.targetPos = targets[0].gameObject.transform.position;
            AIs.Add(newAI);
        }
        obstacleManager = FindFirstObjectByType(typeof(ObstacleManager)).GetComponent<ObstacleManager>();
        mapBounds = DetectObstaclesInPosition.CompleteObstacleMap(obstacleManager.GetObstaclesInScene(), 1, DetectObstaclesInPosition.SetupMapTargets(targets), obstacleManager.GetObjectsInScene()).bounds;
        if (!DetectObstaclesInPosition.IsInteger(mapBounds.size.x / flowFieldSize))
        {
            mapBounds.size.x = Mathf.CeilToInt(mapBounds.size.x / flowFieldSize) * flowFieldSize;
        }
        if (!DetectObstaclesInPosition.IsInteger(mapBounds.size.y / flowFieldSize))
        {
            mapBounds.size.y = Mathf.CeilToInt(mapBounds.size.y / flowFieldSize) * flowFieldSize;
        }
        Vector2 currentMin = mapBounds.position - mapBounds.size;
        Vector2 currentMax = mapBounds.position + mapBounds.size;
        float snappedMinX = Mathf.Floor(currentMin.x / flowFieldSize) * flowFieldSize;
        float snappedMinY = Mathf.Floor(currentMin.y / flowFieldSize) * flowFieldSize;
        float snappedMaxX = Mathf.Ceil(currentMax.x / flowFieldSize) * flowFieldSize;
        float snappedMaxY = Mathf.Ceil(currentMax.y / flowFieldSize) * flowFieldSize;
        mapBounds.SetMinMax(
            new Float2(snappedMinX, snappedMinY),
            new Float2(snappedMaxX, snappedMaxY)
        );

        // initialize tile arrays based on snapped mapBounds
        int sectorsX = Mathf.CeilToInt(mapBounds.size.x * 2f / flowFieldSize);
        int sectorsY = Mathf.CeilToInt(mapBounds.size.y * 2f / flowFieldSize);

        flowFields = new List<FlowFieldReturn>[sectorsX, sectorsY];
        flowFieldObstacleMapReturns = new CompleteObstacleMapReturn[sectorsX, sectorsY];

        for (int x = 0; x < sectorsX; x++)
        {
            for (int y = 0; y < sectorsY; y++)
            {
                flowFields[x, y] = new List<FlowFieldReturn>();

                var obr = new CompleteObstacleMapReturn
                {
                    obstacleMap = new List<Obstacle>[flowFieldSize + 1, flowFieldSize + 1],
                    obstacleMapBool = new bool[flowFieldSize + 1, flowFieldSize + 1],
                    bounds = new Float2Bounds(),
                    size = new Float2(flowFieldSize, flowFieldSize),
                    startArea = findSectorOrigin(new Vector2(x, y))
                };
                for (int ix = 0; ix < flowFieldSize + 1; ix++)
                {
                    for (int iy = 0; iy < flowFieldSize + 1; iy++)
                    {
                        obr.obstacleMap[ix, iy] = new List<Obstacle>();
                        obr.obstacleMapBool[ix, iy] = false;
                    }
                }

                flowFieldObstacleMapReturns[x, y] = obr;
            }
        }

        int totalLowQualityMapReturnWidth = Mathf.CeilToInt(mapBounds.size.x * 2 / nodeSize);
        int totalLowQualityMapReturnHeight = Mathf.CeilToInt(mapBounds.size.y * 2 / nodeSize);
        lowQualityMapReturn = new CompleteObstacleMapReturn
        {
            bounds = mapBounds,
            obstacleMap = new List<Obstacle>[totalLowQualityMapReturnWidth, totalLowQualityMapReturnHeight],
            obstacleMapBool = new bool[totalLowQualityMapReturnWidth, totalLowQualityMapReturnHeight],
            startArea = mapBounds.position - mapBounds.size,
            size = mapBounds.size * 2
        };

        highQualityMapReturn = DetectObstaclesInPosition.CompleteObstacleMap(
            obstacleManager.GetObstaclesInScene(),
            1,
            DetectObstaclesInPosition.SetupMapTargets(targets),
            DetectObstaclesInPosition.SetupObjects(AIs.Select(a => a.obj).ToList())
        );

        for (int x = 0; x < lowQualityMapReturn.obstacleMap.GetLength(0); x++)
        {
            for (int y = 0; y < lowQualityMapReturn.obstacleMap.GetLength(1); y++)
            {
                lowQualityMapReturn.obstacleMap[x, y] = new List<Obstacle>();
                lowQualityMapReturn.obstacleMapBool[x, y] = false;
            }
        }
        Debug.Log($"Low quality Map Return size is currently: x: {lowQualityMapReturn.obstacleMap.GetLength(0)}, y: {lowQualityMapReturn.obstacleMap.GetLength(1)}");
        Debug.Log($"Mapbounds is at position: x: {mapBounds.position.x}, y: {mapBounds.position.y}, size (extents): x: {mapBounds.size.x}, y: {mapBounds.size.y}");
        AIs[0].targetSet = true;
        AIs[0].targetPos = targets[0].transform.position;
        AIs[0].pathStatus = PathfindingStatus.Requested;
    }
    public Vector2 findCurrentSector(Vector2 position)
    {
        Vector2 currentSector = new Vector2();
        float rX = position.x - mapBounds.min.x;
        float rY = position.y - mapBounds.min.y;

        currentSector = new Vector2(Mathf.FloorToInt(rX / flowFieldSize), Mathf.FloorToInt(rY / flowFieldSize));

        return currentSector;
    }

    public Vector2 findSectorOrigin(Vector2 sector)
    {
        return new Vector2(
            mapBounds.min.x + sector.x * flowFieldSize,
            mapBounds.min.y + sector.y * flowFieldSize
        );
    }


    public void Update()
    {
        DrawRectangle(mapBounds.position, mapBounds.size, Color.green);
        //Debug.Log($"mapbounds size {mapBounds.size.x}, {mapBounds.size.y}");
        if (loops > loopsPerObstacleMap)
        {
            loops = 0;
            List<Obstacle> returnedObstacles = obstacleManager.GetObstaclesInScene();
            List<MapTarget> returnedTargets = DetectObstaclesInPosition.SetupMapTargets(targets);
            List<CustomObject> returnedObjects = DetectObstaclesInPosition.SetupObjects(AIs.Select(a => a.obj).ToList());
            currentObstacleMapTask = Task.Run(() => DetectObstaclesInPosition.CompleteObstacleMap(returnedObstacles, 1, returnedTargets, returnedObjects));
        }
        if (currentObstacleMapTask != null)
        {
            if (currentObstacleMapTask.IsCompleted)
            {
                mapBounds = currentObstacleMapTask.Result.bounds;
                if (!DetectObstaclesInPosition.IsInteger(mapBounds.size.x / flowFieldSize))
                {
                    mapBounds.size.x = Mathf.CeilToInt(mapBounds.size.x / flowFieldSize) * flowFieldSize;
                }
                if (!DetectObstaclesInPosition.IsInteger(mapBounds.size.y / flowFieldSize))
                {
                    mapBounds.size.y = Mathf.CeilToInt(mapBounds.size.y / flowFieldSize) * flowFieldSize;
                }
                Vector2 currentMin = mapBounds.position - mapBounds.size;
                Vector2 currentMax = mapBounds.position + mapBounds.size;
                float snappedMinX = Mathf.Floor(currentMin.x / flowFieldSize) * flowFieldSize;
                float snappedMinY = Mathf.Floor(currentMin.y / flowFieldSize) * flowFieldSize;
                float snappedMaxX = Mathf.Ceil(currentMax.x / flowFieldSize) * flowFieldSize;
                float snappedMaxY = Mathf.Ceil(currentMax.y / flowFieldSize) * flowFieldSize;
                mapBounds.SetMinMax(
                    new Float2(snappedMinX, snappedMinY),
                    new Float2(snappedMaxX, snappedMaxY)
                );
                int totalLowQualityMapReturnWidth = Mathf.CeilToInt(mapBounds.size.x * 2 / nodeSize);
                int totalLowQualityMapReturnHeight = Mathf.CeilToInt(mapBounds.size.y * 2 / nodeSize);
                lowQualityMapReturn = new CompleteObstacleMapReturn
                {
                    bounds = mapBounds,
                    obstacleMap = new List<Obstacle>[totalLowQualityMapReturnWidth, totalLowQualityMapReturnHeight],
                    obstacleMapBool = new bool[totalLowQualityMapReturnWidth, totalLowQualityMapReturnHeight],
                    startArea = mapBounds.position - mapBounds.size,
                    size = mapBounds.size * 2
                };
                for (int x = 0; x < lowQualityMapReturn.obstacleMap.GetLength(0); x++)
                {
                    for (int y = 0; y < lowQualityMapReturn.obstacleMap.GetLength(1); y++)
                    {
                        lowQualityMapReturn.obstacleMap[x, y] = new List<Obstacle>();
                        lowQualityMapReturn.obstacleMapBool[x, y] = false;
                    }
                }
                for (int x = 0; x < flowFields.GetLength(0); x++)
                {
                    for (int y = 0; y < flowFields.GetLength(1); y++)
                    {
                        CompleteObstacleMapReturn selectedFlowFieldObstacleMapReturn = flowFieldObstacleMapReturns[x, y];
                        selectedFlowFieldObstacleMapReturn.bounds = new Float2Bounds(findSectorOrigin(new Vector2(x, y)), new Float2(flowFieldSize, flowFieldSize));
                        selectedFlowFieldObstacleMapReturn.size = new Float2(flowFieldSize, flowFieldSize);

                        int offsetX = (int)(findSectorOrigin(new Vector2(x, y)).x - mapBounds.min.x);
                        int offSetY = (int)(findSectorOrigin(new Vector2(x, y)).y - mapBounds.min.y);
                        float highQualityResolution = 1f;
                        var highStart = highQualityMapReturn.startArea;
                        Vector2 sectorOriginWorld = findSectorOrigin(new Vector2(x, y));

                        for (int fX = 0; fX < flowFieldSize + 1; fX++)
                        {
                            for (int fY = 0; fY < flowFieldSize + 1; fY++)
                            {
                                // world coordinates for this cell in the sector
                                float worldX = sectorOriginWorld.x + fX * nodeSize;
                                float worldY = sectorOriginWorld.y + fY * nodeSize;

                                int hx = Mathf.FloorToInt((worldX - highStart.x) / highQualityResolution);
                                int hy = Mathf.FloorToInt((worldY - highStart.y) / highQualityResolution);

                                hx = Mathf.Clamp(hx, 0, highQualityMapReturn.obstacleMap.GetLength(0) - 1);
                                hy = Mathf.Clamp(hy, 0, highQualityMapReturn.obstacleMap.GetLength(1) - 1);

                                selectedFlowFieldObstacleMapReturn.obstacleMap[fX, fY] = highQualityMapReturn.obstacleMap[hx, hy];
                                selectedFlowFieldObstacleMapReturn.obstacleMapBool[fX, fY] = highQualityMapReturn.obstacleMapBool[hx, hy];
                            }
                        }
                        flowFieldObstacleMapReturns[x, y] = selectedFlowFieldObstacleMapReturn;
                    }
                }
                currentObstacleMapTask = null;
                //Debug.Log($"Mapbounds is at position: x: {mapBounds.position.x}, y: {mapBounds.position.y}, size: x: {mapBounds.size.x}, y: {mapBounds.size.y}");
            }
            else if (currentObstacleMapTask.IsFaulted)
            {
                Debug.LogError(currentObstacleMapTask.Exception.InnerException);
            }
        }
        for (int i = 0; i < AIs.Count; i++)
        {
            PathFinderAI AI = AIs[i];
            AI.currentSector = findCurrentSector(AI.obj.transform.position);
            AI.targetSector = findCurrentSector(AI.targetPos);
            if (AI.targetSet)
            {
                if (AI.pathStatus == PathfindingStatus.Faulted)
                {
                    Debug.LogError($"Alert! AI {i}'s pathfinding task has been marked as 'faulted'! find more info above this in the debug log. Retrying pathfinding task");
                    AI.pathStatus = PathfindingStatus.Requested;
                }
                if (AI.pathStatus == PathfindingStatus.Requested)
                {
                    Debug.Log($"AI {i}: path has been requested");
                    AI.pathfindingTask = null;
                    AI.pathStatus = PathfindingStatus.CalculatingFlowField;
                    PathFindTSInput data = new PathFindTSInput
                    {
                        AIPos = AI.obj.transform.position,
                        TargetPos = AI.targetPos,
                        obstaclesInScene = obstacleManager.GetObstaclesInScene(),
                        mapTargetsInScene = DetectObstaclesInPosition.SetupMapTargets(targets),
                        objectsInScene = DetectObstaclesInPosition.SetupObjects(AIs.Select(a => a.obj).ToList()),
                        flowFieldReturns = flowFields,
                        flowFieldSize = flowFieldSize,
                        mapbounds = mapBounds,
                        lowQualityMapReturn = lowQualityMapReturn,
                        obstacleMapReturns = flowFieldObstacleMapReturns,
                        nodeSize = nodeSize,
                        destinationFlowFields = destinationFlowFields,
                    };
                    AI.pathfindingTask = Task.Run(() => Pathfind(data));
                    Debug.Log($"AI {i}: flow field has begun calculating");
                }
                else if (AI.pathStatus == PathfindingStatus.CalculatingFlowField)
                {

                    if (AI.pathfindingTask.IsCompleted)
                    {
                        if (AI.pathfindingTask.Result == null)
                        {
                            AI.pathStatus = PathfindingStatus.Faulted;
                        }
                        else
                        {
                            Debug.Log($"AI {i}: flow field has been calculated");
                            AI.pathStatus = PathfindingStatus.FinishedFlowField;
                            AI.finishedPathfindingTask = AI.pathfindingTask.Result;
                            AI.pathfindingTask = null;
                            destinationFlowFields.Add(AI.finishedPathfindingTask.sectorPath[AI.finishedPathfindingTask.sectorPath.Count - 1]);
                            for (int j = 0; j < AI.finishedPathfindingTask.sectorPath.Count - 1; j++)
                            {
                                int positionX = AI.finishedPathfindingTask.sectorPath[j].nodeX;
                                int positionY = AI.finishedPathfindingTask.sectorPath[j].nodeY;
                                int exit = 0;
                                for (int k = 0; k < flowFields[positionX, positionY].Count; k++)
                                {
                                    if (flowFields[positionX, positionY][k].dir == AI.finishedPathfindingTask.sectorPath[j].dir)
                                    {
                                        exit = 1;
                                        break;
                                    }
                                }
                                if (exit == 0)
                                {
                                    flowFields[positionX, positionY].Add(AI.finishedPathfindingTask.sectorPath[j]);
                                }
                            }
                        }

                    }
                    else if (AI.pathfindingTask.IsFaulted)
                    {
                        AI.pathStatus = PathfindingStatus.Faulted;
                        Debug.LogError(AI.pathfindingTask.Exception.InnerException);
                    }
                }
                else if (AI.pathStatus == PathfindingStatus.FinishedFlowField)
                {
                    if (AI.finishedPathfindingTask != null && AI.finishedPathfindingTask.sectorPath.Count > 0)
                    {
                        var currentActiveField = AI.finishedPathfindingTask.sectorPath[0];
                        Vector2 sectorOrigin = findSectorOrigin(new Vector2(currentActiveField.nodeX, currentActiveField.nodeY));
                        Vector2 localIdx = WorldToLocalFieldIndex(AI.obj.transform.position, sectorOrigin);

                        int lx = (int)localIdx.x;
                        int ly = (int)localIdx.y;




                        Vector2 moveDir = new Vector2();
                        try
                        {
                            moveDir = currentActiveField.field[lx, ly].dir;
                        }
                        catch
                        {
                            moveDir = Vector2.zero;
                        }
                        if (moveDir != Vector2.zero)
                        {
                            AI.obj.transform.Translate(AISpeed * Time.deltaTime * moveDir);
                            Debug.Log("Moving in Dir " + moveDir);
                        }
                        else
                        {
                            if (AI.finishedPathfindingTask.sectorPath.Count > 1)
                            {
                                if (currentActiveField.pos != AI.finishedPathfindingTask.sectorPath[1].pos)
                                {
                                    Vector2 correctionDir = new Vector2();
                                    if (lx > flowFieldSize + 1)
                                    {
                                        correctionDir.x = -1;
                                    }
                                    else if (lx < flowFieldSize + 1)
                                    {
                                        correctionDir.x = 1;
                                    }
                                    if (ly > flowFieldSize + 1)
                                    {
                                        correctionDir.y = -1;
                                    }
                                    else if (ly < flowFieldSize + 1)
                                    {
                                        correctionDir.y = 1;
                                    }
                                    AI.obj.transform.Translate(correctionDir);
                                    Debug.Log("AI off course, trying to correct... Applying correction dir " + correctionDir);
                                }
                                else
                                {
                                    Vector2 nextSectorIdx = WorldToLocalFieldIndex(AI.obj.transform.position, findSectorOrigin(new Vector2(AI.finishedPathfindingTask.sectorPath[1].nodeX, AI.finishedPathfindingTask.sectorPath[1].nodeY)));

                                    Vector2 nudgeDir = Vector2.zero;

                                    if (nextSectorIdx.x > flowFieldSize + 1)
                                    {
                                        nudgeDir.x = -1;
                                    }
                                    else if (nextSectorIdx.x < 0)
                                    {
                                        nudgeDir.x = 1;
                                    }
                                    if (nextSectorIdx.y > flowFieldSize + 1)
                                    {
                                        nudgeDir.y = -1;
                                    }
                                    else if (nextSectorIdx.y < 0)
                                    {
                                        nudgeDir.y = 1;
                                    }

                                    AI.obj.transform.Translate(nudgeDir);
                                    Debug.Log("Next sector index is " + nextSectorIdx + "(before nudge)");
                                    Debug.Log("Applying nudge dir to AI " + nudgeDir);
                                    nextSectorIdx = WorldToLocalFieldIndex(AI.obj.transform.position, findSectorOrigin(new Vector2(AI.finishedPathfindingTask.sectorPath[1].nodeX, AI.finishedPathfindingTask.sectorPath[1].nodeY)));
                                    Debug.Log("Position after nudge: " + nextSectorIdx);
                                    if (nextSectorIdx.x >= 0 && nextSectorIdx.x <= flowFieldSize && nextSectorIdx.y >= 0 && nextSectorIdx.y <= flowFieldSize)
                                    {
                                        AI.finishedPathfindingTask.sectorPath.RemoveAt(0);

                                    }
                                }

                            }
                        }
                        //if (AI.finishedPathfindingTask.sectorPath.Count > 1)
                        //{
                        //    Vector2 nextSectorPos = findSectorOrigin(new Vector2(AI.finishedPathfindingTask.sectorPath[1].nodeX, AI.finishedPathfindingTask.sectorPath[1].nodeY));
                        //    Vector2 handoffDir = (nextSectorPos - (Vector2)AI.obj.transform.position).normalized;
                        //    AI.obj.transform.Translate(handoffDir * Time.deltaTime * AISpeed);
                        //}

                        if (Vector2.Distance(AI.obj.transform.position, AI.targetPos) < 0.5f)
                        {
                            AI.targetSet = false;
                            Debug.Log($"AI {i} has arrived!");
                        }
                        for (int j = 0; j < AI.finishedPathfindingTask.sectorPath.Count; j++)
                        {

                            Debug.DrawRay(AI.finishedPathfindingTask.sectorPath[j].pos, new Vector2(1, 0), Color.white);


                            for (int x = 0; x < AI.finishedPathfindingTask.sectorPath[j].field.GetLength(0); x++)
                            {
                                for (int y = 0; y < AI.finishedPathfindingTask.sectorPath[j].field.GetLength(1); y++)
                                {
                                    Debug.DrawRay(AI.finishedPathfindingTask.sectorPath[j].field[x, y].position, AI.finishedPathfindingTask.sectorPath[j].field[x, y].dir, Color.green);
                                }
                            }
                            Vector2 debugNodePos = sectorOrigin + new Vector2(lx * nodeSize + nodeSize * 0.5f, ly * nodeSize + nodeSize * 0.5f);
                            Debug.DrawLine(AI.obj.transform.position, debugNodePos, Color.blue);
                        }
                        Debug.Log(currentActiveField.field[lx, ly].dir + " lx: " + lx + " ly:" + ly + " sector origin: " + sectorOrigin);

                    }
                }
            }

            AIs[i] = AI;
        }
        loops++;
        //for(int x = 0; x < flowFields.GetLength(0); x++)
        //{
        //    for(int y = 0;y < flowFields.GetLength(1); y++)
        //    {
        //        Debug.DrawLine()
        //    }
        //}

    }
    public Path Pathfind(PathFindTSInput input)
    {
        Path path = new Path();
        AStar.PathRequestData requestData = new AStar.PathRequestData();
        Float2 AIPos = input.AIPos;
        requestData.AIPos = findSectorOrigin(findCurrentSector(AIPos));
        requestData.nodeSize = input.flowFieldSize;
        requestData.searchWidth = Mathf.CeilToInt(input.mapbounds.size.x * 2f);
        requestData.searchHeight = Mathf.CeilToInt(input.mapbounds.size.y * 2f);
        requestData.mapOrigin = input.mapbounds.min;
        requestData.rightAngles = true;
        requestData.obstacleMap = input.lowQualityMapReturn.obstacleMapBool;
        requestData.targetPos = findSectorOrigin(findCurrentSector(input.TargetPos));
        List<Node> AStarFlowMapPath = AStar.FindPathAStar(requestData);
        if (AStarFlowMapPath == null)
        {
            Debug.Log("A star pathfinding failed");
            return null;
        }
        List<FlowFieldReturn> flowFieldsPath = new List<FlowFieldReturn>();
        List<Float2> sectorVectorPath = new List<Float2>();
        for (int i = 0; i < AStarFlowMapPath.Count - 1; i++)
        {
            bool foundFlowField = false;
            Vector2 worldPos = AStarFlowMapPath[i].position;
            Debug.Log("flow map path position " + worldPos + " flow map path dir " + AStarFlowMapPath[i].dir);
            Vector2 sectorIdx = findCurrentSector(worldPos);
            int sx = (int)sectorIdx.x;
            int sy = (int)sectorIdx.y;
            sectorVectorPath.Add(new Vector2(sx, sy));
            if (sx >= 0 && sx < input.flowFieldReturns.GetLength(0) && sy >= 0 && sy < input.flowFieldReturns.GetLength(1) && input.flowFieldReturns[sx, sy].Count > 0)
            {
                List<FlowFieldReturn> selectedFlowFields = input.flowFieldReturns[sx, sy];
                for (int j = 0; j < selectedFlowFields.Count; j++)
                {
                    FlowFieldReturn selectedFlowField = selectedFlowFields[j];
                    if (selectedFlowField.dir == AStarFlowMapPath[i].dir)
                    {
                        flowFieldsPath.Add(selectedFlowField);
                        foundFlowField = true;
                        break;
                    }
                }

            }
            if (foundFlowField == false)
            {
                Vector2 dir = AStarFlowMapPath[i].dir;
                float halfSize = input.flowFieldSize * 0.5f;

                Vector2 sectorOrigin = findSectorOrigin(new Vector2(sx, sy));

                float nudge = input.nodeSize * 0.5f;
                Vector2 targetCenter = worldPos + (dir * (halfSize));

                Vector2 targetSize = (dir.x != 0)
                    ? new Vector2(input.nodeSize, input.flowFieldSize)
                    : new Vector2(input.flowFieldSize, input.nodeSize);

                Float2Bounds targetBounds = new Float2Bounds(new Float2(targetCenter.x, targetCenter.y), new Float2(targetSize.x, targetSize.y));
                Debug.Log($"Target bounds flow field {i}: Size: X: {targetSize.x}, Y: {targetSize.y}. Position: X: {targetCenter.x} Y: {targetCenter.y}. Dir: X: {dir.x} Y: {dir.y}");
                FlowField.FlowFieldRequestData request = new FlowField.FlowFieldRequestData
                {
                    dir = dir,
                    targetBounds = targetBounds,
                    targetPos = targetCenter,
                    mapOrigin = sectorOrigin,
                    searchWidth = input.flowFieldSize + 1,
                    searchHeight = input.flowFieldSize + 1,
                    nodeSize = input.nodeSize,
                    obstacleMap = input.obstacleMapReturns[sx, sy].obstacleMapBool,
                    nodePosInTotalMap = new Vector2(sx, sy)
                };
                Debug.Log(input.obstacleMapReturns[sx, sy].obstacleMapBool.GetLength(0));
                FlowFieldReturn returnedFlowField = FlowField.FindFlowFieldToRect(request);
                if (returnedFlowField != null)
                {
                    flowFieldsPath.Add(returnedFlowField);
                    sectorVectorPath.Add(new Vector2(sx, sy));
                }
                else
                {
                    Debug.LogError("Flow Field pathfinding failed!");
                    return null;
                }

            }
        }
        FlowFieldReturn destinationField = input.destinationFlowFields.Find(x => x.pos == Float2.ConvertToV2(input.TargetPos));
        if (destinationField == null)
        {
            Vector2 lastPathNodePos = AStarFlowMapPath[AStarFlowMapPath.Count - 1].position;
            Vector2 sectorIdx = findCurrentSector(lastPathNodePos);
            int sx = (int)sectorIdx.x;
            int sy = (int)sectorIdx.y;

            Vector2 sectorOrigin = findSectorOrigin(new Vector2(sx, sy));

            FlowField.FlowFieldRequestData request = new FlowField.FlowFieldRequestData
            {
                searchHeight = input.flowFieldSize + 1,
                searchWidth = input.flowFieldSize + 1,
                nodeSize = input.nodeSize,
                mapOrigin = sectorOrigin,
                obstacleMap = input.obstacleMapReturns[sx, sy].obstacleMapBool,
                targetPos = input.TargetPos,
                nodePosInTotalMap = new Vector2(sx, sy)
            };
            destinationField = FlowField.FindFlowFieldToPoint(request);
        }
        if (destinationField == null)
        {
            Debug.Log("Unable to get destination flow field");
            return null;
        }
        flowFieldsPath.Add(destinationField);
        sectorVectorPath.Add(destinationField.pos);
        path.sectorVectorPath = sectorVectorPath;
        path.sectorPath = flowFieldsPath;
        return path;


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
    public bool Contains(Rect outerRect, Rect innerRect)
    {
        bool minContained = innerRect.xMin >= outerRect.xMin && innerRect.yMin >= outerRect.yMin;
        bool maxContained = innerRect.xMax <= outerRect.xMax && innerRect.yMax <= outerRect.yMax;

        return minContained && maxContained;
    }



    public static class FlowField
    {
        public class FlowFieldRequestData
        {
            public Vector2 targetPos;
            //Only used for FindFlowFieldToRect
            public Float2Bounds targetBounds;
            public int searchWidth;
            public int searchHeight;
            public Vector2 mapOrigin;
            public float nodeSize;
            public bool[,] obstacleMap;
            public Vector2 dir;
            public Vector2 nodePosInTotalMap;
            public FlowFieldReturn pathFindingTask;
        }
        public static FlowFieldReturn FindFlowFieldToPoint(FlowFieldRequestData request)
        {
            Queue<FlowNode> openNodes = new Queue<FlowNode>();
            int nodeGridWidth = Mathf.Max(1, Mathf.CeilToInt(request.searchWidth / request.nodeSize));
            int nodeGridHeight = Mathf.Max(1, Mathf.CeilToInt(request.searchHeight / request.nodeSize));
            FlowNode[,] totalNodes = new FlowNode[nodeGridWidth, nodeGridHeight];

            if (request.targetPos.x < request.mapOrigin.x - 0.1f ||
                    request.targetPos.x > request.mapOrigin.x + request.searchWidth + 0.1f ||
                    request.targetPos.y < request.mapOrigin.y - 0.1f ||
                    request.targetPos.y > request.mapOrigin.y + request.searchHeight + 0.1f)
            {
                Debug.LogError($"Target {request.targetPos} is outside search area. Origin: {request.mapOrigin}, Size: {request.searchWidth}");
                return null;
            }

            for (int x = 0; x < nodeGridWidth; x++)
            {
                for (int y = 0; y < nodeGridHeight; y++)
                {
                    totalNodes[x, y] = new FlowNode
                    {
                        position = new Vector2(request.mapOrigin.x + x * request.nodeSize, request.mapOrigin.y + y * request.nodeSize),
                        totalCost = float.MaxValue,
                        gridPos = new Vector2Int(x, y)
                    };
                }
            }


            int startX = Mathf.RoundToInt((request.targetPos.x - request.mapOrigin.x) / request.nodeSize);
            int startY = Mathf.RoundToInt((request.targetPos.y - request.mapOrigin.y) / request.nodeSize);
            startX = Mathf.Clamp(startX, 0, nodeGridWidth - 1);
            startY = Mathf.Clamp(startY, 0, nodeGridHeight - 1);

            FlowNode startNode = totalNodes[startX, startY];
            startNode.totalCost = 0;
            openNodes.Enqueue(startNode);

            while (openNodes.Count > 0)
            {
                FlowNode currentNode = openNodes.Dequeue();
                FlowNode lowestCostNeighbor = new FlowNode { totalCost = Mathf.Infinity };

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        int nx = (int)currentNode.gridPos.x + dx;
                        int ny = (int)currentNode.gridPos.y + dy;

                        if (nx >= 0 && nx < nodeGridWidth && ny >= 0 && ny < nodeGridHeight)
                        {
                            if (request.obstacleMap[nx, ny]) continue;

                            float moveCost = (dx != 0 && dy != 0) ? 1.41f : 1f; // Diagonal vs Straight
                            float newCost = currentNode.totalCost + moveCost;

                            if (newCost < totalNodes[nx, ny].totalCost)
                            {
                                totalNodes[nx, ny].totalCost = newCost;
                                openNodes.Enqueue(totalNodes[nx, ny]);
                            }
                        }
                    }
                }
            }
            for (int x = 0; x < nodeGridWidth; x++)
            {
                for (int y = 0; y < nodeGridHeight; y++)
                {
                    if (totalNodes[x, y].totalCost == 0) continue;

                    float bestCost = totalNodes[x, y].totalCost;
                    Vector2 bestDir = Vector2.zero;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < nodeGridWidth && ny >= 0 && ny < nodeGridHeight)
                            {
                                if (totalNodes[nx, ny].totalCost < bestCost)
                                {
                                    bestCost = totalNodes[nx, ny].totalCost;
                                    bestDir = new Vector2(dx, dy);
                                }
                            }
                        }
                    }
                    totalNodes[x, y].dir = bestDir.normalized;
                }
            }

            FlowFieldReturn fieldReturn = new FlowFieldReturn { field = totalNodes, dir = request.dir, nodeX = (int)request.nodePosInTotalMap.x, nodeY = (int)request.nodePosInTotalMap.y };
            return fieldReturn;
        }

        public static FlowFieldReturn FindFlowFieldToRect(FlowFieldRequestData request)
        {
            Queue<FlowNode> openNodes = new Queue<FlowNode>();
            int nodeGridWidth = Mathf.Max(1, Mathf.CeilToInt(request.searchWidth / request.nodeSize));
            int nodeGridHeight = Mathf.Max(1, Mathf.CeilToInt(request.searchHeight / request.nodeSize));
            FlowNode[,] totalNodes = new FlowNode[nodeGridWidth, nodeGridHeight];

            for (int x = 0; x < nodeGridWidth; x++)
            {
                for (int y = 0; y < nodeGridHeight; y++)
                {
                    totalNodes[x, y] = new FlowNode
                    {
                        position = new Vector2(request.mapOrigin.x + x * request.nodeSize, request.mapOrigin.y + y * request.nodeSize),
                        totalCost = float.MaxValue,
                        gridPos = new Vector2Int(x, y)
                    };
                }
            }

            if (request.dir == new Vector2(1, 0))
            {
                int x = nodeGridWidth - 1;
                for (int y = 0; y < nodeGridHeight; y++)
                {
                    totalNodes[x, y].totalCost = 0;
                    openNodes.Enqueue(totalNodes[x, y]);
                }
            }
            else if (request.dir == new Vector2(-1, 0))
            {
                int x = 0;
                for (int y = 0; y < nodeGridHeight; y++)
                {
                    totalNodes[x, y].totalCost = 0;
                    openNodes.Enqueue(totalNodes[x, y]);
                }
            }
            else if (request.dir == new Vector2(0, 1))
            {
                int y = nodeGridHeight - 1;
                for (int x = 0; x < nodeGridHeight; x++)
                {
                    totalNodes[x, y].totalCost = 0;
                    openNodes.Enqueue(totalNodes[x, y]);
                }
            }
            else if (request.dir == new Vector2(0, -1))
            {
                int y = 0;
                for (int x = 0; x < nodeGridHeight; x++)
                {
                    totalNodes[x, y].totalCost = 0;
                    openNodes.Enqueue(totalNodes[x, y]);
                }
            }
            else
            {
                Debug.LogError("No direction");
                return null;
            }
            //float bMinX = request.targetBounds.position.x - (request.targetBounds.size.x * 0.5f);
            //float bMaxX = request.targetBounds.position.x + (request.targetBounds.size.x * 0.5f);
            //float bMinY = request.targetBounds.position.y - (request.targetBounds.size.y * 0.5f);
            //float bMaxY = request.targetBounds.position.y + (request.targetBounds.size.y * 0.5f);

            //int minX = Mathf.FloorToInt((bMinX - request.mapOrigin.x) / request.nodeSize);
            //int maxX = Mathf.CeilToInt((bMaxX - request.mapOrigin.x) / request.nodeSize);
            //int minY = Mathf.FloorToInt((bMinY - request.mapOrigin.y) / request.nodeSize);
            //int maxY = Mathf.CeilToInt((bMaxY - request.mapOrigin.y) / request.nodeSize);

            //for (int x = Mathf.Max(0, minX); x < Mathf.Min(nodeGridWidth, maxX); x++)
            //{
            //    for (int y = Mathf.Max(0, minY); y < Mathf.Min(nodeGridHeight, maxY); y++)
            //    {
            //        totalNodes[x, y].totalCost = 0;
            //        openNodes.Enqueue(totalNodes[x, y]);
            //    }
            //}

            while (openNodes.Count > 0)
            {
                FlowNode currentNode = openNodes.Dequeue();
                FlowNode lowestCostNeighbor = new FlowNode { totalCost = Mathf.Infinity };

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        int nx = (int)currentNode.gridPos.x + dx;
                        int ny = (int)currentNode.gridPos.y + dy;

                        if (nx >= 0 && nx < nodeGridWidth && ny >= 0 && ny < nodeGridHeight)
                        {
                            if (request.obstacleMap[nx, ny]) continue;

                            float moveCost = (dx != 0 && dy != 0) ? 1.41f : 1f; // Diagonal vs Straight
                            float newCost = currentNode.totalCost + moveCost;

                            if (newCost < totalNodes[nx, ny].totalCost)
                            {
                                totalNodes[nx, ny].totalCost = newCost;
                                openNodes.Enqueue(totalNodes[nx, ny]);
                            }
                        }
                    }
                }
            }
            for (int x = 0; x < nodeGridWidth; x++)
            {
                for (int y = 0; y < nodeGridHeight; y++)
                {
                    if (totalNodes[x, y].totalCost == 0) continue;

                    float bestCost = totalNodes[x, y].totalCost;
                    Vector2 bestDir = Vector2.zero;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && nx < nodeGridWidth && ny >= 0 && ny < nodeGridHeight)
                            {
                                if (totalNodes[nx, ny].totalCost < bestCost)
                                {
                                    bestCost = totalNodes[nx, ny].totalCost;
                                    bestDir = new Vector2(dx, dy);
                                }
                            }
                        }
                    }
                    totalNodes[x, y].dir = bestDir.normalized;
                }
            }

            FlowFieldReturn fieldReturn = new FlowFieldReturn { field = totalNodes, dir = request.dir, nodeX = (int)request.nodePosInTotalMap.x, nodeY = (int)request.nodePosInTotalMap.y };
            return fieldReturn;
        }
    }
    public static class AStar
    {
        public class PathRequestData
        {
            public Vector2 targetPos;
            public Vector2 mapOrigin;
            public int searchWidth;
            public int searchHeight;
            public float nodeSize;
            public Vector2 AIPos;
            public bool[,] obstacleMap;
            public Task<List<Node>> pathFindingTask;
            public int AIIndex;
            public bool rightAngles;
        }
        public static List<Node> FindPathAStar(PathRequestData pathData)
        {
            // basic guards
            if (pathData.nodeSize <= 0) pathData.nodeSize = 1f;
            Node startNode;
            List<Node> OpenNodes = new List<Node>();
            Node[,] totalNodes;
            List<Node> path = new List<Node>();
            int halfWidth = pathData.searchWidth / 2;
            int halfHeight = pathData.searchHeight / 2;
            int nodeGridWidth = pathData.obstacleMap.GetLength(0);
            int nodeGridHeight = pathData.obstacleMap.GetLength(1);
            Vector2 gridWorldOrigin = pathData.mapOrigin;
            totalNodes = new Node[nodeGridWidth, nodeGridHeight];
            //Debug.Log($"Node width: {nodeGridWidth}, height: {halfHeight}. Pathdata width: {pathData.searchWidth}, height: {pathData.searchHeight}");
            // ensure start is in bounds (recalculate origin if needed)
            if (pathData.AIPos.x < gridWorldOrigin.x || pathData.AIPos.x > gridWorldOrigin.x + pathData.searchWidth ||
                pathData.AIPos.y < gridWorldOrigin.y || pathData.AIPos.y > gridWorldOrigin.y + pathData.searchHeight)
            {
                //Debug.LogError($"AI {pathData.AIIndex}'s start position {pathData.AIPos} is outside search area origin {gridWorldOrigin} size ({pathData.searchWidth},{pathData.searchHeight}). Expand search area or recenter.");
                return null;
            }
            for (int x = 0; x < nodeGridWidth; x++)
            {
                for (int y = 0; y < nodeGridHeight; y++)
                {
                    Node node = new Node
                    {
                        position = new Vector2(gridWorldOrigin.x + x * pathData.nodeSize, gridWorldOrigin.y + y * pathData.nodeSize),
                        startCost = Mathf.Infinity,
                        targetDistance = Mathf.Infinity,
                        totalCost = Mathf.Infinity,
                        parent = null,
                        evaluated = false
                    };

                    totalNodes[x, y] = node;
                }
            }
            int startX = Mathf.Clamp(Mathf.RoundToInt((pathData.AIPos.x - gridWorldOrigin.x) / pathData.nodeSize), 0, nodeGridWidth - 1);
            int startY = Mathf.Clamp(Mathf.RoundToInt((pathData.AIPos.y - gridWorldOrigin.y) / pathData.nodeSize), 0, nodeGridHeight - 1);
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
                if (Vector2.Distance(currentNode.position, pathData.targetPos) < pathData.nodeSize * 0.9f)
                {
                    targetNode = currentNode;
                    break;
                }

                int currentNodeGridX = Mathf.Clamp(Mathf.RoundToInt((currentNode.position.x - gridWorldOrigin.x) / pathData.nodeSize), 0, nodeGridWidth - 1);
                int currentNodeGridY = Mathf.Clamp(Mathf.RoundToInt((currentNode.position.y - gridWorldOrigin.y) / pathData.nodeSize), 0, nodeGridHeight - 1);

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
                        if (pathData.rightAngles)
                        {
                            if (dx == -1 && dy == -1)
                            {
                                continue;
                            }
                            else if (dx == -1 && dy == 1)
                            {
                                continue;
                            }
                            else if (dx == 1 && dy == 1)
                            {
                                continue;
                            }
                            else if (dx == 1 && dy == -1)
                            {
                                continue;
                            }
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
                            currentNode.child = neighboringNode;
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
                Debug.LogWarning("No path found to the target. Possible causes: search area too small, obstacles block the route, or nodeSize is too large.");
                return null;
            }

            Node pathNode = targetNode;
            while (pathNode != null)
            {
                path.Add(pathNode);
                pathNode = pathNode.parent;
            }
            path.Reverse();

            for (int j = 0; j < path.Count - 1; j++)
            {
                path[j].dir = (path[j + 1].position - path[j].position).normalized;
            }
            path[path.Count - 1].dir = Vector2.zero;
            return path;
        }
    }

    private Vector2 WorldToSectorIndex(Vector2 worldPos)
    {
        float rx = worldPos.x - mapBounds.min.x;
        float ry = worldPos.y - mapBounds.min.y;
        int sx = Mathf.FloorToInt(rx / flowFieldSize);
        int sy = Mathf.FloorToInt(ry / flowFieldSize);
        return new Vector2(sx, sy);
    }

    private Vector2 WorldToLocalFieldIndex(Vector2 worldPos, Vector2 sectorOrigin)
    {
        float localX = worldPos.x - sectorOrigin.x;
        float localY = worldPos.y - sectorOrigin.y;
        return new Vector2(Mathf.FloorToInt(localX), Mathf.FloorToInt(localY));
    }
}
