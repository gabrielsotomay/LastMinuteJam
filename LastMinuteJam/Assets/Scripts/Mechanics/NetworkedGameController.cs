using Platformer.Core;
using Platformer.Model;
using UnityEngine;
using Netick;
using Netick.Unity;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This class exposes the the game model in the inspector, and ticks the
    /// simulation.
    /// </summary> 
    public class NetworkedGameController : NetworkEventsListener
    {
        public static NetworkedGameController Instance { get; private set; }

        //This model field is public and can be therefore be modified in the 
        //inspector.
        //The reference actually comes from the InstanceRegister, and is shared
        //through the simulation and events. Unity will deserialize over this
        //shared reference when the scene loads, allowing the model to be
        //conveniently configured inside the inspector.
        public PlatformerModel model = SimulationNetick.GetModel<PlatformerModel>();

        void OnEnable()
        {
            Instance = this;
        }

        void OnDisable()
        {
            if (Instance == this) Instance = null;
        }

        public  void TickSimulation()
        {
            if (Instance == this)
            {
                //Debug.Log("Ticking " + Sandbox.Tick.TickValue);
                SimulationNetick.Tick(Sandbox.Tick.TickValue);
            }
        }


    }
}