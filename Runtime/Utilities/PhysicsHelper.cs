using System;
using System.Collections;
using UnityEngine;

namespace UnityExtended.Utilities {
    public static class PhysicsHelper {
        /// <summary>
        /// Predicts linear velocity of a <see cref="Rigidbody"/> on the next physics update 
        /// after application of accumulated forces, but before the collision and friction forces.
        /// <para>Intended to be used before internal physics update (on FixedUpdate).</para>
        /// <para>When tested, method provided exclusively correct results
        /// (predictions before physics simulation coincided with actual forces after physics simulation without collisions).</para>
        /// </summary>
        /// <param name="rb"><see cref="Rigidbody"/>, linear velocity of which is calculated.</param>
        /// <param name="accountDrag">Whether to account rigidbody's drag into calculations. Set to true for accuracy.</param>
        /// <param name="accountGravity">Whether to account rigidbody's gravity into calculations. Set to true for accuracy.</param>
        /// <returns>Linear velocity of a rigidbody right after application of accumulated forces in the internal physics update.</returns>
        public static Vector3 PredictAccumulatedLinearVelocity(Rigidbody rb, bool accountDrag = true, bool accountGravity = true) {
            float fixedDT = Time.fixedDeltaTime;

            Vector3 accumulatedForce = rb.GetAccumulatedForce();
            Vector3 velocityDelta = accumulatedForce * fixedDT / rb.mass; // turn accumulated force into velocity delta
            Vector3 velocityPostApplication = rb.velocity + velocityDelta;

            if (accountGravity && rb.useGravity) { // gravity isn't an accumulated force so it's getting calculated separately in here
                velocityPostApplication += Physics.gravity * fixedDT;
            }

            Vector3 predicted = velocityPostApplication;

            if (accountDrag) {
                float dragMultiplier = Mathf.Clamp01(1 - rb.drag * fixedDT); // people figured out the drag formula a while ago
                predicted *= dragMultiplier;
            }

            return predicted;
        }

        /// <summary>
        /// Makes Unity consistently invoke supplied function after physics update (technically after all the OnCollisionXXX messages). 
        /// </summary>
        /// <param name="monoBehaviour"><see cref="MonoBehaviour"/> to which a coroutine will be attached.</param>
        /// <param name="function">Invoked function.</param>
        public static void InvokePostPhysicsUpdate(this MonoBehaviour monoBehaviour, Action function) {
            monoBehaviour.StartCoroutine(Impl(function));

            static IEnumerator Impl(Action function) {
                while (true) {
                    yield return new WaitForFixedUpdate();

                    function();
                }
            }
        }
    }
}
