using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{	
	public float moveSpeed = 7f;
	public float shootSpeed = 15f;
	public GameObject bulletObject;	
	public int maxHealth = 100;

	[HideInInspector]
	public int health;
	[HideInInspector]
	public bool isDead = false;

	Rigidbody rigidBody;

	void Awake()
	{
		rigidBody = GetComponent<Rigidbody>();
	}

	void Start()
	{
		Camera.main.GetComponent<CameraFollower>().player = transform;
		health = maxHealth;
	}

	void FixedUpdate()
	{
		if(isDead)
			return;

		float moveX = Input.GetAxis("Horizontal");
		float moveZ = Input.GetAxis("Vertical");
		Vector3 curVelocity = rigidBody.velocity;
		rigidBody.velocity = new Vector3(moveSpeed * moveX, curVelocity.y, moveSpeed * moveZ);
		Vector3 movement = new Vector3(moveX, 0.0f, moveZ);
		if(movement != Vector3.zero)
 			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), 0.15F);
	}

	void Update()
	{
		if(isDead)
			return;
		
		if(Input.GetKeyDown(KeyCode.Space))
		{
			GameObject bullet = GameObject.Instantiate(bulletObject, transform.position, Quaternion.identity);
			Vector3 curScale = bullet.transform.localScale;
			Vector3 thisScale = transform.localScale;
			bullet.GetComponent<Bullet>().playerFwd = transform.forward;
			bullet.transform.localScale = new Vector3(curScale.x * thisScale.x, curScale.y * thisScale.y, curScale.z * thisScale.z);
		}
	}

	void OnGUI()
	{
		if(isDead)
			return;

		GUIStyle style =  new GUIStyle();
		style.fontSize = 15;
		style.normal.textColor = Color.white;
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);		
		GUILayout.Label("Player", style, GUILayout.Width(70));
		GUILayout.HorizontalScrollbar(0, health, 0, maxHealth, GUILayout.Width(200));
		GUILayout.EndHorizontal();
	}

	public void Damage(int val)
	{
		if(health <= 0)
		{
			isDead = true;
			Invoke("reloadScene", 1.0f);
		}
		else
			health-=val;
	}

	void reloadScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
