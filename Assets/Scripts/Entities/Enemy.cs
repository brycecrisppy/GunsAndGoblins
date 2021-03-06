using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : NPC {
    
    // -- Variables --
    public float projectileSpeed = 0.3f;
    public int maxProjectiles = 3;
    public float projectileInterpolationPeriod = 3;
    public float projectileRotation = 0.5f;
    
    private float projectileTimer = 0.0f;
    private float projectileWaitTimer = 0.0f;
    private int numberOfProjectiles = 0;

    private bool canShoot = false;
    
    // -- Components --
    public GameObject projectilePrefab;

    private GameObject player;
    private List<GameObject> spawnedProjectiles = new List<GameObject>();

    // Start is called before the first frame update
    protected override void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update() {
        RemoveProjectiles();

        if (numberOfProjectiles == maxProjectiles) {
            CalculateProjectileWait();
            canShoot = false;
        } else {
            if (IsAlive()) CalculateTime();
        }
        
        if (IsAlive()) MoveProjectiles();
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (!other.transform.CompareTag("Player")) return;
        if (other.GetComponent<Player>().godMode) return;
        
        if (numberOfProjectiles < maxProjectiles) canShoot = true;
    }

    // -- Utility Methods --
    private void PrintProjectile() {
        var prefab = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        prefab.GetComponent<EnemyBullet>().damage = damage;
        spawnedProjectiles.Add(prefab);
        Destroy(prefab, deletePrefabTime);
        numberOfProjectiles += 1;
    }

    private void MoveProjectiles() {
        foreach (var projectile in spawnedProjectiles) {
            if (projectile != null) {
                projectile.transform.position = Vector3.MoveTowards(
                    projectile.transform.position, player.transform.position, 
                    projectileSpeed * Time.fixedDeltaTime);

                var rotation = new Vector3(0, 0, projectileRotation);
                projectile.transform.Rotate(rotation);
            }
        }
    }

    private void RemoveProjectiles() {
        if (spawnedProjectiles.Count > 0) spawnedProjectiles.RemoveAll(item => item == null);
    }

    protected override void CalculateTime() {
        bool interpolatedPeriodReached = projectileTimer >= interpolationPeriod;
        
        projectileTimer += Time.fixedDeltaTime;
        
        if (!interpolatedPeriodReached) return;
        if (canShoot) PrintProjectile();
        
        projectileTimer -= interpolationPeriod;
        
        base.CalculateTime();
    }

    private void CalculateProjectileWait() {
        bool interpolatedPeriodReached = projectileWaitTimer >= projectileInterpolationPeriod;
        projectileWaitTimer += Time.fixedDeltaTime;
        
        if (!interpolatedPeriodReached) return;
        
        projectileWaitTimer -= projectileInterpolationPeriod;

        canShoot = true;
        numberOfProjectiles = 0;
    }

    protected override void Kill() {
        if (IsAlive()) return;
        foreach (var projectile in spawnedProjectiles) {
            Debug.Log("Delete projectile");
            Destroy(projectile);
        }
        
        base.Kill();
    }
}
