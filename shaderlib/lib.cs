using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace shaderlib
{
    public class CameraShader
    {
        public Shader Shader { get; private set; }
        public Material Material { get; private set; }

        // Constructor for CameraShader
        public CameraShader(Shader shader, Material material)
        {
            Shader = shader;
            Material = material;
        }
    }


    public static class shaderloader
    {
        private static List<AssetBundle> bundles = new List<AssetBundle>();
        private static List<Shader> shaders = new List<Shader>();
        private static List<Material> materials = new List<Material>();
        
        private static List<CameraShader> camera_shaders = new List<CameraShader>();

        public static AssetBundle LoadBundle(string filepath)
        {
            try
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(filepath);
                if (bundle != null)
                {
                    bundles.Add(bundle);
                }
                else
                {
                    Debug.Log($"Failed to load AssetBundle from {filepath}");
                }
                return bundle;
            }
            catch (Exception e)
            {
                Debug.Log("ShaderLib has prevented a crash: " + e.Message);
                return null;
            }
        }

        public static Shader GetShaderFromBundle(AssetBundle bundle, string shaderName)
        {
            if (bundle != null)
            {
                Shader _shader = bundle.LoadAsset<Shader>(shaderName);

                if (_shader != null)
                {
                    Material _material = new Material(_shader);
                    shaders.Add(_shader);
                    materials.Add(_material);
                    Debug.Log($"Shaderlib Loaded {shaderName}");
                    return _shader; // Return the loaded shader
                }
                else
                {
                    Debug.Log($"{shaderName} not found in AssetBundle!");
                }
            }
            return null;
        }

        public static Material MaterialFromShader(Shader shader)
        {
            Material _material = new Material(shader);
            return _material;
        }


        public static CameraShader CreateCameraShader(Shader shader, Material material)
        {
            return new CameraShader(shader, material);
        }


        public class CameraEffect : MonoBehaviour
        {
            private void OnRenderImage(RenderTexture source, RenderTexture destination)
            {
                Graphics.Blit(source, destination);
                foreach (CameraShader cs in camera_shaders)
                {
                    Graphics.Blit(source, destination, cs.Material);
                }
            }
        }
    }

    public class AddMaterialToGameObject : MonoBehaviour
    {
        public Material newMaterial; // The material to mix with the existing one

        private void Start()
        {
            // Get the MeshRenderer component of the GameObject
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            if (meshRenderer != null)
            {
                // Get the existing materials
                Material[] existingMaterials = meshRenderer.materials;
                Material combinedMaterial;

                // Create a new material instance to combine with the existing material
                if (existingMaterials.Length > 0)
                {
                    combinedMaterial = new Material(existingMaterials[0]); // Use the first existing material
                }
                else
                {
                    combinedMaterial = new Material(Shader.Find("Standard")); // Fallback to a default material
                }

                // Mix properties from the new material into the combined material
                MixMaterials(combinedMaterial, newMaterial);

                // Create a new material array to include the combined material
                Material[] newMaterials = new Material[existingMaterials.Length + 1];
                existingMaterials.CopyTo(newMaterials, 0);
                newMaterials[newMaterials.Length - 1] = combinedMaterial;

                // Assign the new materials array back to the renderer
                meshRenderer.materials = newMaterials;
            }
            else
            {
                Debug.LogWarning("MeshRenderer component not found on this GameObject.");
            }
        }

        private void MixMaterials(Material baseMaterial, Material overlayMaterial)
        {
            // Mix colors (assuming both materials have color properties)
            baseMaterial.color = Color.Lerp(baseMaterial.color, overlayMaterial.color, 0.5f); // 50% blend

            // Mix other properties as needed; for example, if both materials have texture
            if (baseMaterial.mainTexture != null && overlayMaterial.mainTexture != null)
            {
                baseMaterial.mainTexture = overlayMaterial.mainTexture; // Use the overlay's texture
            }

            // You can add more mixing logic for different properties (e.g., metallic, smoothness)
            // Adjust metallic and smoothness if needed
            if (baseMaterial.HasProperty("_Metallic") && overlayMaterial.HasProperty("_Metallic"))
            {
                float mixedMetallic = Mathf.Lerp(baseMaterial.GetFloat("_Metallic"), overlayMaterial.GetFloat("_Metallic"), 0.5f);
                baseMaterial.SetFloat("_Metallic", mixedMetallic);
            }

            if (baseMaterial.HasProperty("_Glossiness") && overlayMaterial.HasProperty("_Glossiness"))
            {
                float mixedGlossiness = Mathf.Lerp(baseMaterial.GetFloat("_Glossiness"), overlayMaterial.GetFloat("_Glossiness"), 0.5f);
                baseMaterial.SetFloat("_Glossiness", mixedGlossiness);
            }
        }
    }
}
