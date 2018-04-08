using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProceduralMap : MonoBehaviour
{
	public int width = 16;
	public int height = 16;
	public float scale = 1;
	public Texture2D noise;
	public bool GenerateRandomNoise = true;
	public Transform bondFinal;
	public GameObject wallObject;
	public GameObject playerObject;
	public GameObject[] enemyObjects;
	public int enemyCount = 1;
	public LayerMask obstacleLayer;
	public EnemyAgentBehavior enemyAgentBehavior = EnemyAgentBehavior.Normal;
	public enum EnemyAgentBehavior { Normal, Ranged };

	[HideInInspector]
	public List<Node> debugPath;

	Player playerInstance;
	int enemiesSpawned = 0;
	float brickWidth;
	float brickHeight;
	Vector3 brickScale;
	Node[,] grid;

	void Start()
	{
		if(GenerateRandomNoise)
			noise = GenerateTexture();
		
		brickWidth = width * scale;
		brickHeight = height * scale;
		brickScale = new Vector3(brickWidth, Mathf.Max(brickWidth, brickHeight) , brickHeight);
		
		SpawnBlocks();
	}

	void OnGUI()
	{
		GUI.Label(new Rect(0, Screen.height - 20, Screen.width, 20), "Press R to reload the scene");
		if(Input.GetKeyDown(KeyCode.R))
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	Texture2D GenerateTexture()
	{		
		Texture2D tex = new Texture2D(width, height);
		for(int x = 0; x < width; x++)
		{
			for(int y = 0; y < height; y++)
			{
				int rand = Random.Range(0, 10);
				Color color = ( rand == 0 ) ? Color.black : Color.white;
				tex.SetPixel(x, y, color);
			}
		}
		tex.Apply();
		return tex;
	}

	void SpawnBlocks()
	{
		grid = new Node[width, height];

		transform.localScale = new Vector3(width * brickWidth, height * brickHeight , 1);
		transform.position = new Vector3( ( width * brickWidth - brickWidth ) / 2, 0, ( height * brickHeight - brickHeight ) / 2);
		bondFinal.position = new Vector3( width * brickWidth - 1, 0, height * brickHeight - 1);

		for(int x = 0; x < width; x++)
		{
			for(int y = 0; y < height; y++)
			{
				Vector3 worldPoint = new Vector3(x * brickWidth, brickHeight/2 , y * brickHeight);

				if( noise.GetPixel(x, y) == Color.black)
				{
					GameObject tar = GameObject.Instantiate(wallObject, worldPoint, Quaternion.identity);					
					tar.transform.localScale = brickScale;
					tar.transform.parent = transform;
				}
				else
				{
					if(playerInstance == null)
					{
						GameObject pl = GameObject.Instantiate(playerObject, worldPoint, Quaternion.identity);
						pl.name = "Player";
						pl.transform.localScale = brickScale;
						playerInstance = pl.GetComponent<Player>();
					}
					else if(x >= (int)width * 0.8f && y >= (int) height * 0.8f && enemiesSpawned < enemyCount)
					{						
						GameObject en = GameObject.Instantiate(enemyObjects[enemiesSpawned], worldPoint, Quaternion.identity);
						enemiesSpawned++;
						Enemy e = en.GetComponent<Enemy>();
						e.map = this;
						e.id = enemiesSpawned;
						e.target = playerInstance.transform;						
						en.transform.localScale = brickScale;						
					}
				}
				
				bool walkable = !(Physics.CheckSphere(worldPoint, brickWidth/4, obstacleLayer));
				grid[x, y] = new Node(walkable, worldPoint, x, y);
			}
		}
	}
	
	void OnDrawGizmos()
	{
		if(grid != null)
		{
			Node playerNode = NodeFromWorldPoint(playerInstance.transform.position);
			foreach (Node n in grid)
			{
				if (n == playerNode)
					Gizmos.color = new Color(0, 1, 0, 0.5f);
				else if (n.walkable)
					Gizmos.color = new Color(1, 1, 1, 0.5f);
				else
					Gizmos.color = new Color(1, 0, 0, 0.5f);

				if(debugPath != null)
				{
					if(debugPath.Contains(n))
						Gizmos.color = new Color(0, 0, 1, 0.5f);
				}
				Gizmos.DrawCube(n.worldPosition + brickScale.y/2 * Vector3.up, new Vector3(brickScale.x, 0.1f, brickScale.z));
			}
		}
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		int x = Mathf.RoundToInt(worldPosition.x/brickWidth);
		int y = Mathf.RoundToInt(worldPosition.z/brickHeight);

		x = Mathf.Clamp(x, 0, width-1);
		y = Mathf.Clamp(y, 0, height-1);

		return grid[x, y];
	}

	public List<Node> GetNeighbours(Node node, bool includeDiagonals = true)
	{
		List<Node> neighbours = new List<Node>();

		for(int x=-1; x<=1; x++)
		{
			for(int y=-1; y<=1; y++)
			{
				if((x==0 && y==0) || (x==-1 && !includeDiagonals && (y==-1 || y==1)) ||
					(x==1 && !includeDiagonals && (y==-1 || y==1)))
					continue;
				
				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if(checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
					neighbours.Add(grid[checkX, checkY]);
			}
		}

		return neighbours;
	}
}
