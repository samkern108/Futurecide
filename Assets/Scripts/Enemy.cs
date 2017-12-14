﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

	public GameObject projectile;

	private Vector3 size;
	private LayerMask layerMask;

	private bool detectedPlayer = false;

	private float detectionRadius = 4.0f;

	private Bounds floor;

	private Vector3 startPosition, persistentTargetPosition;

	private Animator animator;

	void Start () {
		size = GetComponent <SpriteRenderer>().bounds.extents;

		Vector3 newPosition = Room.GetRandomPointInRoom ();
		RaycastHit2D hit = Physics2D.Raycast (newPosition, Vector2.down, 10.0f, 1 << LayerMask.NameToLayer("Wall"));
		if (!hit.collider) {
			newPosition.y += size.y;
			hit = Physics2D.Raycast (newPosition, Vector2.down, 10.0f, 1 << LayerMask.NameToLayer ("Wall"));
			Debug.DrawRay (newPosition, Vector2.down, Color.green, 5.0f);
			Debug.Log ("hit top");
		} else {
			Debug.Log ("hit center with normal " + hit.point.y);
		}

		if (!hit.collider) {
			newPosition.y -= size.y * 2;
			hit = Physics2D.Raycast (newPosition, Vector2.down, 10.0f, 1 << LayerMask.NameToLayer ("Wall"));
			Debug.DrawRay (newPosition, Vector2.down,Color.blue,5.0f);
			Debug.Log ("hit bottom");
		}
			
		newPosition.y = hit.point.y + size.y;

		floor = hit.collider.bounds;

		transform.position = newPosition;
		startPosition = newPosition;

		layerMask = 1 << LayerMask.NameToLayer ("Wall") | 1 << LayerMask.NameToLayer("Player");

		animator = GetComponent <Animator>();
	}

	void Update () {
		RaycastHit2D hit = Physics2D.Linecast (transform.position, PlayerController.PlayerPosition, layerMask);
		if (hit.collider.tag == "Wall") {
			MoveToStart ();
			detectedPlayer = false;
			return;
		} else if (hit.distance < detectionRadius) {
			MoveToPlayer ();
			if (!detectedPlayer) {
				detectedPlayer = true;
				StartCoroutine ("Co_Shoot");
			}
		}
	}

	public float smoothDampTime = 0.2f;
	private void MoveToPlayer() {
		Vector3 targetPosition = PlayerController.PlayerPosition;
		targetPosition.x = Mathf.Clamp (targetPosition.x, floor.center.x - floor.extents.x, floor.center.x + floor.extents.x);
		targetPosition.y = transform.position.y;
		Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, 2.0f * Time.deltaTime);
		//Vector3 newPosition = Vector3.SmoothDamp( transform.position, targetPosition, ref _smoothDampVelocity, smoothDampTime );
		// TODO(samkern): Which looks better, smoothdamp or lerp?
		transform.position = newPosition;
	}

	private void MoveToStart() {
		Vector3 targetPosition = startPosition;
		targetPosition.x = Mathf.Clamp (targetPosition.x, floor.center.x - floor.extents.x, floor.center.x + floor.extents.x);
		targetPosition.y = transform.position.y;
		Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, .5f * Time.deltaTime);
		//Vector3 newPosition = Vector3.SmoothDamp( transform.position, targetPosition, ref _smoothDampVelocity, smoothDampTime );
		// TODO(samkern): Which looks better, smoothdamp or lerp?
		transform.position = newPosition;
	}

	private float shootCooldown = 1.0f;
	private float projectileSpeed = 7.0f;
	private IEnumerator Co_Shoot()
	{
		while (detectedPlayer) {
			Shoot ();
			yield return new WaitForSeconds (shootCooldown);
		}
	}

	private void Shoot() {
		AudioManager.PlayEnemyShoot ();
		animator.Play ("Shoot");
		Vector3 direction = (PlayerController.PlayerPosition - transform.position).normalized;
		GameObject missile = Instantiate (projectile);
		missile.transform.position = transform.position;
		missile.GetComponent <Missile>().Initialize(direction, projectileSpeed);
	}
}
