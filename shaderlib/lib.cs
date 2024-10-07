using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace shaderlib
{
    public static class shaderloader 
    {
        private static List<AssetBundle> bundles;
        private static List<Shader> shaders;
        private static List<Material> materials;

        public static AssetBundle loadBundle(string filepath)
        {
            try
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(filepath);
                bundles.Add(bundle);
                return bundle;
            }
            catch (Exception e) {
                Debug.Log("ShaderLib has prevented a crash: "+e.Message);
                return null;
            }
        }
        public static Shader GetShaderFromBundle(AssetBundle bundle, string shaderName)
        {
            if (bundle != null)
            {
                Shader _shader;
                Material _material;
                _shader = bundle.LoadAsset<Shader>(shaderName);

                if (_shader != null)
                {
                    _material = new Material(_shader);
                    Debug.Log($"Shaderlib Loaded {shaderName}");
                }
                else
                {
                    Debug.Log($"{shaderName} not found in AssetBundle!");
                }
            }


            return null;
        }


    }
}
