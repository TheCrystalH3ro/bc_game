using UnityEngine;

namespace Assets.Scripts.Responses
{
    [System.Serializable]
    public class ApiError
    {
        public string field;
        public string message;

        public ApiError(string field, string message) {
            this.field = field;
            this.message = message;
        }
    }

    [System.Serializable]
    public class ApiErrors
    {
        public ApiError[] errors;

        public static ApiErrors CreateFromJSON(string jsonString) {
            return JsonUtility.FromJson<ApiErrors>(jsonString);
        }

        public ApiErrors(string error) {
            this.errors = new ApiError[1];
            this.errors[0] = new ApiError("", error);
        }
    }
}
