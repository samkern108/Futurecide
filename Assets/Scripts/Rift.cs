﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MovementEffects;

public class Rift : MonoBehaviour, IRestartObserver, IGhostDeathObserver, IPlayerShootObserver {

	GhostAIStats ghostStats;
	ParticleSystem ps;
	BoxCollider2D boxCollider;

	// TODO(samkern): Should rifts get bigger over time? Spawn little enemies? How should we close them?
	// TODO(samkern): Idea: maybe if shit's really hitting the fan, a checkpoint has a random chance of spawning a friendly ghost? Or like, if you catch it really early it becomes friendly?

	private bool activated = false;

	private float timeOpened;

	private AnimationCurve psSizeCurve;

	public void Start() {
		timeOpened = Time.time;

		ghostStats = new GhostAIStats ();

		ps = GetComponent <ParticleSystem>();
		boxCollider = GetComponent <BoxCollider2D>();

		NotificationMaster.restartObservers.Add (this);
		NotificationMaster.ghostDeathObservers.Add (this);
		NotificationMaster.playerShootObservers.Add (this);

		psSizeCurve = new AnimationCurve();
		psSizeCurve.AddKey(0.0f, 0.0f);
		psSizeCurve.AddKey(1.0f, 1.0f);

		Vector3 point;
		float distance, minDistance;
		do {
			point = Room.GetRandomPointInRoom ();

			distance = Vector2.Distance (PlayerController.PlayerPosition, point);
			minDistance = 2.0f;
		} while (distance < minDistance);

		transform.position = point;

		StartCoroutine ("C_AnimateSize");
	}
		
	private float psRadius = 0.0f;
	private float endSize = .0f;
	private IEnumerator C_AnimateSize () {
		Vector3 newSize;
		float delay;
		while(true) {
			var shape = ps.shape;

			psRadius += .05f;
			endSize += .1f;

			shape.radius = psRadius;

			var size = ps.sizeOverLifetime;
			boxCollider.size = new Vector2 (psRadius, psRadius);

			size.size = new ParticleSystem.MinMaxCurve(endSize, psSizeCurve);

			// TODO(samkern): Figure out an appropriate scaling measure between rift & ghost
			delay = Random.Range (3.0f, 8.0f);
			yield return new WaitForSeconds(delay);
		}
	}

	void OnTriggerEnter2D(Collider2D coll) {
		if (!activated) {
			ghostStats.timeOpen = Time.time - timeOpened;

			// For reference, the player is .14 scale
			ghostStats.size = psRadius;

			// TODO(samkern): Should we record ALL ghosts killed while the rift is active, or just some? Should they decay over time?
			// ghosts alive during this rift's time in the level = ghosts currently alive plus ghosts murdered.
			ghostStats.totalGhostsInLevel = (GhostManager.instance.children.Count + ghostStats.ghostsKilled);
			ghostStats.totalGhostAggressiveness = GhostManager.instance.TotalGhostAggressiveness ();

			GhostManager.instance.SpawnGhost (ghostStats);

			NotificationMaster.SendCheckpointReachedNotification (Time.time - timeOpened);
			AudioManager.PlayDotPickup ();
			Destroy (this.gameObject);
		}
	}

	public void Restart() {
		Destroy (this.gameObject);
	}

	public void GhostDied(GhostAIStats stats) {
		ghostStats.ghostsKilled++;
		ghostStats.killedGhostAggressiveness += stats.Aggressiveness ();
	}

	public void PlayerShoot() {
		ghostStats.shotsFired++;
	}
}
