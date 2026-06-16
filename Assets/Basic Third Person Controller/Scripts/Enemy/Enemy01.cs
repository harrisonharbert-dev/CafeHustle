using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a basic enemy with simple movement behavior.
/// </summary>
/// 
namespace PbwanskaasProductions.BasicThirdPersonController
{
    public class Enemy01 : MonoBehaviour
    {
        [Tooltip("The Rigidbody component of the enemy.")]
        public Rigidbody rb;

        /// <summary>
        /// This method is called before the first frame update.
        /// </summary>
        void Start()
        {

        }

        /// <summary>
        /// This method is called once per frame.
        /// Updates the enemy's velocity to a constant value.
        /// </summary>
        void Update()
        {
            rb.linearVelocity = new Vector3(0.5f, 0, 0.5f);
        }
    }
}