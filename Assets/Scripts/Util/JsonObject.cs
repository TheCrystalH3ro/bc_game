using System.Collections.Generic;

namespace Assets.Scripts.Util
{
    public class JSONObject
    {
        private Dictionary<string, object> data = new Dictionary<string, object>();

        public void AddField(string key, object value)
        {
            if (!data.ContainsKey(key))
            {
                data.Add(key, value);
            }
            else
            {
                data[key] = value;
            }
        }

        public override string ToString()
        {
            string jsonString = "{";
            foreach (var pair in data)
            {
                jsonString += $"\"{pair.Key}\":";
                if (pair.Value is string)
                {
                    jsonString += $"\"{pair.Value}\",";
                }
                else
                {
                    jsonString += $"{pair.Value},";
                }
            }
            jsonString = jsonString.TrimEnd(',') + "}";
            return jsonString;
        }
    }
}
