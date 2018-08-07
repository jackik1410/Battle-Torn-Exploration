using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using Sandbox.Game.Weapons;
using VRage.Game.ModAPI;
using VRageMath;
using Sandbox.Game;
using VRage.Game.Entity;
using Sandbox.Game.Entities;
using VRage.Game.ModAPI.Interfaces;
using Sandbox.Definitions;
using Whiplash.ArmorPiercingProjectiles;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using Rexxar;

namespace Whiplash.Railgun
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_LargeGatlingTurret), false, "LargeRailgunTurretLZM")]
    public class RailgunTurret : Railgun
    {
        static bool _terminalControlsInit = false;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            try
            {
                CreateCustomTerminalControls();
            }
            catch (Exception e)
            {

                MyAPIGateway.Utilities.ShowNotification("Exception in railgun turret init", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }

        void CreateCustomTerminalControls()
        {
            if (_terminalControlsInit)
                return;

            _terminalControlsInit = true;

            IMyTerminalControlOnOffSwitch rechargeControl = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyLargeGatlingTurret>("RechargeRailgun");
            rechargeControl.Title = MyStringId.GetOrCompute("Recharge Railgun");
            rechargeControl.Enabled = x => x.BlockDefinition.SubtypeId.Equals("LargeRailgunTurretLZM");
            rechargeControl.Visible = x => x.BlockDefinition.SubtypeId.Equals("LargeRailgunTurretLZM");
            rechargeControl.SupportsMultipleBlocks = true;
            rechargeControl.OnText = MyStringId.GetOrCompute("On");
            rechargeControl.OffText = MyStringId.GetOrCompute("Off");
            rechargeControl.Setter = (x, v) => SetRecharging(x, v);
            rechargeControl.Getter = x => GetRecharging(x);
            MyAPIGateway.TerminalControls.AddControl<IMyLargeGatlingTurret>(rechargeControl);

            //Recharge toggle action
            IMyTerminalAction rechargeOnOff = MyAPIGateway.TerminalControls.CreateAction<IMyLargeGatlingTurret>("Recharge_OnOff");
            rechargeOnOff.Action = (x) =>
            {
                var recharge = GetRecharging(x);
                SetRecharging(x, !recharge);
            };
            rechargeOnOff.ValidForGroups = true;
            rechargeOnOff.Writer = (x, s) => GetWriter(x, s);
            rechargeOnOff.Icon = @"Textures\GUI\Icons\Actions\Toggle.dds";
            rechargeOnOff.Enabled = x => x.BlockDefinition.SubtypeId.Equals("LargeRailgunTurretLZM");
            rechargeOnOff.Name = new StringBuilder("Recharge On/Off");
            MyAPIGateway.TerminalControls.AddAction<IMyLargeGatlingTurret>(rechargeOnOff);

            //Recharge on action
            IMyTerminalAction rechargeOn = MyAPIGateway.TerminalControls.CreateAction<IMyLargeGatlingTurret>("Recharge_On");
            rechargeOn.Action = (x) => SetRecharging(x, true);
            rechargeOn.ValidForGroups = true;
            rechargeOn.Writer = (x, s) => GetWriter(x, s);
            rechargeOn.Icon = @"Textures\GUI\Icons\Actions\SwitchOn.dds";
            rechargeOn.Enabled = x => x.BlockDefinition.SubtypeId.Equals("LargeRailgunTurretLZM");
            rechargeOn.Name = new StringBuilder("Recharge On");
            MyAPIGateway.TerminalControls.AddAction<IMyLargeGatlingTurret>(rechargeOn);

            //Recharge off action
            IMyTerminalAction rechargeOff = MyAPIGateway.TerminalControls.CreateAction<IMyLargeGatlingTurret>("Recharge_Off");
            rechargeOff.Action = (x) => SetRecharging(x, false);
            rechargeOff.ValidForGroups = true;
            rechargeOff.Writer = (x, s) => GetWriter(x, s);
            rechargeOff.Icon = @"Textures\GUI\Icons\Actions\SwitchOff.dds";
            rechargeOff.Enabled = x => x.BlockDefinition.SubtypeId.Equals("LargeRailgunTurretLZM");
            rechargeOff.Name = new StringBuilder("Recharge Off");
            MyAPIGateway.TerminalControls.AddAction<IMyLargeGatlingTurret>(rechargeOff);
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_SmallMissileLauncher), false, "LargeRailGunLZM")]
    public class RailgunFixed : Railgun
    {
        static bool _terminalControlsInit = false;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            try
            {
                CreateCustomTerminalControls();
            }
            catch (Exception e)
            {

                MyAPIGateway.Utilities.ShowNotification("Exception in fixed railgun init", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }

        void CreateCustomTerminalControls()
        {
            if (_terminalControlsInit)
                return;

            _terminalControlsInit = true;

            IMyTerminalControlOnOffSwitch rechargeControl = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMySmallMissileLauncher>("RechargeRailgun");
            rechargeControl.Title = MyStringId.GetOrCompute("Recharge Railgun");
            rechargeControl.Enabled = x => x.BlockDefinition.SubtypeId.Equals("LargeRailGunLZM");
            rechargeControl.Visible = x => x.BlockDefinition.SubtypeId.Equals("LargeRailGunLZM");
            rechargeControl.SupportsMultipleBlocks = true;
            rechargeControl.OnText = MyStringId.GetOrCompute("On");
            rechargeControl.OffText = MyStringId.GetOrCompute("Off");
            rechargeControl.Setter = (x, v) => SetRecharging(x, v);
            rechargeControl.Getter = x => GetRecharging(x);
            MyAPIGateway.TerminalControls.AddControl<IMySmallMissileLauncher>(rechargeControl);

            //Recharge toggle action
            IMyTerminalAction rechargeOnOff = MyAPIGateway.TerminalControls.CreateAction<IMySmallMissileLauncher>("Recharge_OnOff");
            rechargeOnOff.Action = (x) =>
            {
                var recharge = GetRecharging(x);
                SetRecharging(x, !recharge);
            };
            rechargeOnOff.ValidForGroups = true;
            rechargeOnOff.Writer = (x, s) => GetWriter(x, s);
            rechargeOnOff.Icon = @"Textures\GUI\Icons\Actions\Toggle.dds";
            rechargeOnOff.Enabled = x => x.BlockDefinition.SubtypeId.Equals("LargeRailGunLZM");
            rechargeOnOff.Name = new StringBuilder("Recharge On/Off");
            MyAPIGateway.TerminalControls.AddAction<IMySmallMissileLauncher>(rechargeOnOff);

            //Recharge on action
            IMyTerminalAction rechargeOn = MyAPIGateway.TerminalControls.CreateAction<IMySmallMissileLauncher>("Recharge_On");
            rechargeOn.Action = (x) => SetRecharging(x, true);
            rechargeOn.ValidForGroups = true;
            rechargeOn.Writer = (x, s) => GetWriter(x, s);
            rechargeOn.Icon = @"Textures\GUI\Icons\Actions\SwitchOn.dds";
            rechargeOn.Enabled = x => x.BlockDefinition.SubtypeId.Equals("LargeRailGunLZM");
            rechargeOn.Name = new StringBuilder("Recharge On");
            MyAPIGateway.TerminalControls.AddAction<IMySmallMissileLauncher>(rechargeOn);

            //Recharge off action
            IMyTerminalAction rechargeOff = MyAPIGateway.TerminalControls.CreateAction<IMySmallMissileLauncher>("Recharge_Off");
            rechargeOff.Action = (x) => SetRecharging(x, false);
            rechargeOff.ValidForGroups = true;
            rechargeOff.Writer = (x, s) => GetWriter(x, s);
            rechargeOff.Icon = @"Textures\GUI\Icons\Actions\SwitchOff.dds";
            rechargeOff.Enabled = x => x.BlockDefinition.SubtypeId.Equals("LargeRailGunLZM");
            rechargeOff.Name = new StringBuilder("Recharge Off");
            MyAPIGateway.TerminalControls.AddAction<IMySmallMissileLauncher>(rechargeOff);
        }
    }

    public struct RailgunProjectileData
    {
        public float DesiredSpeed;
        public float MaxTrajectory;
        public float ExplosionDamage;
        public float ExplosionRadius;
        public float PenetrationDamage;
        public float PenetrationRange;
        public Vector3 ProjectileTrailColor;
        public float ProjectileTrailScale;
        public bool DrawTracer;
        public bool Explode;
        public bool Penetrate;
    }

    public class Railgun : MyGameLogicComponent
    {
        IMyFunctionalBlock block;
        IMyCubeBlock cube;
        IMyLargeTurretBase turret;
        DateTime _lastShootTime;
        DateTime _currentShootTime;
        //List<ArmorPiercingProjectileSimulation> liveProjectiles = new List<ArmorPiercingProjectileSimulation>();
        float _maxTrajectory;
        float _desiredSpeed;
        float _projectileDamage;
        float _turretMaxRange;
        float _backkickForce;
        int _reloadTime;
        int _reloadTicks;
        int _ticksSinceLastReload = 0;
        int _currentReloadTicks = 0;
        bool _isReloading = false;
        bool _enabledStatus = true;
        bool _firstUpdate = true;
        const float _idlePowerDrawBase = 2f; //MW
        const float _idlePowerDrawMax = 20f; //MW
        const float _reloadPowerDraw = 200f; //MW
        float _powerDrawDecrementPerTick;
        private static readonly MyDefinitionId resourceId = MyResourceDistributorComponent.ElectricityId; //new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");
        Vector3 _trailColor;
        float _trailScale;
        MyResourceSinkComponent sink;
        //public bool Recharge = true;
        MyObjectBuilder_EntityBase _objectBuilder;

        MySoundPair shootSound = new MySoundPair("WepShipLargeRailgunShotLZM");
        MyEntity3DSoundEmitter soundEmitter;

        RailgunProjectileData projectileData;

        public bool _settingsDirty;

        public void GetWriter(IMyTerminalBlock x, StringBuilder s)
        {
            s.Clear();
            var y = x.GameLogic.GetAs<Railgun>();
            var set = Settings.GetSettings(x);

            if (y != null)
            {
                if (set.Recharging)
                    s.Append("On");
                else
                    s.Append("Off");
            }
        }

        public void SetRecharging(IMyTerminalBlock b, bool v)
        {
            var s = Settings.GetSettings(b);
            s.Recharging = v;
            Settings.SetSettings(b, s);
            SetDirty(b);
        }

        public bool GetRecharging(IMyTerminalBlock b)
        {
            return Settings.GetSettings(b).Recharging;
        }

        public void SetDirty(IMyTerminalBlock b)
        {
            var g = b.GameLogic.GetAs<Railgun>();
            if (g != null)
                g._settingsDirty = true;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            try
            {
                _objectBuilder = objectBuilder;
                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                // this.m_missileAmmoDefinition = weaponProperties.GetCurrentAmmoDefinitionAs<MyMissileAmmoDefinition>();

                SetPowerSink();

                cube = (IMyCubeBlock)Entity;
                block = (IMyFunctionalBlock)Entity;

                GetAmmoProperties();

                //get shoot time for initial check
                _lastShootTime = GetLastShootTime();

                GetTurretMaxRange();

                soundEmitter = new MyEntity3DSoundEmitter((MyEntity)Entity, true);

                projectileData = new RailgunProjectileData()
                {
                    DesiredSpeed = _desiredSpeed,
                    MaxTrajectory = _maxTrajectory,
                    ExplosionDamage = 0f,
                    ExplosionRadius = 0f,
                    PenetrationDamage = _projectileDamage,
                    PenetrationRange = 50f,
                    ProjectileTrailColor = _trailColor,
                    ProjectileTrailScale = _trailScale,
                    DrawTracer = true,
                    Explode = true,
                    Penetrate = true
                };

                RailgunCore.RegisterRailgun(Entity.EntityId, projectileData);
            }
            catch (Exception e)
            {
                MyAPIGateway.Utilities.ShowNotification("Exception in init", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }

        public override void Close()
        {
            base.Close();
            //remove from dict
            RailgunCore.UnregisterRailgun(Entity.EntityId);
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            try
            {
                //GetTurretMaxRange();
            }
            catch (Exception e)
            {
                MyAPIGateway.Utilities.ShowNotification("Exception in update once", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            try
            {
                if (cube?.CubeGrid?.Physics == null) //ignore ghost grids
                    return;

                if (MyAPIGateway.Multiplayer.IsServer)
                {
                    Vector3D direction;
                    Vector3D origin;

                    _currentShootTime = GetLastShootTime();

                    //fire weapon
                    if (_currentShootTime != _lastShootTime && !_firstUpdate)
                    {
                        if (Entity is IMyLargeTurretBase)
                        {
                            Vector3D.CreateFromAzimuthAndElevation(turret.Azimuth, turret.Elevation, out direction);
                            direction = Vector3D.TransformNormal(direction, turret.WorldMatrix);
                            origin = turret.WorldMatrix.Translation + turret.WorldMatrix.Up * 1.75 + direction * 0.5;
                        }
                        else
                        {
                            direction = block.WorldMatrix.Forward;
                            origin = block.WorldMatrix.Translation + direction * -2.5; //for cushion
                        }

                        var velocity = block.CubeGrid.Physics.LinearVelocity;
                        //var projectile = new ArmorPiercingProjectileSimulation(origin, direction, velocity, this._desiredSpeed, this._maxTrajectory, 0f, 0f, _projectileDamage, 50f, Entity.EntityId, _trailColor, _trailScale, true, true, true);
                        var fireData = new RailgunFireData()
                        {
                            ShooterVelocity = velocity,
                            Origin = origin,
                            Direction = direction,
                            ShooterID = Entity.EntityId
                        };

                        RailgunCore.ShootProjectileServer(fireData);
                        //RailgunCore.AddProjectile(projectile);

                        _isReloading = true;
                        _currentReloadTicks = 0;

                        //Apply recoil force
                        var centerOfMass = block.CubeGrid.Physics.CenterOfMassWorld;
                        var forceVector = -direction * _backkickForce;

                        block.CubeGrid.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, forceVector, block.GetPosition(), null);

                        //MyAPIGateway.Utilities.ShowNotification("Shot", 3000);
                    }

                    _lastShootTime = _currentShootTime;
                    sink.Update();
                    _firstUpdate = false;
                }

                ShowReloadMessage();

                //MyAPIGateway.Utilities.ShowNotification($"Power draw: {GetPowerInput(false):n0} MW | reloading?: {_isReloading} | max: {sink.MaxRequiredInputByType(resourceId)} | current ticks {_currentReloadTicks}", 16, MyFontEnum.Blue);
            }
            catch (Exception e)
            {
                MyAPIGateway.Utilities.ShowNotification("Exception in update", 16, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }

        void GetAmmoProperties()
        {
            //-----------------------------------------------------------------
            //Thanks digi <3
            var slim = block.SlimBlock; //.CubeGrid.GetCubeBlock(block.Position);
            var definition = slim.BlockDefinition;
            var weapon = (MyWeaponBlockDefinition)definition;
            var wepDef = MyDefinitionManager.Static.GetWeaponDefinition(weapon.WeaponDefinitionId);

            for (int i = 0; i < wepDef.AmmoMagazinesId.Length; i++) //TODO: Make a dict of this data for different ammo types
            {
                var mag = MyDefinitionManager.Static.GetAmmoMagazineDefinition(wepDef.AmmoMagazinesId[i]);
                var ammo = MyDefinitionManager.Static.GetAmmoDefinition(mag.AmmoDefinitionId);

                _maxTrajectory = ammo.MaxTrajectory;
                _desiredSpeed = ammo.DesiredSpeed;
                _projectileDamage = ammo.GetDamageForMechanicalObjects() * 1e5f;
                _backkickForce = ammo.BackkickForce;

                var projectileAmmo = ammo as MyProjectileAmmoDefinition;
                _trailColor = projectileAmmo.ProjectileTrailColor;
                _trailScale = projectileAmmo.ProjectileTrailScale;
            }
            //-------------------------------------------

            //Compute reload ticks
            _reloadTime = wepDef.ReloadTime;
            _reloadTicks = (int)(_reloadTime / 1000f * 60f);
            _powerDrawDecrementPerTick = (_reloadPowerDraw - _idlePowerDrawBase) / _reloadTicks;
        }

        void GetTurretMaxRange()
        {
            //init turret power draw function constants
            if (Entity is IMyLargeTurretBase)
            {
                turret = Entity as IMyLargeTurretBase;
                var def = cube.SlimBlock.BlockDefinition as MyLargeTurretBaseDefinition;
                _turretMaxRange = def.MaxRangeMeters;
                var ob = (MyObjectBuilder_TurretBase)cube.GetObjectBuilderCubeBlock();
                ob.Range = _turretMaxRange;
                GetTurretPowerDrawConstants(_idlePowerDrawBase, _idlePowerDrawMax, _turretMaxRange);

                //_turretMaxRange = turret.GetMaximum<float>("Range");
                //this.m_shootingRange.SetLocalValue(Math.Min(this.BlockDefinition.MaxRangeMeters, Math.Max(0f, myObjectBuilder_TurretBase.Range)));
            }
        }

        void SetPowerSink()
        {
            sink = Entity.Components.Get<MyResourceSinkComponent>();
            sink.SetRequiredInputFuncByType(resourceId, () => GetPowerInput());
            MyResourceSinkInfo resourceInfo = new MyResourceSinkInfo()
            {
                ResourceTypeId = resourceId,
                MaxRequiredInput = turret == null ? _idlePowerDrawBase : _idlePowerDrawMax,
                RequiredInputFunc = () => GetPowerInput()
            };
            sink.RemoveType(ref resourceInfo.ResourceTypeId);
            sink.Init(MyStringHash.GetOrCompute("Defense"), resourceInfo); //sink.Init(MyStringHash.GetOrCompute("Defense"), turret == null ? _idlePowerDrawBase : _idlePowerDrawMax, () => GetPowerInput());
            sink.AddType(ref resourceInfo);
        }

        float GetPowerInput(bool count = true)
        {
            //if (block.Enabled == false)
            //return 0f;
            var s = Settings.GetSettings(Entity);

            if (!block.Enabled && (!s.Recharging || !_isReloading))
                return 0f;

            var requiredInput = turret != null ? CalculateTurretPowerDraw(turret.Range) : _idlePowerDrawBase;
            if (!_isReloading)
            {
                _enabledStatus = block.Enabled;
                sink.SetMaxRequiredInputByType(resourceId, requiredInput);
                return requiredInput;
            }

            _ticksSinceLastReload++;
            if (_ticksSinceLastReload > _reloadTicks)
                block.Enabled = false;
            else
                _enabledStatus = block.Enabled;

            if (!s.Recharging)
            {
                sink.SetMaxRequiredInputByType(resourceId, requiredInput);
                return requiredInput;
            }

            //if (block.IsWorking) //check if power is overloaded
            if (count && sink.IsPoweredByType(resourceId)) //SuppliedRatioByType(resourceId) == 1f)
                _currentReloadTicks++;

            if (_currentReloadTicks >= _reloadTicks)
            {
                _isReloading = false;
                _currentReloadTicks = 0;
                _ticksSinceLastReload = 0;
                block.Enabled = _enabledStatus; //reset to enabled after reloaded
                sink.SetMaxRequiredInputByType(resourceId, requiredInput);
                return requiredInput;
            }

            var scaledReloadPowerDraw = _reloadPowerDraw - _currentReloadTicks * _powerDrawDecrementPerTick;
            requiredInput = Math.Max(requiredInput, scaledReloadPowerDraw);
            sink.SetMaxRequiredInputByType(resourceId, requiredInput);
            return requiredInput;
        }

        DateTime GetLastShootTime()
        {
            if (cube == null)
                return new DateTime(0);

            var gun = Entity as IMyGunObject<MyGunBase>;
            return gun.GunBase.LastShootTime;

            /*
            if (Entity is IMyLargeTurretBase)
                return ((MyObjectBuilder_LargeGatlingTurret)cube.GetObjectBuilderCubeBlock()).GunBase.LastShootTime;
            else
                return ((MyObjectBuilder_SmallMissileLauncher)cube.GetObjectBuilderCubeBlock()).GunBase.LastShootTime;
            */
        }

        void ShowReloadMessage()
        {
            var s = Settings.GetSettings(Entity);
            if (_isReloading && s.Recharging)
            {
                IMyPlayer player = MyAPIGateway.Players.GetPlayerControllingEntity(block.CubeGrid);

                if (player == null)
                    return;

                MyVisualScriptLogicProvider.ShowNotification($"Railgun reloading ({100 * _currentReloadTicks / _reloadTicks}%)", 16, MyFontEnum.White, player.IdentityId);
            }
        }

        float _m = 0;
        float _b = 0;
        void GetTurretPowerDrawConstants(float start, float end, float maxRange)
        {
            _b = start;
            _m = (end - start) / (maxRange * maxRange * maxRange);
        }

        float CalculateTurretPowerDraw(float currentRange)
        {
            return _m * currentRange * currentRange * currentRange + _b;
        }
    }
}