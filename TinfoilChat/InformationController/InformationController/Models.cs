using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace InformationController
{
    public abstract class Model
    {
        public int Id { get; set; }

        public string ModelType { get; set; }

        public Model()
        {
            this.ModelType = this.ModelName();
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static T Deserialize<T>(string value) where T : Model
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        public abstract string ModelName();
    }

    public class ConnectionModel : Model
    {
        public string IPAddress { get; set; }

        public override string ModelName()
        {
            return "Connection";
        }
    }

    public class FriendModel : Model
    {
        public bool Verified { get; set; }

        public string[] Aliases { get; set; }

        public ConnectionModel[] KnownConnections { get; set; }

        public override string ModelName()
        {
            return "Friend";
        }
    }

    public class SelfModel : Model
    {
        public string Alias { get; set; }

        public override string ModelName()
        {
            return "Self";
        }
    }
}
