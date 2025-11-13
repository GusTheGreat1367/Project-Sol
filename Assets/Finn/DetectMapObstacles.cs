using System.Collections.Generic;
using UnityEngine;
public class Obstacle
{
    public Vector2 position;
    public Vector2 size;
    public string name;
    public int instanceID;
}

public class DetectMapObstacles : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 100; i++)
        {
            List<Obstacle> obstacles = ObstacleReturn();
        }
    }

    public List<Obstacle> ObstacleReturn()
    {
        List<Obstacle> obstacles = new List<Obstacle>();
        Collider2D[] obstaclesWithColliders = (Collider2D[])FindObjectsByType(typeof(Collider2D), FindObjectsSortMode.None);
        for (int i = 0; i < obstaclesWithColliders.Length; i++)
        {
            Obstacle obstacle = new Obstacle();
            obstacle.position = obstaclesWithColliders[i].bounds.center;
            obstacle.size = obstaclesWithColliders[i].bounds.size;
            obstacle.name = obstaclesWithColliders[i].name;
            obstacle.instanceID = obstaclesWithColliders[i].gameObject.GetInstanceID();
            obstacles.Add(obstacle);
        }
        return obstacles;
    }
}

public static class DetectObstaclesInPosition
{
    //public static bool[,] DetectObstacleMap(Vector2 position, Vector2 size, List<Obstacle> obstacles)
    //{
    //    bool[,] obstacleMapReturn = new bool[(int)size.x, (int)size.y];
    //    for (int x = 0; x < size.x; x++)
    //    {
    //        for (int y = 0; y < size.y; y++)
    //        {
    //            if (DetectInPositionExclusive(new Vector2(x, y))
    //        }
    //    }
    //}
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
    public static bool DetectInPositionExclusive(Vector2 position, Vector2 size, int ExcludeinstanceID, List<Obstacle> obstacles)
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
    public static bool Contains(Vector2 position1, Vector2 size1, Vector2 position2, Vector2 size2)
    {
        Vector2 hA = new Vector2(size1.x / 2, size1.y / 2);
        Vector2 hB = new Vector2(size2.x / 2, size2.y / 2);
        Vector2 s1A = new Vector2(position1.x - hA.x, position1.y + hA.y);
        Vector2 s2A = new Vector2(position1.x + hA.x, position1.y + hA.y);
        Vector2 s3A = new Vector2(position1.x + hA.x, position1.y - hA.y);
        Vector2 s4A = new Vector2(position1.x - hA.x, position1.y - hA.y);
        Vector2 s1B = new Vector2(position2.x - hB.x, position2.y + hB.y);
        Vector2 s2B = new Vector2(position2.x + hB.x, position2.y + hB.y);
        Vector2 s3B = new Vector2(position2.x + hB.x, position2.y - hB.y);
        Vector2 s4B = new Vector2(position2.x - hB.x, position2.y - hB.y);

        if (s1A.x < s2B.x && s2A.x > s1B.x && s4A.y < s2B.y && s2A.y > s4B.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static ContainsPointReturn ContainsPoint(Vector2 position1, Vector2 size1, Vector2 position2)
    {
        Vector2 hA = new Vector2(size1.x / 2, size1.y / 2);
        Vector2 s1A = new Vector2(position1.x - hA.x, position1.y + hA.y);
        Vector2 s2A = new Vector2(position1.x + hA.x, position1.y + hA.y);
        Vector2 s3A = new Vector2(position1.x + hA.x, position1.y - hA.y);
        Vector2 s4A = new Vector2(position1.x - hA.x, position1.y - hA.y);

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
