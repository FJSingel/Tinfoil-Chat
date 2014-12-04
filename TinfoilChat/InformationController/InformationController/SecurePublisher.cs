using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace InformationController
{
    public class SecurePublisher
    {
        private static Dictionary<string, SecurePublisher> publishers = new Dictionary<string, SecurePublisher>();

        private static SecurePublisher GetInstance(string filename)
        {
            SecurePublisher publisher;
            if (publishers.TryGetValue(filename, out publisher))
            {
                return publisher;
            }
            else
            {
                publishers[filename] = publisher;
                return publisher;
            }
        }

        private string filename;

        private List<Model> modelsById;

        private Dictionary<string, List<Model>> modelsByType;

        private SecurePublisher(string filename)
        {
            this.filename = filename;
            this.modelsById = new List<Model>();
            this.modelsByType = new Dictionary<string, List<Model>>();
            if (File.Exists(this.filename))
            {
                string text = File.ReadAllText(this.filename);
                var map = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(text);
                foreach(var kvp in map)
                {
                    switch(kvp.Key)
                    {
                        case "Friend":
                            foreach(var friendModel in JsonConvert.DeserializeObject<List<FriendModel>>(kvp.Value.ToString()))
                            {
                                this.RegisterModel(friendModel);
                            }
                            break;
                        case "Self":
                            foreach (var selfModel in JsonConvert.DeserializeObject<List<SelfModel>>(kvp.Value.ToString()))
                            {
                                this.RegisterModel(selfModel);
                            }
                            break;
                        case "Connection":
                            foreach (var connectionModel in JsonConvert.DeserializeObject<List<ConnectionModel>>(kvp.Value.ToString()))
                            {
                                this.RegisterModel(connectionModel);
                            }
                            break;
                    }
                }
            }
        }

        private void RegisterModel(Model m)
        {
            this.modelsById.Add(m);
            List<Model> models;
            if (this.modelsByType.TryGetValue(m.ModelName(), out models))
            {
                models.Add(m);
            }
            else
            {
                models = new List<Model>();
                models.Add(m);
                this.modelsByType[m.ModelName()] = models;
            }
        }

        private int NextId()
        {
            return modelsById.Count;
        }

        public ModelType CreateModel<ModelType> () where ModelType : Model, new()
        {
            int id = NextId();
            ModelType model = new ModelType();
            model.Id = id;
            this.RegisterModel(model);
            return model;
        }

        public IEnumerable<ModelType> GetAllModels<ModelType> () where ModelType : Model, new()
        {
            ModelType prototype = new ModelType();
            IEnumerable<Model> untypedModels = this.modelsByType[prototype.ModelName()];
            List<ModelType> typedModels = new List<ModelType>(untypedModels.Count());
            foreach(Model m in untypedModels) {
                typedModels.Add((ModelType)m);
            }
            return typedModels;
        }

        public void PublishToFile()
        {
            File.WriteAllText(this.filename, JsonConvert.SerializeObject(this.modelsByType));
        }
    }
}
