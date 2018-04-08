using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	public float speed = 5f;	
	public bool diagonalMovement = true;
	public GameObject bulletObject;
	public int maxHealth = 100;
	public float shootRange = 10;
	public float range = 15;

	[HideInInspector]
	public Transform target;
	[HideInInspector]
	public ProceduralMap map;
	[HideInInspector]
	public int health;
	[HideInInspector]
	public int id;

	List<Node> path;
	bool canShoot = true;
	bool isRanged = false;

	void Start()
	{
		health = maxHealth;
	}

	void FixedUpdate()
	{
		if(target.GetComponent<Player>().isDead)
			return;

		if(map.enemyAgentBehavior == ProceduralMap.EnemyAgentBehavior.Ranged)
			isRanged = true;
		else
			isRanged = false;
		
		float distance = Vector3.Distance(transform.position, target.position);

		if((isRanged && distance < range) || (!isRanged))
		{
			FindPath(transform.position, target.position);		
			if(path == null)
				return;
			
			foreach(Node n in path)
				transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
			
			if(distance < shootRange && canShoot)
				Shoot();
		}
	}

	void OnGUI()
	{
		GUIStyle style =  new GUIStyle();
		style.fontSize = 15;
		style.normal.textColor = Color.white;
		GUILayout.Space(30 * id);
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		GUILayout.Label("Enemy" + id, style, GUILayout.Width(70));
		GUILayout.HorizontalScrollbar(0, health, 0, maxHealth, GUILayout.Width(200));
		GUILayout.Label((canShoot) ? "Shooting" : "Patroling", style, GUILayout.Width(70));
		GUILayout.EndHorizontal();
	}

	void FindPath(Vector3 startPos, Vector3 targetPos)
	{
		Node startNode = map.NodeFromWorldPoint(startPos);
		Node targetNode = map.NodeFromWorldPoint(targetPos);

		List<Node> openSet = new List<Node>();
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);

		while(openSet.Count > 0)
		{
			Node currentNode = openSet[0];
			for (int i=1; i < openSet.Count; i++)
			{
				if (openSet[i].fcost < currentNode.fcost || openSet[i].fcost == currentNode.fcost)
				{
					if(openSet[i].hCost < currentNode.hCost)
						currentNode = openSet[i];
				}
			}

			openSet.Remove(currentNode);
			closedSet.Add(currentNode);

			if(currentNode == targetNode)
			{
				RetracePath(startNode, targetNode);
				return;
			}
			
			foreach (Node neighbour in map.GetNeighbours(currentNode, diagonalMovement))
			{
				if(!neighbour.walkable || closedSet.Contains(neighbour))
					continue;
				
				int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if(newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					neighbour.gCost = newCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;

					if(!openSet.Contains(neighbour))
						openSet.Add(neighbour);
				}
			}
		}
	}

	void RetracePath(Node startNode, Node endNode)
	{
		path = new List<Node>();
		Node currentNode = endNode;

		while(currentNode != startNode)
		{
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Reverse();

		map.debugPath = path;
	}

	int GetDistance(Node nodeA, Node nodeB)
	{
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}

	void Shoot()
	{
		canShoot = false;
		transform.LookAt(target);
		GameObject bullet = GameObject.Instantiate(bulletObject, transform.position, Quaternion.identity);
		Vector3 curScale = bullet.transform.localScale;
		Vector3 thisScale = transform.localScale;
		bullet.GetComponent<Bullet>().playerFwd = transform.forward;
		bullet.transform.localScale = new Vector3(curScale.x * thisScale.x, curScale.y * thisScale.y, curScale.z * thisScale.z);
		Invoke("CanShootNow", Random.Range(0.6f, 1.0f));
	}

	void CanShootNow()
	{
		canShoot = true;
	}

	public void Damage(int val)
	{
		health-=val;
		if(health <= 0)
			Destroy(gameObject);
	}
}
