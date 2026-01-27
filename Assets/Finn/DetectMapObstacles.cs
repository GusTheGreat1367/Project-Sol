using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;



public class DetectMapObstacles : MonoBehaviour
{

    public GameObject randomObj;
    private Obstacle randomObst = new Obstacle();
    public ObstacleManager obstacleManager;
    public List<ObstacleMapRequest> mapRequests = new List<ObstacleMapRequest>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        obstacleManager = FindFirstObjectByType(typeof(ObstacleManager)).GetComponent<ObstacleManager>();
        randomObst = DetectObstaclesInPosition.SetupObstacle(randomObj);
        List<Obstacle> obstacles = obstacleManager.GetObstaclesInScene();
        for (int i = 0; i < 100; i++)
        {
            ObstacleMapRequest rq = new ObstacleMapRequest
            {
                obstacleMapReturn = Task.Run(() => DetectObstaclesInPosition.DetectObstacleMap(new Float2(50, 50), obstacles, randomObst, 1)),
                timeStarted = Time.time
            };
            mapRequests.Add(rq);
        }
    }

    // Update is called once per frame
    void Update()
    {
        List<Obstacle> obstacles = obstacleManager.GetObstaclesInScene();
        for (int i = 0; i < mapRequests.Count; i++)
        {

            if (mapRequests[i].obstacleMapReturn.IsCompletedSuccessfully)
            {
                mapRequests[i].obstacleMapReturn = null;
                mapRequests[i].timeCompleted = Time.time;
                Debug.Log("Task completed successfully in " + (mapRequests[i].timeCompleted - mapRequests[i].timeStarted) + " seconds");
                ObstacleMapRequest rq = new ObstacleMapRequest
                {
                    obstacleMapReturn = Task.Run(() => DetectObstaclesInPosition.DetectObstacleMap(new Float2(50, 50), obstacles, randomObst, 1))
                };
                mapRequests[i] = rq;
                mapRequests[i].timeStarted = Time.time;
            }

        }
    }

    public List<Obstacle> ObstacleReturn()
    {
        List<Obstacle> obstacles = new List<Obstacle>();
        Collider2D[] obstaclesWithColliders = (Collider2D[])FindObjectsByType(typeof(Collider2D), FindObjectsSortMode.None);
        for (int i = 0; i < obstaclesWithColliders.Length; i++)
        {
            Obstacle obstacle = DetectObstaclesInPosition.SetupObstacle(obstaclesWithColliders[i].gameObject);
            obstacles.Add(obstacle);
        }
        return obstacles;
    }
}

