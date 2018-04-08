using UnityEngine;

public class Bullet : MonoBehaviour
{	
	public LayerMask collisionLayer;
	public float speed = 10;
	public int damage = 10;
	public float damageProbability = 1.0f;

	[HideInInspector]
	public Vector3 playerFwd;
	
	Vector3 velocityPos;

	void Start()
	{
		Invoke("DestroyInstance", 5);
		velocityPos = transform.position;
	}

	void FixedUpdate()
	{
		velocityPos += speed * playerFwd;
		float fwdDot = Vector3.Dot(transform.forward, velocityPos);
		float upDot = Vector3.Dot(transform.up, velocityPos);
		float rightDot = Vector3.Dot(transform.right, velocityPos);
		
		Vector3 velocityVector = new Vector3(rightDot, upDot, fwdDot);
		transform.position = velocityVector;
	}

	void Update()
	{
		Vector3 fwd = transform.TransformDirection(playerFwd);
		RaycastHit hit;
		if (Physics.Raycast(transform.position, fwd, out hit, 1, collisionLayer))
		{
			if(hit.collider == null)
				return;

			GameObject gb = hit.collider.gameObject;
			if(gb.tag == "Player")
			{
				float damageProb = Random.Range(damage - damageProbability, damage + damageProbability);
				gb.GetComponent<Player>().Damage(Mathf.RoundToInt(damageProb));
			}
			if(gb.tag == "Enemy")
				gb.GetComponent<Enemy>().Damage(damage);
			
			DestroyInstance();
		}
	}

	void DestroyInstance()
	{
		Destroy(gameObject);
	}
}
