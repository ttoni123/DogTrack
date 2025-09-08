using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DogTrack.Models
{
    public class UserContext
    {
        [Newtonsoft.Json.JsonConstructor]
        public UserContext(string? application = null, string? version = null)
        {

            if (application != null)
            {
                _appName = application;
            }

            if (version != null)
            {
                _version = version;
            }

            if (_appName == null || _version == null)
            {
                try
                {
                    var assemblyName = Assembly.GetEntryAssembly()?.GetName();

                    _autoName = assemblyName?.Name;
                    _version = assemblyName?.Version?.ToString(3);
                }
                catch
                {
                    ;
                }
            }
        }

        private readonly string? _appName;
        private readonly string? _autoName;

        public string Application
        {
            get
            {
                return _appName ?? _autoName ?? "Unknown";
            }
        }

        private readonly string? _version;

        public string? Version
        {
            get
            {
                return _version;
            }
        }

        public int UserId
        {
            get; set;
        }


        #region serialization

        public string ToBase64()
        {
            return Convert.ToBase64String
            (
                Encoding.UTF8.GetBytes
                (
                    JsonConvert.SerializeObject(this, _jsonSetting)
                )
            );
        }

        public static UserContext? FromBase64(string base64String)
        {
            return JsonConvert.DeserializeObject<UserContext>
            (
                Encoding.UTF8.GetString
                (
                    Convert.FromBase64String(base64String)
                ),
                _jsonSetting
            );
        }

        private static readonly JsonSerializerSettings _jsonSetting = new() { TypeNameHandling = TypeNameHandling.All };

        #endregion serialization
    }
}