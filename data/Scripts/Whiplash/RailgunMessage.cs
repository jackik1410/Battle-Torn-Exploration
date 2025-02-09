﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using ProtoBuf;

namespace Whiplash.Railgun
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class RailgunMessage : MySessionComponentBase
    {
        bool scriptInit = false;
        const ushort NetID = 1564; 

        public override void UpdateBeforeSimulation()
        {

            if (scriptInit == false)
            {

                scriptInit = true;

                MyAPIGateway.Multiplayer.RegisterMessageHandler(NetID, ProcessClient);

            }
        }

        public static void SendToClients(RailgunFireData data)
        {
            //Below, save your data to string or some other serializable type
            var sendData = MyAPIGateway.Utilities.SerializeToBinary(data);

            //Send the message to the ID you registered in Setup, and specify the user via SteamId
            bool sendStatus = MyAPIGateway.Multiplayer.SendMessageToOthers(NetID, sendData);    
        }

        public static void ProcessClient(byte[] data)
        {
            if (MyAPIGateway.Utilities.IsDedicated || (!MyAPIGateway.Utilities.IsDedicated && MyAPIGateway.Multiplayer.IsServer))
                return;

            //This converts your data back to the original type you had before you sent
            //Depending on what you sent, you may need to parse it back into something usable
            var receivedData = MyAPIGateway.Utilities.SerializeFromBinary<RailgunFireData>(data);

            IMyEntity ent;
            var entityExists = MyAPIGateway.Entities.TryGetEntityById(receivedData.ShooterID, out ent);
            if (entityExists)
            {
                RailgunCore.ShootProjectileClient(receivedData);
            }
        }

        protected override void UnloadData()
        {
            //Unregister the Message Handler on Unload. I guess these persist?
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(NetID, ProcessClient);
        }

    }

    [ProtoContract]
    public struct RailgunFireData
    {
        [ProtoMember]
        public Vector3D Origin;

        [ProtoMember]
        public Vector3D Direction;

        [ProtoMember]
        public Vector3D ShooterVelocity;

        [ProtoMember]
        public long ShooterID;
    }

}