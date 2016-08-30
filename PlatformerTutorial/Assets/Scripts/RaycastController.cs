using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	public LayerMask collisionMask;

	public const float skinWidth = .015f;
	const float dstBetweenRays = .25f;
	[HideInInspector]
	public int horizontalRayCount;// = 4; //Number of horizontal raycasts
	[HideInInspector]
	public int verticalRayCount;// = 4; //Number of vertical raycasts

	[HideInInspector]
	public float horizontalRaySpacing; //horizontal space between raycasts
	[HideInInspector]
	public float verticalRaySpacing; //vertical space between raycasts

	[HideInInspector]
	public BoxCollider2D collider;	
	public RaycastOrigins raycastOrigins;	

	public virtual void Awake(){
		collider = GetComponent<BoxCollider2D>();

	}

	public virtual void Start(){
		CalculateRaySpacing();				
	}


	public void UpdateRaycastOrigins(){
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);

		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
	}

	public void CalculateRaySpacing(){
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);		

		//Calculate number of horizontal/vvertical rays
		float boundsWidth= bounds.size.x;
		float boundsHeight= bounds.size.y;
		horizontalRayCount = Mathf.RoundToInt(boundsHeight/dstBetweenRays);
		verticalRayCount = Mathf.RoundToInt(boundsWidth/dstBetweenRays);

		horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	public struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
