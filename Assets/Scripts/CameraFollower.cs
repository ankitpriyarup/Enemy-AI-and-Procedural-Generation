using UnityEngine;

public class CameraFollower : MonoBehaviour
{
	public Transform player;
	public Vector3 offset;

    Vector3 velocity = Vector3.zero;

	void LateUpdate () 
    {
		if(player == null)
			return;
		
        Vector3 pos = player.position;
        transform.position = Vector3.SmoothDamp(transform.position,
                                                new Vector3(pos.x + offset.x, transform.position.y + offset.y, pos.z + offset.z),
                                                ref velocity,
                                                0.3f);
    }
}
