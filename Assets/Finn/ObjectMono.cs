using Unity.VisualScripting;
using UnityEngine;

public class ObjectMono : MonoBehaviour
{
    public CustomObject obj = new CustomObject();
    ObstacleManager obstacleManager;
    public void Start()
    {
        obj = DetectObstaclesInPosition.SetupObject(gameObject);
        if (obj == null)
        {
            Debug.LogWarning($"Something went wrong with the setup process with object {obj.name}, please look into it.");
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
        obstacleManager.objectsInScene.Add(this);
    }
    public void LateUpdate()
    {
        if (Vector2.Distance(new Vector2(obj.position.x, obj.position.y), transform.position) > 0.1f)
        {
            obj.position = new Float2(transform.position.x, transform.position.y);
        }
        if (new Vector2(obj.size.x, obj.size.y) != new Vector2(GetComponent<Collider2D>().bounds.size.x, GetComponent<Collider2D>().bounds.size.y))
        {
            obj.size = new Float2(GetComponent<Collider2D>().bounds.size.x, GetComponent<Collider2D>().bounds.size.y);
        }
    }
    public void OnDestroy()
    {
        obstacleManager.objectsInScene.Remove(this);
    }
}
