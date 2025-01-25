using NUnit.Framework;
using Platformer.Mechanics;
using UnityEngine;
using System.Collections.Generic;
namespace Platformer.Model
{
    /// <summary>
    /// The main model containing needed data to implement a platformer style 
    /// game. This class should only contain data, and methods that operate 
    /// on the data. It is initialised with data in the GameController class.
    /// </summary>
    [System.Serializable]
    public class PlatformerModel
    {
        /// <summary>
        /// The virtual camera in the scene.
        /// </summary>
        public Unity.Cinemachine.CinemachineCamera virtualCamera;

        public GameUIController UIcontroller;
        public ComboUIController comboUIcontroller;
        public HealthBarController healthBarController;

        /// <summary>
        /// The main component which controls the player sprite, controlled 
        /// by the user.
        /// </summary>
        //public PlayerController player;

        /// <summary>
        /// The spawn point in the scene.
        /// </summary>]
        public List<Transform> spawnPointsContainers = new();
        public List<GameObject> maps;
        public List<Transform> mapMarkers = new();
        public Transform topLeft;
        public Transform topRight;

        /// <summary>
        /// A global jump modifier applied to all initial jump velocities.
        /// </summary>
        public float jumpModifier = 1.5f;

        /// <summary>
        /// A global jump modifier applied to slow down an active jump when 
        /// the user releases the jump input.
        /// </summary>
        public float jumpDeceleration = 0.5f;

    }
}