﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour {

	public GameObject projectile;
	private float playerWidth;

	private bool active = false;

	private float[] positions;
	private int positionIndex = 0;

	private int spriteFlipped = 1;

	private Animate animate;
	private SpriteRenderer spriteRenderer;

	private float projectileSpeed = 8.0f;

	private Vector3 oldPosition, nextPosition;
	private float playbackRate = 1f, lerpFraction = .0f;

	private Color currentColor;

	public void Awake() {
		spriteRenderer = GetComponent <SpriteRenderer> ();
		animate = GetComponent <Animate>();

		playerWidth = spriteRenderer.bounds.size.x;

		currentColor = Palette.GhostColor;
		currentColor.a = .9f;
		spriteRenderer.color = currentColor;
	}

	public void Initialize (float[] positions) {
		this.positions = positions;
	}

	public void EnactRoutine () {
		currentColor.a -= .05f;
		playbackRate -= .1f;

		if (playbackRate <= 0)
			Destroy (this.gameObject);

		active = true;
		positionIndex = 0;
		transform.position = new Vector3 (positions [positionIndex], positions [positionIndex + 1], 0);
		animate.AnimateToColor (Palette.Invisible,currentColor,.3f);
	}

	private void SetInactive() {
		gameObject.SetActive (false);
	}

	public void FixedUpdate() {
		if (active && positionIndex >= positions.Length - 3) {
			active = !active;
			animate.AnimateToColor (currentColor, Palette.Invisible, .3f);
			Invoke ("SetInactive", .3f);
		}
		if (!active)
			return;

		lerpFraction += playbackRate;

		if (lerpFraction >= 1.0f) {
			lerpFraction -= 1.0f;

			if (((positionIndex + (4 * 30) - 1) < positions.Length) && (positions [positionIndex + (4 * 30) - 1] == 1))
				TelemarkShoot ();

			oldPosition = nextPosition;

			// (x | y | flip | shoot)
			nextPosition = new Vector3 (positions [positionIndex++], positions [positionIndex++], 0);

			if (positions [positionIndex++] != spriteFlipped) {
				spriteFlipped *= -1;
				GetComponent<SpriteRenderer> ().flipX = (spriteFlipped == -1);
			}
			if (positions [positionIndex++] == 1)
				Shoot ();
		}

		transform.position = Vector3.Lerp (oldPosition, nextPosition, lerpFraction);
	}

	private void TelemarkShoot() {
		animate.AnimateToColor (currentColor,Color.red,.3f);
	}

	// Lot of duplicate shoot code
	private void Shoot() {
		animate.AnimateToColor (Color.red,currentColor,.3f);
		AudioManager.PlayEnemyShoot ();
		float direction = spriteFlipped;
		GameObject missile = Instantiate (projectile, ProjectileManager.myTransform);
		missile.transform.position = transform.position + new Vector3((playerWidth * direction), 0, 0);
		missile.GetComponent <Missile>().Initialize(Vector3.right * direction, projectileSpeed * playbackRate);
	}
}
