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
using Whiplash.Railgun;

namespace Whiplash.ArmorPiercingProjectiles
{
    public class ArmorPiercingProjectileSimulation
    {
        Vector3D _origin;
        Vector3D _position;
        Vector3D _velocityCombined;
        Vector3D _direction;
        Vector3D _hitPosition;
        float _explosionDamage;
        float _explosionRadius;
        float _projectileSpeed;
        float _maxTrajectory;
        float _minimumArmDistance = 0f;
        float _penetrationRange;
        float _penetrationDamage;
        int _checkIntersectionIndex = 4;
        const float tick = 1f / 60f;
        public bool Killed = false;
        bool _positionChecked = false;
        bool _shouldExplode;
        bool _shouldPenetrate;
        bool _drawTracer;
        bool _targetHit = false;
        int _currentTick = 0;
        const int _maxTracerFadeTicks = 120;
        int _currentTracerFadeTicks = 0;
        List<IHitInfo> hitInfo = new List<IHitInfo>();
        long _gunEntityID;
        Vector4 _lineColor; // = new Color(0, 128, 255).ToVector4();
        MyStringId material = MyStringId.GetOrCompute("WeaponLaser");
        MyStringId bulletMaterial = MyStringId.GetOrCompute("ProjectileTrailLine");
        Vector3 _tracerColor;
        float _tracerScale;

        public ArmorPiercingProjectileSimulation(Vector3D origin, Vector3D direction, Vector3D shooterVelocity, float projectileSpeed, float maxTrajectory, float explosionDamage, float explosionRadius, float penetrationDamage, float penetrationRange, long gunEntityID, Vector3? tracerColor = null, float tracerScale = 0, bool drawTracer = false, bool explode = true, bool penetrate = true)
        {
            _tracerColor = tracerColor.HasValue ? tracerColor.Value : Vector3.Zero;
            _lineColor = new Vector4(_tracerColor, 1f);
            _tracerScale = tracerScale;
            _drawTracer = drawTracer;
            _shouldPenetrate = penetrate;
            _shouldExplode = explode;
            _penetrationDamage = penetrationDamage;
            _penetrationRange = penetrationRange;
            _gunEntityID = gunEntityID;
            _direction = Vector3D.IsUnit(ref direction) ? direction : Vector3D.Normalize(direction);
            _origin = origin;
            _position = _origin;
            _explosionRadius = explosionRadius;
            _explosionDamage = explosionDamage;
            _maxTrajectory = maxTrajectory;
            _projectileSpeed = projectileSpeed;
            _velocityCombined = shooterVelocity + _direction * _projectileSpeed;
        }

        public ArmorPiercingProjectileSimulation(RailgunFireData fireData, RailgunProjectileData projectileData)
        {
            //weapon data
            _tracerColor = projectileData.ProjectileTrailColor;
            _lineColor = new Vector4(_tracerColor, 1f);
            _tracerScale = projectileData.ProjectileTrailScale;
            _drawTracer = projectileData.DrawTracer;
            _shouldPenetrate = projectileData.Penetrate;
            _shouldExplode = projectileData.Explode;
            _penetrationDamage = projectileData.PenetrationDamage;
            _penetrationRange = projectileData.PenetrationRange;
            _explosionRadius = projectileData.ExplosionRadius;
            _explosionDamage = projectileData.ExplosionDamage;
            _maxTrajectory = projectileData.MaxTrajectory;
            _projectileSpeed = projectileData.DesiredSpeed;

            //fire data
            var temp = fireData.Direction;
            _direction = Vector3D.IsUnit(ref temp) ? temp : Vector3D.Normalize(temp);
            _origin = fireData.Origin;
            _position = _origin;
            _gunEntityID = 0;
            _velocityCombined = fireData.ShooterVelocity + _direction * _projectileSpeed;
        }

        public void Update(bool isServer)
        {
            if (_targetHit)
            {
                Kill();
                return;
            }

            _position += _velocityCombined * tick;
            var _toOrigin = _position - _origin;

            //draw tracer line
            if (_drawTracer && _currentTracerFadeTicks < _maxTracerFadeTicks)
            {
                _lineColor *= 0.95f;
                _currentTracerFadeTicks++;
            }

            if (_toOrigin.LengthSquared() > _maxTrajectory * _maxTrajectory)
            {
                _targetHit = true;
                _hitPosition = _position;
                Kill();
                if (_shouldExplode)
                    CreateExplosion(_position, _direction, _explosionRadius, _explosionDamage);
                //MyAPIGateway.Utilities.ShowNotification("Projectile killed", 2000, MyFontEnum.White);
                return;
            }

            _checkIntersectionIndex = ++_checkIntersectionIndex % 5;
            if (_checkIntersectionIndex != 0 && _positionChecked)
            {
                return;
            }

            var to = _position + 5.0 * _velocityCombined * tick;
            var from = _positionChecked ? _position : _origin;
            _positionChecked = true;

            MyAPIGateway.Physics.CastRay(from, to, hitInfo);

            if (hitInfo.Count > 0)
            {
                _hitPosition = hitInfo[0].Position + -0.5 * _direction;

                //MyAPIGateway.Utilities.ShowNotification("Target hit", 2000, MyFontEnum.White);
                //MyAPIGateway.Utilities.ShowNotification($"hit count: {hitInfo.Count}", 2000, MyFontEnum.Blue);
                if ((_hitPosition - _origin).LengthSquared() > _minimumArmDistance * _minimumArmDistance) //only explode if beyond arm distance
                {
                    if (_shouldExplode && isServer)
                        CreateExplosion(_hitPosition, _direction, _explosionRadius, _explosionDamage);

                    if (_shouldPenetrate /* && isServer*/) //to fight desync
                        CreatePenetrationDamage(_hitPosition, _hitPosition + _direction * _penetrationRange, _penetrationDamage);

                    _targetHit = true;

                    Kill();
                }
                else
                {
                    _targetHit = true;
                    _hitPosition = _position;
                    Kill();
                }
                return;
            }

            //MyAPIGateway.Utilities.ShowNotification("Projectile live", 80, MyFontEnum.White);
        }

        void CreateExplosion(Vector3D position, Vector3D direction, float radius, float damage, float scale = 1f)
        {
            var m_explosionFullSphere = new BoundingSphere(position, radius);

            MyExplosionInfo info = new MyExplosionInfo()
            {
                PlayerDamage = 100,
                Damage = damage,
                ExplosionType = MyExplosionTypeEnum.WARHEAD_EXPLOSION_02,
                ExplosionSphere = m_explosionFullSphere,
                LifespanMiliseconds = MyExplosionsConstants.EXPLOSION_LIFESPAN,
                ParticleScale = scale,
                Direction = direction,
                VoxelExplosionCenter = m_explosionFullSphere.Center,
                ExplosionFlags = MyExplosionFlags.AFFECT_VOXELS | MyExplosionFlags.CREATE_PARTICLE_EFFECT | MyExplosionFlags.APPLY_DEFORMATION,
                VoxelCutoutScale = 0.1f,
                PlaySound = true,
                ApplyForceAndDamage = false, //to stop from flinging objects to orbit
                ObjectsRemoveDelayInMiliseconds = 40
            };

            MyExplosions.AddExplosion(ref info);
        }

        List<MyLineSegmentOverlapResult<MyEntity>> overlappingEntities = new List<MyLineSegmentOverlapResult<MyEntity>>();
        List<Vector3I> hitPositions = new List<Vector3I>();

        void CreatePenetrationDamage(Vector3D start, Vector3D end, float damage)
        {
            var testRay = new LineD(start, end);
            MyGamePruningStructure.GetAllEntitiesInRay(ref testRay, overlappingEntities);
            //MyAPIGateway.Utilities.ShowNotification($"Entities penetrated: {overlappingEntities.Count}", 3000, MyFontEnum.Green);

            MyLog.Default.WriteLine(">>>>>>>>>> Railgun Penetration Start <<<<<<<<<<");
            MyLog.Default.WriteLine($"Railgun initial pooled damage: {damage}"); ;

            foreach (var hit in overlappingEntities)
            {
                MyLog.Default.WriteLine("----------------Railgun hit entity: " + $"{hit.Element.GetType()}");

                if (damage <= 0)
                {
                    MyLog.Default.WriteLine("-------------Pooled damage expended");
                    break;
                }

                //IMyCubeGrid grid = hit.Element as IMyCubeGrid;
                if (hit.Element is IMyCubeGrid)
                {
                    var grid = (IMyCubeGrid)hit.Element;

                    MyLog.Default.WriteLine($"--------------Grid found");

                    IMySlimBlock slimBlock;

                    grid.RayCastCells(start, end, hitPositions);
                    var localStart = Vector3D.Transform(start, grid.WorldMatrixNormalizedInv);
                    localStart /= grid.GridSize;

                    hitPositions.Sort((x, y) => Vector3D.DistanceSquared(localStart, x).CompareTo(Vector3D.DistanceSquared(localStart, y)));

                    if (hitPositions.Count == 0)
                    {
                        MyLog.Default.WriteLine("-----No slim block found in intersection");
                        continue;
                    }

                    MyLog.Default.WriteLine($"-----{hitPositions.Count} slim blocks in intersection");

                    foreach (var position in hitPositions)
                    {
                        MyLog.Default.WriteLine("-----");

                        slimBlock = grid.GetCubeBlock(position);
                        MyLog.Default.WriteLine($"dist: {Vector3D.DistanceSquared(position, localStart)}");

                        if (slimBlock == null)
                        {
                            MyLog.Default.WriteLine($">> slim is null at");
                            MyLog.Default.WriteLine($"position: {position}");
                            continue;
                        }

                        var blockIntegrity = slimBlock.Integrity;
                        var cube = slimBlock.FatBlock;
                        MyLog.Default.WriteLine($"cube type: {(cube == null ? "null" : cube.GetType().ToString())}");
                        MyLog.Default.WriteLine($"pooled damage before: {damage}");
                        MyLog.Default.WriteLine($"block integrity before: {blockIntegrity}");

                        var invDamageMultiplier = 1f;
                        var cubeDef = slimBlock.BlockDefinition as MyCubeBlockDefinition;
                        if (cubeDef != null)
                        {
                            MyLog.Default.WriteLine($"block damage mult: {cubeDef.GeneralDamageMultiplier}");
                            invDamageMultiplier = 1f / cubeDef.GeneralDamageMultiplier;
                        }

                        try
                        {
                            if (damage > blockIntegrity)
                                slimBlock.DoDamage(blockIntegrity * invDamageMultiplier, MyStringHash.GetOrCompute("Railgun"), false, default(MyHitInfo), _gunEntityID); //because some blocks have a stupid damage intake modifier
                            else
                                slimBlock.DoDamage(invDamageMultiplier * damage, MyStringHash.GetOrCompute("Railgun"), false, default(MyHitInfo), _gunEntityID);
                        }
                        catch (Exception ex)
                        {
                            MyLog.Default.WriteLine(ex);
                        }

                        if (damage < blockIntegrity)
                        {
                            damage = 0;
                            MyLog.Default.WriteLine($"pooled damage after: {damage}");
                            MyLog.Default.WriteLine($"block integrity after: {slimBlock.Integrity}");
                            break;
                        }
                        else
                            damage -= blockIntegrity;

                        MyLog.Default.WriteLine($"pooled damage after: {damage}");
                        MyLog.Default.WriteLine($"block integrity after: {slimBlock.Integrity}");
                    }
                }
                else if (hit.Element is IMyDestroyableObject)
                {
                    var destroyableEntity = (IMyDestroyableObject)hit.Element;

                    MyLog.Default.WriteLine($"----------------------------Destroyable entity found");
                    var cachedIntegrity = destroyableEntity.Integrity;

                    destroyableEntity.DoDamage(damage, MyStringHash.GetOrCompute("Railgun"), false, default(MyHitInfo), _gunEntityID);
                    if (damage < cachedIntegrity)
                        damage = 0;
                    else
                        damage -= cachedIntegrity;

                    //continue;
                }
            }
            MyLog.Default.WriteLine("<<<<<<<<<< Railgun Penetration End >>>>>>>>>>");
        }

        void Kill()
        {
            if (_drawTracer && _currentTracerFadeTicks < _maxTracerFadeTicks)
            {
                _lineColor *= 0.95f;
                _currentTracerFadeTicks++;
                return;
            }

            Killed = true;
        }

        public void DrawTracer()
        {
            //draw bullet
            float scaleFactor = MyParticlesManager.Paused ? 1f : MyUtils.GetRandomFloat(1f, 2f);
            float lengthMultiplier = 40f * _tracerScale;
            lengthMultiplier *= MyParticlesManager.Paused ? 0.6f : MyUtils.GetRandomFloat(0.6f, 0.8f);
            var startPoint = _position - _direction * lengthMultiplier;
            float thickness = (MyParticlesManager.Paused ? 0.2f : MyUtils.GetRandomFloat(0.2f, 0.3f)) * _tracerScale;
            thickness *= MathHelper.Lerp(0.2f, 0.8f, 1f/*MySector.MainCamera.Zoom.GetZoomLevel()*/);

            if (lengthMultiplier > 0f && !_targetHit && Vector3D.DistanceSquared(_position, _origin) > lengthMultiplier * lengthMultiplier)
                MyTransparentGeometry.AddLineBillboard(bulletMaterial, new Vector4(_tracerColor * scaleFactor * 10f, 1f), startPoint, _direction, lengthMultiplier, thickness);

            if (_targetHit)
                MySimpleObjectDraw.DrawLine(_origin, _hitPosition, material, ref _lineColor, _tracerScale * 0.1f);
            else
                MySimpleObjectDraw.DrawLine(_origin, _position, material, ref _lineColor, _tracerScale * 0.1f);
        }
    }
}
