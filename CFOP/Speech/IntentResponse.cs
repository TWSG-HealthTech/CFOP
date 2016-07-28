using System.Collections.Concurrent;
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

            public Intent()
            {
                Actions = new List<Action>();
            }

            public bool IsActionTriggered(string name)
            {
                var action = GetAction(name);
                return action != null && action.Triggered;
            }

            public Action GetAction(string name)
            {
                return Actions.FirstOrDefault(a => a.Name == name);
            }

            public class Action
            {
                public bool Triggered { get; set; }
                public string Name { get; set; }
                public IList<Parameter> Parameters { get; set; }

                public Action()
                {
                    Parameters = new List<Parameter>();
                }

                public Parameter GetParameter(string name)
                {
                    return Parameters.FirstOrDefault(p => p.Name == name);
                }

                public class Parameter
                {
                    public string Name { get; set; }
                    public bool Required { get; set; }
                    [JsonProperty("value")]
                    public IList<Value> Values { get; set; }

                    public Parameter()
                    {
                        Values = new List<Value>();
                    }

                    public Value GetValue(string entity)
                    {
                        return Values.FirstOrDefault(v => v.Entity == entity);
                    }

                    public class Value
                    {
                        public string Entity { get; set; }
                        public string Type { get; set; }
                        public IDictionary<string, string> Resolution { get; set; }

                        public string GetResolution(string key) => Resolution[key];

                        public Value()
                        {
                            Resolution = new Dictionary<string, string>();
                        }
                    }
                }
            }
        }

        public class Entity
        {
            [JsonProperty("entity")]
            public string Name { get; set; }
            public string Type { get; set; }
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }
            public IDictionary<string, string> Resolution { get; set; }

            public Entity()
            {
                Resolution = new Dictionary<string, string>();
            }
        }
    }
}
