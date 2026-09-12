using DeskQuest.Common;
using UnityEngine;

namespace DeskQuest.Character
{
    /// <summary>
    /// Builds a stick man out of Unity's primitive meshes.
    ///
    /// Everything is proportional to one height parameter, so the same code produces a
    /// nine-centimetre character for a desk or a taller one for a different surface later.
    /// Colliders are stripped: the motor does its own collision against obstacle boxes and
    /// never consults Unity physics, so physics colliders here would be cost with no
    /// benefit.
    ///
    /// This is a factory rather than a prefab because a prefab is a binary asset that has
    /// to survive being copied between machines and Unity versions. A prefab can be baked
    /// from the result later if one is wanted; nothing depends on it being one.
    /// </summary>
    public static class StickManFactory
    {
        // Proportions as fractions of total height. Roughly human, with the head enlarged
        // the way a toy figure's is, because at nine centimetres a realistic head is a
        // couple of pixels on screen.
        const float k_HeadRadius = 0.115f;
        const float k_LegLength = 0.42f;
        const float k_TorsoLength = 0.30f;
        const float k_ArmLength = 0.34f;
        const float k_LimbThickness = 0.05f;
        const float k_TorsoThickness = 0.085f;
        const float k_ShoulderOffset = 0.085f;
        const float k_HipOffset = 0.06f;

        /// <summary>
        /// Creates the character under <paramref name="parent"/> and returns its rig.
        /// The origin sits at the feet, so placing it is a matter of setting the position
        /// to a point on the desk.
        /// </summary>
        public static StickManRig Create(Transform parent, float height, Color bodyColor, Color headColor)
        {
            GameObject root = new GameObject("StickMan");
            root.transform.SetParent(parent, false);

            StickManRig rig = root.AddComponent<StickManRig>();
            rig.Height = height;

            Material bodyMaterial = MaterialLibrary.CreateOpaque(bodyColor);
            Material headMaterial = MaterialLibrary.CreateOpaque(headColor);

            float legLength = height * k_LegLength;
            float torsoLength = height * k_TorsoLength;
            float armLength = height * k_ArmLength;
            float limbThickness = height * k_LimbThickness;
            float headRadius = height * k_HeadRadius;

            rig.RestPelvisHeight = legLength;

            // Pelvis sits at the top of the legs. Everything above hangs off the spine,
            // everything below off the hips, so crouching is one pelvis move plus knee
            // and hip rotations.
            rig.Pelvis = NewJoint("Pelvis", root.transform, new Vector3(0f, legLength, 0f));

            rig.Spine = NewJoint("Spine", rig.Pelvis, Vector3.zero);
            AddLimbMesh(rig.Spine, torsoLength, height * k_TorsoThickness, bodyMaterial, up: true);

            rig.Head = NewJoint("Head", rig.Spine, new Vector3(0f, torsoLength + headRadius * 0.8f, 0f));
            AddSphereMesh(rig.Head, headRadius * 2f, headMaterial);

            float shoulderY = torsoLength;
            float shoulderX = height * k_ShoulderOffset;
            float upperArm = armLength * 0.5f;

            rig.ShoulderLeft = NewJoint("ShoulderLeft", rig.Spine, new Vector3(-shoulderX, shoulderY, 0f));
            AddLimbMesh(rig.ShoulderLeft, upperArm, limbThickness, bodyMaterial, up: false);
            rig.ElbowLeft = NewJoint("ElbowLeft", rig.ShoulderLeft, new Vector3(0f, -upperArm, 0f));
            AddLimbMesh(rig.ElbowLeft, upperArm, limbThickness, bodyMaterial, up: false);

            rig.ShoulderRight = NewJoint("ShoulderRight", rig.Spine, new Vector3(shoulderX, shoulderY, 0f));
            AddLimbMesh(rig.ShoulderRight, upperArm, limbThickness, bodyMaterial, up: false);
            rig.ElbowRight = NewJoint("ElbowRight", rig.ShoulderRight, new Vector3(0f, -upperArm, 0f));
            AddLimbMesh(rig.ElbowRight, upperArm, limbThickness, bodyMaterial, up: false);

            float hipX = height * k_HipOffset;
            float thigh = legLength * 0.5f;

            rig.HipLeft = NewJoint("HipLeft", rig.Pelvis, new Vector3(-hipX, 0f, 0f));
            AddLimbMesh(rig.HipLeft, thigh, limbThickness, bodyMaterial, up: false);
            rig.KneeLeft = NewJoint("KneeLeft", rig.HipLeft, new Vector3(0f, -thigh, 0f));
            AddLimbMesh(rig.KneeLeft, thigh, limbThickness, bodyMaterial, up: false);

            rig.HipRight = NewJoint("HipRight", rig.Pelvis, new Vector3(hipX, 0f, 0f));
            AddLimbMesh(rig.HipRight, thigh, limbThickness, bodyMaterial, up: false);
            rig.KneeRight = NewJoint("KneeRight", rig.HipRight, new Vector3(0f, -thigh, 0f));
            AddLimbMesh(rig.KneeRight, thigh, limbThickness, bodyMaterial, up: false);

            return rig;
        }

        static Transform NewJoint(string name, Transform parent, Vector3 localPosition)
        {
            GameObject joint = new GameObject(name);
            joint.transform.SetParent(parent, false);
            joint.transform.localPosition = localPosition;
            return joint.transform;
        }

        /// <summary>
        /// Adds a cylinder to a joint, running either up from it or down from it.
        /// Unity's cylinder primitive is two units tall and one across, hence the halved
        /// vertical scale and the half-length offset.
        /// </summary>
        static void AddLimbMesh(Transform joint, float length, float thickness, Material material, bool up)
        {
            GameObject limb = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            limb.name = "Mesh";
            limb.transform.SetParent(joint, false);
            limb.transform.localPosition = new Vector3(0f, up ? length * 0.5f : -length * 0.5f, 0f);
            limb.transform.localScale = new Vector3(thickness, length * 0.5f, thickness);
            Finish(limb, material);
        }

        static void AddSphereMesh(Transform joint, float diameter, Material material)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Mesh";
            sphere.transform.SetParent(joint, false);
            sphere.transform.localScale = new Vector3(diameter, diameter, diameter);
            Finish(sphere, material);
        }

        static void Finish(GameObject part, Material material)
        {
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
                Object.Destroy(collider);

            MeshRenderer renderer = part.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }
    }
}
