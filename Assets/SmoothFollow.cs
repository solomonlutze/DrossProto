﻿ using UnityEngine;
 using System.Collections;
 
 public class SmoothFollow : MonoBehaviour {
	
	public float dampTime = 0.15f;
	private Vector3 velocity = Vector3.zero;
	public Transform target;
	public Camera c;

	void Start () {
		c = GetComponent<Camera>();
	}

	void Update () {
		if (GameMaster.Instance.GetPlayerController() != null) {
			target = GameMaster.Instance.GetPlayerController().transform;
			Vector3 point = c.WorldToViewportPoint(target.position);
			Vector3 delta = target.position - c.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
			Vector3 destination = transform.position + delta;
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
		}
	
	}
 }