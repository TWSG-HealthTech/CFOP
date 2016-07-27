using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CFOP.Speech
{
    public class IntentResponse
    {
        public string Query { get; set; }
        public IList<Intent> Intents { get; set; }
        public IList<Entity> Entities { get; set; }

        public IntentResponse()
        {
            Intents = new List<Intent>();
            Entities = new List<Entity>();
        }

        public class Intent
        {
            [JsonProperty("intent")]
            public string Name { get; set; }

            public decimal Score { get; set; }
            public IList<Action> Actions { get; set; }

            public bool IsFirstActionTriggered() => Actions.Any() && Actions.First().Triggered;

            public class Action
            {
                public bool Triggered { get; set; }
                public string Name { get; set; }
                public IList<Parameter> Parameters { get; set; }

                public class Parameter
                {
                    public string Name { get; set; }
                    public bool Required { get; set; }
                    [JsonProperty("value")]
                    public IList<Value> Values { get; set; }

                    public class Value
                    {
                        public string Entity { get; set; }
                        public string Type { get; set; }
                        public IDictionary<string, string> Resolution { get; set; }

                        public string GetResolution(string key) => Resolution[key];
                    }
                }
            }
        }

        public class Entity
        {
            
        }
    }
}
