using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewerWPF.PublicFunction
{
    public class SkeletonBinaryHeader
    {
        public string Hash { get; set; }
        public string Version { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public System.Version SpineVersion
        {
            get
            {
                return new System.Version(Version);
            }
        }


        public SkeletonBinaryHeader() { }

        public bool ReadFromBinary(Stream input)
        {
            try
            {
                Hash = input.ReadString();
                if (Hash.Length == 0) return false;
                Version = input.ReadString();
                if (Version.Length == 0) return false;
                if (SpineVersion.CompareTo(new Version("3.7")) > 0)
                {
                    X = input.ReadFloat();
                    Y = input.ReadFloat();
                }
                Width = input.ReadFloat();
                Height = input.ReadFloat();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool ReadFromJSON(string text)
        {
            try
            {
                Newtonsoft.Json.Linq.JObject jsonObj = Newtonsoft.Json.Linq.JObject.Parse(text);
                if (jsonObj["skeleton"] != null)
                {
                    Hash = jsonObj["skeleton"]["hash"].ToString();
                    Version = jsonObj["skeleton"]["spine"].ToString();
                    if (jsonObj["skeleton"]["x"] != null)
                        X = jsonObj["skeleton"]["x"].ToObject<float>();
                    if (jsonObj["skeleton"]["y"] != null)
                        Y = jsonObj["skeleton"]["y"].ToObject<float>();
                    if (jsonObj["skeleton"]["width"] != null)
                        Width = jsonObj["skeleton"]["width"].ToObject<float>();
                    if (jsonObj["skeleton"]["height"] != null)
                        Height = jsonObj["skeleton"]["height"].ToObject<float>();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
