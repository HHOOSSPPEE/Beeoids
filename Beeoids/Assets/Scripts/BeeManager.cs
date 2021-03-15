/* mhosster - Merry Hospelhorn
 * 2021 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BeeManager : MonoBehaviour
{

    Transform target;
    public Transform[] targets;

    [Header("Bee Settings")]
    Vector3 velocity;
    public float speed, maxSpeed;
    public float separationDist;

    [Header("Weights")]
    public float weightAlignment;
    public float weightCohesion;
    public float weightSeparation;
    public float weightSeeking;

    ParticleSystem beeSystem;
    ParticleSystem.Particle[] beeParticles;
    Transform[] beeTargets;


    //
    Vector3 alignment, separation, cohesion, seeking;

    void LateUpdate()
    {
        SpawnBees();

        // GetParticles is allocation free because we reuse the m_Particles buffer between updates
        int numParticlesAlive = beeSystem.GetParticles(beeParticles);

        Beehaviour(numParticlesAlive);

        // Apply the particle changes to the Particle System
        beeSystem.SetParticles(beeParticles, numParticlesAlive);
    }

    void SpawnBees()
    {
        if (beeSystem == null) {
            beeSystem = GetComponent<ParticleSystem>();
            beeTargets = new Transform[beeSystem.main.maxParticles];
            for (int i = 0; i < beeTargets.Length; i++)
            {
                float newTarget = Random.value * targets.Length;
                beeTargets[i] = targets[(int)newTarget];
            }
        }

        if (beeParticles == null || beeParticles.Length < beeSystem.main.maxParticles)
        {
            beeParticles = new ParticleSystem.Particle[beeSystem.main.maxParticles];
            velocity = new Vector3((Random.value - 0.5f) * 2f, (Random.value - 0.5f) * 2f, (Random.value - 0.5f) * 2f);            
        }

    }

    void Beehaviour(int bees) 
    {
        
        for (int i = 0; i < bees; i++)
        {
            //core loop of steering!
            Vector3 currentPos = beeParticles[i].position;
            velocity = beeParticles[i].velocity;
            Vector3 lastVelocity = velocity;

            float randomTarget = Random.value;
            if (randomTarget > 0.99f) //1% chance bee will change target
            {
                float newTarget = Random.value * targets.Length;
                beeTargets[i] = targets[(int)newTarget];
            }
            target = beeTargets[i];

            //initialize the behavior vectors
            alignment = Vector3.zero;
            separation = Vector3.zero;
            cohesion = Vector3.zero;
            seeking = Vector3.zero;


            //compare ourselves against all other boids
            for (int j = 0; j < bees; j++)
            {
                if (i != j) {

                    alignment += beeParticles[j].velocity.normalized;
                    cohesion += beeParticles[j].position;

                    //separatation logic
                    Vector3 directBetween = beeParticles[i].position - beeParticles[j].position; //vector between us and them
                    if (directBetween.magnitude > 0 && directBetween.magnitude < separationDist) //avoid divide by zero && if you are too close
                    {
                        separation += (directBetween.normalized / directBetween.magnitude) * separationDist;
                    }
                }

            }


            seeking = target.position - beeParticles[i].position;

            //we need to average the behavior vectors
            alignment /= (bees);
            cohesion /= (bees);
            cohesion -= beeParticles[i].position; //turn into that direction
            separation /= (bees);

            Vector3 newVelocity = Vector3.zero; ///initilize our new heading
            newVelocity += alignment * weightAlignment;
            newVelocity += cohesion * weightCohesion;
            newVelocity += separation * weightSeparation;
            newVelocity += seeking * weightSeeking;

            velocity = Vector3.Lerp(velocity, newVelocity, Time.deltaTime * .01f); //0.2f
            velocity = Limit(velocity, maxSpeed);

            float dot = ((lastVelocity.x * velocity.x) + (lastVelocity.y * velocity.y)); //gets a jolt of energy when it gets a new target

            beeParticles[i].velocity = velocity.normalized;
            //beeParticles[i].position = currentPos + velocity * (Time.deltaTime * speed);
            beeParticles[i].position = currentPos + velocity * (Time.deltaTime * (dot*speed));
        }
    }

    Vector3 Limit(Vector3 vectorToClamp, float maxLength)
    {
        if (vectorToClamp.magnitude > maxLength)
            return vectorToClamp.normalized * maxLength;
        else
            return vectorToClamp;
    }

}
