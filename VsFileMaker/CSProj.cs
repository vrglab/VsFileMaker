using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsFileMaker
{
    public class CSProj : Serializable
    {
        public string pathFromRoot { get; private set; }

        public CSxmlEntery entery { get; set; }

        public CSProj(string pathFromRoot)
        {
            this.pathFromRoot = pathFromRoot;
        }

        public string Serialize()
        {
            return entery.Serialize();
        }
    }

    public class CSxmlEntery : Serializable
    {
        public string Name { get; private set; }
        public Dictionary<string, string> parameters { get; private set; } = new Dictionary<string, string>();
        public List<object> value { get; private set; }

        public CSxmlEntery(string name, Dictionary<string, string> parameters, object value)
        {
            Name = name;
            this.parameters = parameters;
            this.value = new List<object>();
            this.value.Add(value);
        }
        public CSxmlEntery(string name, Dictionary<string, string> parameters, List<object> value)
        {
            Name = name;
            this.parameters = parameters;
            this.value = value;
        }

        public CSxmlEntery(string name, object value)
        {
            Name = name;
            this.value = new List<object>();
            this.value.Add(value);
        }

        public CSxmlEntery(string name, List<object> value)
        {
            Name = name;
            this.value = new List<object>();
            this.value = value;
        }

        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"<{Name}");
            if(parameters.Count > 0)
            {
                foreach (var item in parameters)
                {
                    sb.Append($" {item.Key}=\"{item.Value}\"");
                }
            }
            sb.AppendLine($">");


            foreach (var item in value)
            {
                if (item is string)
                {
                    sb.AppendLine(item.ToString());
                }
                else if (item is Serializable)
                {
                    Serializable serializableOneWay = item as Serializable;
                    sb.AppendLine(serializableOneWay.Serialize());
                }
            }
            

            sb.AppendLine($"</{Name}>");
            return sb.ToString();
        }
    }
}
