using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages global game settings, such as the target frame rate.
/// </summary>
/// 
namespace PbwanskaasProductions.BasicThirdPersonController
{
    public class GameManager : MonoBehaviour
    {
        [Tooltip("The target frame rate for the application.")]
        public int frame;

        /// <summary>
        /// This method is called before the first frame update.
        /// It sets the application's target frame rate.
        /// </summary>
        void Start()
        {
            Application.targetFrameRate = frame;
        }

        /// <summary>
        /// This method is called once per frame.
        /// </summary>
        void Update()
        {

        }
    }
}