using System;
using AcTools.Render.Base;
using AcTools.Render.Base.Cameras;
using AcTools.Render.Base.Objects;
using AcTools.Render.Base.Shadows;
using AcTools.Render.Shaders;
using AcTools.Utils.Helpers;
using Newtonsoft.Json.Linq;
using SlimDX;
using SlimDX.Direct3D11;

namespace AcTools.Render.Kn5SpecificForwardDark.Lights {
    public class DarkAreaSphereLight : DarkLightBase {
        public DarkAreaSphereLight() : base(DarkLightType.Sphere) {
            ShadowsAvailable = false;
            HighQualityShadowsAvailable = false;
        }

        protected override DarkLightBase ChangeTypeOverride(DarkLightType newType) {
            switch (newType) {
                case DarkLightType.Point:
                    return new DarkPointLight {
                        Range = Range
                    };
                case DarkLightType.Directional:
                    return new DarkDirectionalLight();
                case DarkLightType.Spot:
                    return new DarkSpotLight {
                        Range = Range
                    };
                case DarkLightType.Sphere:
                    return new DarkAreaSphereLight {
                        Range = Range,
                        Radius = Radius
                    };
                case DarkLightType.Tube:
                    return new DarkAreaTubeLight {
                        Range = Range,
                        Radius = Radius
                    };
                case DarkLightType.Plane:
                    return new DarkAreaPlaneLight {
                        Range = Range,
                        Width = Radius,
                        Height = Radius,
                        DoubleSide = true,
                    };
                default:
                    return base.ChangeTypeOverride(newType);
            }
        }

        protected override void SerializeOverride(JObject obj) {
            base.SerializeOverride(obj);
            obj["range"] = Range;
            obj["radius"] = Radius;
        }

        protected override void DeserializeOverride(JObject obj) {
            base.DeserializeOverride(obj);
            Range = obj["range"] != null ? (float)obj["range"] : 2f;
            Radius = obj["radius"] != null ? (float)obj["radius"] : 0.2f;
        }

        private float _range = 2f;

        public float Range {
            get { return _range; }
            set {
                if (value.Equals(_range)) return;
                _range = value;
                InvalidateShadows();
                OnPropertyChanged();
            }
        }

        private float _radius = 0.2f;

        public float Radius {
            get { return _radius; }
            set {
                if (Equals(value, _radius)) return;
                _radius = value;
                DisposeHelper.Dispose(ref _dummy);
                OnPropertyChanged();
            }
        }

        protected override void UpdateShadowsOverride(DeviceContextHolder holder, Vector3 shadowsPosition, IShadowsDraw shadowsDraw) {}

        protected override void SetShadowOverride(out Vector4 size, out Matrix matrix, out ShaderResourceView view, ref Vector4 nearFar) {
            size = default(Vector4);
            matrix = Matrix.Identity;
            view = null;
        }

        protected override void SetOverride(IDeviceContextHolder holder, ref EffectDarkMaterial.Light light) {
            base.SetOverride(holder, ref light);
            light.Range = Range;
            light.SpotlightCosMin = Radius;
            light.Type = EffectDarkMaterial.LightSphere;
        }

        public override void InvalidateShadows() {}

        protected override void DisposeOverride() {
            DisposeHelper.Dispose(ref _dummy);
        }

        public override void Rotate(Quaternion delta) {}

        protected override MoveableHelper CreateMoveableHelper() {
            return new MoveableHelper(this, MoveableRotationAxis.None);
        }

        private RenderableList _dummy;

        public override void DrawDummy(IDeviceContextHolder holder, ICamera camera) {
            if (_dummy == null) {
                _dummy = new RenderableList {
                    DebugLinesObject.GetLinesCircle(Matrix.Identity, Vector3.UnitY, new Color4(1f, 1f, 1f, 0f), 20, Radius),
                    DebugLinesObject.GetLinesCircle(Matrix.Identity, Vector3.UnitX, new Color4(1f, 1f, 1f, 0f), 20, Radius),
                    DebugLinesObject.GetLinesCircle(Matrix.Identity, Vector3.UnitZ, new Color4(1f, 1f, 1f, 0f), 20, Radius),
                };
            }

            _dummy.ParentMatrix = Matrix.Translation(ActualPosition);
            _dummy.Draw(holder, camera, SpecialRenderMode.Simple);
        }
    }
}