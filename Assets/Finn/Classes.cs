using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static DetectObstaclesInPosition;

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
public class Node
{
    public Vector2 position;
    public float startCost;
    public float targetDistance;
    public float totalCost;
    public bool evaluated;
    public Node parent;
    public Node child;
    public Vector2 dir;
}
public enum PathfindingStatus
{
    Requested,
    CalculatingFlowField,
    FinishedFlowField,
    Faulted,
    NotRequested
}
public class PathFinderAI
{
    public PathfindingStatus pathStatus;
    public GameObject obj;
    public bool needsPath;
    public float velocity;
    public Vector2 currentSector;
    public Vector2 targetSector;
    public bool selected;
    public float maxVelocity = 5f;
    public bool targetSet = false;
    public Vector2 targetPos;
    public int loops;
    public bool currentlyInAITask;
    public bool pathFinding;
    public AIPathFindingData pathData;
    public int instanceID;
    public Task<bool[,]> obstacleReturn;
    public Task<Path> pathfindingTask;
    public Path finishedPathfindingTask;
    public Task<List<Node>> aStarPathfindingTask;
    public List<Node> finishedAStarPathfindingTask;
    public bool usingAStarPath;
    public CustomObject objectReference;
    public void PathFind(Vector2 destination)
    {

        targetPos = destination;
        targetSet = true;
    }
}
public class PathFindTSInput
{
    //Used for inputting thread-safe variables into the background "pathfind()" function
    public Float2 AIPos;
    public Float2 TargetPos;
    public List<Obstacle> obstaclesInScene;
    public List<MapTarget> mapTargetsInScene;
    public List<CustomObject> objectsInScene;
    public List<FlowFieldReturn>[,] flowFieldReturns;
    public int flowFieldSize;
    public Float2Bounds mapbounds;
    public CompleteObstacleMapReturn lowQualityMapReturn;
    public CompleteObstacleMapReturn[,] obstacleMapReturns;
    public float nodeSize;
    public List<FlowFieldReturn> destinationFlowFields;

}
public class FlowFieldReturn
{
    public int nodeX;
    public int nodeY;
    public FlowNode[,] field;
    public Vector2 dir;
    public Vector2 pos;
}
public class FlowNode
{
    public Vector2 position;
    public Vector2 gridPos;
    public float totalCost;
    public Vector2 dir;
}
public class Path
{
    public PathFinderAI AI;
    public List<FlowFieldReturn> sectorPath;
    public List<Float2> sectorVectorPath;

}
public class AIPathFindingData
{
    public Vector2 mapOrigin;
    public int mapWidth;
    public int mapHeight;
    public float pathResolution;
}
public class ObstacleMapReturn
{
    public bool[,] obstacleMap;
    public PathFinderAI pathfinderAI;
}
public struct Float2
{
    public float x;
    public float y;
    public Float2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
    public static implicit operator float2(Float2 fr)
    {
        return new float2(fr.x, fr.y);
    }
    public static implicit operator Vector2(Float2 fr)
    {
        return new Vector2(fr.x, fr.y);
    }
    public static Vector2 ConvertToV2(Float2 fr)
    {
        return new Vector2(fr.x, fr.y);
    }
    public static Float2 Lerp(Float2 a, Float2 b, float t)
    {
        t = Mathf.Clamp01(t);
        return new Float2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator +(Float2 a, Float2 b)
    {
        return new Float2(a.x + b.x, a.y + b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator -(Float2 a, Float2 b)
    {
        return new Float2(a.x - b.x, a.y - b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator *(Float2 a, Float2 b)
    {
        return new Float2(a.x * b.x, a.y * b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator /(Float2 a, Float2 b)
    {
        return new Float2(a.x / b.x, a.y / b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator -(Float2 a)
    {
        return new Float2(0f - a.x, 0f - a.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator *(Float2 a, float d)
    {
        return new Float2(a.x * d, a.y * d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator *(float d, Float2 a)
    {
        return new Float2(a.x * d, a.y * d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator /(Float2 a, float d)
    {
        return new Float2(a.x / d, a.y / d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Float2 lhs, Float2 rhs)
    {
        float num = lhs.x - rhs.x;
        float num2 = lhs.y - rhs.y;
        return num * num + num2 * num2 < 9.99999944E-11f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Float2 lhs, Float2 rhs)
    {
        return !(lhs == rhs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Float2(Vector3 v)
    {
        return new Float2(v.x, v.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3(Float2 v)
    {
        return new Vector3(v.x, v.y, 0f);
    }
    public static implicit operator Float2(Vector2 fr)
    {
        return new Float2(fr.x, fr.y);
    }
    public static float Distance(Float2 a, Float2 b)
    {
        float num = a.x - b.x;
        float num2 = a.y - b.y;
        return (float)Math.Sqrt(num * num + num2 * num2);
    }
    public static Float2 Min(Float2 lhs, Float2 rhs)
    {
        return new Float2(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));
    }
    public static Float2 Max(Float2 lhs, Float2 rhs)
    {
        return new Float2(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));
    }

    public override bool Equals(object obj)
    {
        return obj is Float2 @float &&
               x == @float.x &&
               y == @float.y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y);
    }
}

public struct Float2Bounds

{
    public Float2 position;
    public Float2 size;
    public Float2Bounds(Float2 position, Float2 size)
    {
        this.position = position;
        this.size = size;
    }
    public void SetMinMax(Float2 min, Float2 max)
    {
        size = (max - min) * 0.5f;
        position = min + size;
    }
    public Float2 min
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return position - size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            SetMinMax(value, max);
        }
    }
    public Float2 max
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return position + size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            SetMinMax(value, max);
        }
    }
    public void Encapsulate(Float2 point)
    {
        SetMinMax(Float2.Min(min, point), Float2.Max(max, point));
    }

    public void Encapsulate(Float2Bounds bounds)
    {
        Encapsulate(bounds.position - bounds.size);
        Encapsulate(bounds.position + bounds.size);
    }
}
public class CustomObject
{
    public Float2 position;
    public Float2 size;
    public float instanceID;
    public string name;
}
public static class DetectObstaclesInPosition
{
    public static void DrawRectangle(Vector3 position, Vector3 extent, Color color)
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
    public static Obstacle SetupObstacle(GameObject obj)
    {
        if (obj.GetComponent<Collider2D>() != null)
        {
            Obstacle returnObstacle = new Obstacle
            {
                position = new Float2(obj.transform.position.x, obj.transform.position.y),
                size = new Float2(obj.GetComponent<Collider2D>().bounds.size.x, obj.GetComponent<Collider2D>().bounds.size.y),
                name = obj.name.ToString(),
                instanceID = obj.GetInstanceID()
            };
            //Debug.Log("Set up obstacle with size " + returnObstacle.size.x + ", " + returnObstacle.size.y);
            return returnObstacle;
        }
        else
        {
            Debug.LogError("ERR: OBJ " + obj.name + " DOES NOT INCLUDE A COLLIDER, PLEASE ADD ONE!");
            return null;
        }
    }
    public static bool IsInteger(float value)
    {
        return value == Math.Truncate(value);
    }
    public static CustomObject SetupObject(GameObject obj)
    {
        if (obj.GetComponent<Collider2D>() != null)
        {
            CustomObject returnObstacle = new CustomObject
            {
                position = new Float2(obj.transform.position.x, obj.transform.position.y),
                size = new Float2(obj.GetComponent<Collider2D>().bounds.size.x, obj.GetComponent<Collider2D>().bounds.size.y),
                name = obj.name.ToString(),
                instanceID = obj.GetInstanceID()
            };
            //Debug.Log("Set up obstacle with size " + returnObstacle.size.x + ", " + returnObstacle.size.y);
            return returnObstacle;
        }
        else
        {
            CustomObject returnObstacle = new CustomObject
            {
                position = new Float2(obj.transform.position.x, obj.transform.position.y),
                size = new Float2(5, 5),
                name = obj.name.ToString(),
                instanceID = obj.GetInstanceID()
            };
            //Debug.Log("Set up obstacle with size " + returnObstacle.size.x + ", " + returnObstacle.size.y);
            return returnObstacle;
        }
    }
    public static List<CustomObject> SetupObjects(List<GameObject> obj)
    {
        List<CustomObject> returnList = new List<CustomObject>();
        for (int i = 0; i < obj.Count; i++)
        {
            if (obj[i].GetComponent<Collider2D>() != null)
            {
                CustomObject returnObstacle = new CustomObject
                {
                    position = new Float2(obj[i].transform.position.x, obj[i].transform.position.y),
                    size = new Float2(obj[i].GetComponent<Collider2D>().bounds.size.x, obj[i].GetComponent<Collider2D>().bounds.size.y),
                    name = obj[i].name.ToString(),
                    instanceID = obj[i].GetInstanceID()
                };
                //Debug.Log("Set up obstacle with size " + returnObstacle.size.x + ", " + returnObstacle.size.y);
                returnList.Add(returnObstacle);
            }
            else
            {
                CustomObject returnObstacle = new CustomObject
                {
                    position = new Float2(obj[i].transform.position.x, obj[i].transform.position.y),
                    size = new Float2(5, 5),
                    name = obj[i].name.ToString(),
                    instanceID = obj[i].GetInstanceID()
                };
                //Debug.Log("Set up obstacle with size " + returnObstacle.size.x + ", " + returnObstacle.size.y);
                returnList.Add(returnObstacle);
            }
        }
        return returnList;

    }
    public static MapTarget SetupMapTarget(GameObject obj)
    {
        MapTarget returnTarget = new MapTarget
        {
            position = new Float2(obj.transform.position.x, obj.transform.position.y),
            name = obj.name.ToString(),
            instanceID = obj.GetInstanceID()
        };
        return returnTarget;
    }

    public static List<MapTarget> SetupMapTargets(List<GameObject> objs)
    {
        List<MapTarget> targets = new List<MapTarget>();
        for (int i = 0; i < objs.Count; i++)
        {
            MapTarget returnTarget = new MapTarget
            {
                position = new Float2(objs[i].transform.position.x, objs[i].transform.position.y),
                name = objs[i].name.ToString(),
                instanceID = objs[i].GetInstanceID()
            };
            targets.Add(returnTarget);
        }
        return targets;
    }

    public static CompleteObstacleMapReturn CompleteObstacleMap(List<Obstacle> obstacles, float nodeSize, List<MapTarget> mapTargets, List<CustomObject> others)
    {
        Float2Bounds boundingBox = new Float2Bounds();
        if (obstacles.Count > 0)
        {
            boundingBox = new Float2Bounds(obstacles[0].position, obstacles[0].size * 0.5f);

        }
        else if (mapTargets.Count > 0)
        {
            boundingBox = new Float2Bounds(mapTargets[0].position, new Float2(5, 5));
        }
        else if (others.Count > 0)
        {
            boundingBox = new Float2Bounds(others[0].position, others[0].size * 0.5f);
        }
        else
        {
            Debug.LogError("No objects found in any lists, maybe this was an accidental call?");
            return null;
        }
        if (obstacles.Count > 0)
        {

        }

        for (int i = 0; i < mapTargets.Count; i++)
        {
            boundingBox.Encapsulate(new Float2Bounds(mapTargets[i].position, new Float2(5, 5)));
        }
        for (int i = 0; i < others.Count; i++)
        {
            boundingBox.Encapsulate(new Float2Bounds(others[i].position, others[i].size));
        }
        Float2 leftBCorner = new Float2(boundingBox.position.x - (boundingBox.size.x), boundingBox.position.y - (boundingBox.size.y));
        Float2 rightBCorner = new Float2(boundingBox.position.x + (boundingBox.size.x), boundingBox.position.y - (boundingBox.size.y));
        Float2 leftTCorner = new Float2(boundingBox.position.x - (boundingBox.size.x), boundingBox.position.y + (boundingBox.size.y));
        Float2 rightTCorner = new Float2(boundingBox.position.x + (boundingBox.size.x), boundingBox.position.y + (boundingBox.size.y));
        Float2 center = boundingBox.position;
        Float2 size = boundingBox.size;
        //Debug.Log($"Bounding box size: {size.x}, {size.y}, Origin is {leftBCorner.x}, {leftBCorner.y}");
        int nodeGridWidth = Mathf.Max(1, Mathf.CeilToInt((size.x * 2) / nodeSize));
        int nodeGridHeight = Mathf.Max(1, Mathf.CeilToInt((size.y * 2) / nodeSize));
        CompleteObstacleMapReturn objMap = new CompleteObstacleMapReturn
        {
            obstacleMap = new List<Obstacle>[nodeGridWidth, nodeGridHeight],
            obstacleMapBool = new bool[nodeGridWidth, nodeGridHeight],
            size = new Float2(nodeGridWidth, nodeGridHeight),
            startArea = leftBCorner,
            bounds = boundingBox
        };
        for (int x = 0; x < nodeGridWidth; x++)
        {
            for (int y = 0; y < nodeGridHeight; y++)
            {
                objMap.obstacleMap[x, y] = new List<Obstacle>();
                objMap.obstacleMapBool[x, y] = false;
            }
        }
        for (int i = 0; i < obstacles.Count; i++)
        {
            Float2 obsMin = obstacles[i].position - (obstacles[i].size / 2f);
            Float2 obsMax = obstacles[i].position + (obstacles[i].size / 2f);

            int startX = Mathf.FloorToInt((obsMin.x - objMap.startArea.x) / nodeSize);
            int startY = Mathf.FloorToInt((obsMin.y - objMap.startArea.y) / nodeSize);

            int endX = Mathf.Min(nodeGridWidth, Mathf.CeilToInt((obsMax.x - objMap.startArea.x) / nodeSize));
            int endY = Mathf.Min(nodeGridHeight, Mathf.CeilToInt((obsMax.y - objMap.startArea.y) / nodeSize));


            startX = Mathf.Max(0, startX);
            startY = Mathf.Max(0, startY);

            endX = Mathf.Min(nodeGridWidth, endX);
            endY = Mathf.Min(nodeGridHeight, endY);
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    objMap.obstacleMap[x, y].Add(obstacles[i]);
                    objMap.obstacleMapBool[x, y] = true;
                    //Debug.Log($"Obstacle detected at {x}, {y}");
                }
            }
        }
        return objMap;
    }

    //Deprecated
    public static bool[,] DetectObstacleMap(Float2 size, List<Obstacle> obstacles, Obstacle exclude, float pathResolution)
    {
        int halfWidth = Mathf.RoundToInt(size.x / 2);
        int halfHeight = Mathf.RoundToInt(size.y / 2);
        int nodeGridWidth = Mathf.Max(1, Mathf.RoundToInt(size.x / pathResolution));
        int nodeGridHeight = Mathf.Max(1, Mathf.RoundToInt(size.y / pathResolution));
        bool[,] obstacleMapReturn = new bool[(int)size.x, (int)size.y];
        Float2 nodePosOrigin = new Float2(exclude.position.x - halfWidth, exclude.position.y - halfHeight);
        for (int x = 0; x < nodeGridWidth; x++)
        {
            for (int y = 0; y < nodeGridHeight; y++)
            {
                Float2 nodePos = new Float2(
                    nodePosOrigin.x + x * pathResolution,
                    nodePosOrigin.y + y * pathResolution
                );
                if (DetectInPositionExclusive(new Float2(nodePos.x, nodePos.y), exclude.size, exclude.instanceID, obstacles))
                {
                    obstacleMapReturn[x, y] = true;
                }
                else
                {
                    obstacleMapReturn[x, y] = false;
                }
            }
        }
        return obstacleMapReturn;
    }
    //Deprecated
    public static bool DetectInPositionExclusive(Float2 position, Float2 size, int ExcludeinstanceID, List<Obstacle> obstacles)
    {
        for (int i = 0; i < obstacles.Count; i++)
        {
            if (obstacles[i].instanceID == ExcludeinstanceID)
            {
                continue;
            }
            else
            {
                if (Contains(position, size, obstacles[i].position, obstacles[i].size))
                {
                    return true;
                }
            }
        }
        return false;
    }
    public static bool Contains(Float2 position1, Float2 size1, Float2 position2, Float2 size2)
    {
        Float2 hA = new Float2(size1.x / 2, size1.y / 2);
        Float2 hB = new Float2(size2.x / 2, size2.y / 2);
        Float2 s1A = new Float2(position1.x - hA.x, position1.y + hA.y);
        Float2 s2A = new Float2(position1.x + hA.x, position1.y + hA.y);
        Float2 s3A = new Float2(position1.x + hA.x, position1.y - hA.y);
        Float2 s4A = new Float2(position1.x - hA.x, position1.y - hA.y);
        Float2 s1B = new Float2(position2.x - hB.x, position2.y + hB.y);
        Float2 s2B = new Float2(position2.x + hB.x, position2.y + hB.y);
        Float2 s3B = new Float2(position2.x + hB.x, position2.y - hB.y);
        Float2 s4B = new Float2(position2.x - hB.x, position2.y - hB.y);

        if (s1A.x < s2B.x && s2A.x > s1B.x && s4A.y < s2B.y && s2A.y > s4B.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static ContainsPointReturn ContainsPoint(Float2 position1, Float2 size1, Float2 position2)
    {
        Float2 hA = new Float2(size1.x / 2, size1.y / 2);
        Float2 s1A = new Float2(position1.x - hA.x, position1.y + hA.y);
        Float2 s2A = new Float2(position1.x + hA.x, position1.y + hA.y);
        Float2 s3A = new Float2(position1.x + hA.x, position1.y - hA.y);
        Float2 s4A = new Float2(position1.x - hA.x, position1.y - hA.y);

        if (s1A.x < position2.x && s2A.x > position2.x && s4A.y < position2.y && s2A.y > position2.y)
        {
            return new ContainsPointReturn();
        }
        else
        {
            ContainsPointReturn returnVal = new ContainsPointReturn();
            if (s1A.x > position2.x)
            {
                returnVal.s1 = true;
                returnVal.s3 = false;
            }
            else if (s2A.x < position2.x)
            {
                returnVal.s3 = true;
                returnVal.s1 = false;
            }
            else
            {
                returnVal.s3 = false;
                returnVal.s1 = false;
            }

            if (s4A.y > position2.y)
            {
                returnVal.s4 = true;
                returnVal.s2 = false;
            }
            else if (s2A.y < position2.y)
            {
                returnVal.s2 = true;
                returnVal.s4 = false;
            }
            else
            {
                returnVal.s2 = false;
                returnVal.s4 = false;
            }
            returnVal.any = false;
            return returnVal;
        }

    }
}
public class ObstacleMapRequest
{
    public Task<bool[,]> obstacleMapReturn;
    public float timeStarted;
    public float timeCompleted;
}
public class Obstacle
{
    public Float2 position;
    public Float2 size;
    public string name = null;
    public int instanceID = 0;
}
public class MapTarget
{
    public Float2 position;
    public string name;
    public int instanceID;
}
public class CompleteObstacleMapReturn
{
    public List<Obstacle>[,] obstacleMap;
    public bool[,] obstacleMapBool;
    public Float2 startArea;
    public Float2 size;
    public Float2Bounds bounds;
}
public class ContainsPointReturn
{
    //Left
    public bool s1 = true;
    //Top
    public bool s2 = true;
    //Right
    public bool s3 = true;
    //Bottom
    public bool s4 = true;

    public bool any = true;
}
