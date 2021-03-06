using AcTools.Kn5File;
using AcTools.Render.Base;
using AcTools.Render.Base.Objects;
using AcTools.Render.Kn5Specific.Materials;
using JetBrains.Annotations;

namespace AcTools.Render.Kn5SpecificForward.Materials {
    public class Kn5MaterialSimpleGl : Kn5MaterialSimpleBase {
        public Kn5MaterialSimpleGl([NotNull] Kn5MaterialDescription description) : base(description) { }

        internal Kn5MaterialSimpleGl() : base(new Kn5MaterialDescription(new Kn5Material {
            DepthMode = Kn5MaterialDepthMode.DepthOff
        })) { }

        public override void Draw(IDeviceContextHolder contextHolder, int indices, SpecialRenderMode mode) {
            Effect.TechGl.DrawAllPasses(contextHolder.DeviceContext, indices);
        }
    }
}