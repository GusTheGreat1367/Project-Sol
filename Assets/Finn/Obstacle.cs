using Unity.VisualScripting;
using UnityEngine;

public class ObstacleObj : MonoBehaviour
{

    public Obstacle objObstacle = new Obstacle();
    ObstacleManager obstacleManager;
    public void Start()
    {
        objObstacle = DetectObstaclesInPosition.SetupObstacle(gameObject);
        if (objObstacle == null)
        {
            Debug.LogWarning($"GameObject {gameObject.name} does not have a collider, so there is no reason for it to be an obstacle. Deleting ObstacleObj script from GameObject {gameObject.name}");
            Destroy(this);
        }
        try
        {
            obstacleManager = FindFirstObjectByType(typeof(ObstacleManager)).GetComponent<ObstacleManager>();
        }
        catch
        {
            Debug.LogWarning("No obstacleManager detected in scene, please add one!");
            Destroy(this);
        }
        obstacleManager.obstaclesInScene.Add(this);
    }
    public void LateUpdate()
    {
        if (Vector2.Distance(new Vector2(objObstacle.position.x, objObstacle.position.y), transform.position) > 0.1f)
        {
            objObstacle.position = new Float2(transform.position.x, transform.position.y);
        }
        if (new Vector2(objObstacle.size.x, objObstacle.size.y) != new Vector2(GetComponent<Collider2D>().bounds.size.x, GetComponent<Collider2D>().bounds.size.y))
        {
            objObstacle.size = new Float2(GetComponent<Collider2D>().bounds.size.x, GetComponent<Collider2D>().bounds.size.y);
        }
    }
    public void OnDestroy()
    {
        obstacleManager.obstaclesInScene.Remove(this);
    }
}
