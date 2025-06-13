using System;
using System.Collections.Generic;

namespace Assets.Scripts.Requests
{
    [Serializable]
    public class EnumInfo
    {
        public int id;
        public string name;

        public EnumInfo(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }

    [Serializable]
    public class EnumRequest : BaseRequest
    {
        public List<EnumInfo> data;

        public EnumRequest(Array values)
        {
            data = new();

            foreach (var enumValue in values)
            {
                data.Add(new((int) enumValue, enumValue.ToString()));
            }
        }
    }
}