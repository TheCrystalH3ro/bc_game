using System;
using Assets.Scripts.Requests;

namespace Assets.Scripts.Modules
{
    public class EnumSeederModule
    {
        private static EnumSeederModule _instance;

        public static EnumSeederModule Singleton
        { 
            get
            {
                _instance ??=  new();

                return _instance;
            }
        }

        private readonly string apiKey = ConfigModule.Get("SERVER_API_KEY");

        public void RegisterEnum(Type enumType, string endPoint)
        {
            Array enumValues = Enum.GetValues(enumType);

            EnumRequest requestBody = new(enumValues);

            RequestModule.Singleton.PostRequest(endPoint, apiKey, requestBody);
        }
    }
}