using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.VFX.Block
{
    [VFXInfo(category = "Orientation")]
    class Orient : VFXBlock
    {
        public enum Mode
        {
            FaceCameraPlane,
            FaceCameraPosition,
            LookAtPosition,
            FixedOrientation,
            FixedAxis,
            AlongVelocity,
        }

        [VFXSetting]
        public Mode mode;

        public override string name { get { return "Orient"; } }

        public override VFXContextType compatibleContexts   { get { return VFXContextType.kOutput; } }
        public override VFXDataType compatibleData          { get { return VFXDataType.kParticle; } }

        public override IEnumerable<VFXAttributeInfo> attributes
        {
            get
            {
                yield return new VFXAttributeInfo(VFXAttribute.Front, VFXAttributeMode.Write);
                yield return new VFXAttributeInfo(VFXAttribute.Side, VFXAttributeMode.Write);
                yield return new VFXAttributeInfo(VFXAttribute.Up, VFXAttributeMode.Write);
                if (mode != Mode.FixedOrientation && mode != Mode.FaceCameraPlane)
                    yield return new VFXAttributeInfo(VFXAttribute.Position, VFXAttributeMode.Read);
                if (mode == Mode.AlongVelocity)
                    yield return new VFXAttributeInfo(VFXAttribute.Velocity, VFXAttributeMode.Read);
            }
        }

        protected override IEnumerable<VFXPropertyWithValue> inputProperties
        {
            get
            {
                switch (mode)
                {
                    case Mode.LookAtPosition:
                        yield return new VFXPropertyWithValue(new VFXProperty(typeof(Position), "Position"));
                        break;

                    case Mode.FixedOrientation:
                        yield return new VFXPropertyWithValue(new VFXProperty(typeof(DirectionType), "Front"), new DirectionType() { direction = Vector3.forward });
                        yield return new VFXPropertyWithValue(new VFXProperty(typeof(DirectionType), "Up"), new DirectionType() { direction = Vector3.up });
                        break;

                    case Mode.FixedAxis:
                        yield return new VFXPropertyWithValue(new VFXProperty(typeof(DirectionType), "Up"), new DirectionType() { direction = Vector3.up });
                        break;
                }
            }
        }

        public override string source
        {
            get
            {
                switch (mode)
                {
                    case Mode.FaceCameraPlane:
                        return @"
float4x4 cameraMat = VFXCameraMatrix();
front = VFXCameraLook();
side = cameraMat[0].xyz;
up = cameraMat[1].xyz;
";

                    case Mode.FaceCameraPosition:
                        return @"
front = normalize(VFXCameraPos() - position);
side = normalize(cross(front,VFXCameraMatrix()[1].xyz));
up = cross(side,front);
";

                    case Mode.LookAtPosition:
                        return @"
front = normalize(Position_position - position);
side = normalize(cross(front,VFXCameraMatrix()[1].xyz));
up = cross(side,front);
";

                    case Mode.FixedOrientation:
                        return @"
front = Front;
side = normalize(cross(front,Up));
up = cross(side,front);
";

                    case Mode.FixedAxis:
                        return @"
up = Up;
front = VFXCameraPos() - position;
side = normalize(cross(front,up));
front = cross(up,side);
";

                    case Mode.AlongVelocity:
                        return @"
up = normalize(velocity);
front = VFXCameraPos() - position;
side = normalize(cross(front,up));
front = cross(up,side);
";

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
