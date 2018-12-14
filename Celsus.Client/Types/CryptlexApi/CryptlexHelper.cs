using Flurl;
using Flurl.Http;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celsus.Client.Types.CryptlexApi
{
    public class CryptlexHelper
    {
        private static readonly string bearer = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzY29wZSI6WyJhY2NvdW50OnJlYWQiLCJhY2NvdW50OndyaXRlIiwiYWN0aXZhdGlvbjpyZWFkIiwiYW5hbHl0aWNzOnJlYWQiLCJldmVudExvZzpyZWFkIiwiaW52b2ljZTpyZWFkIiwibGljZW5zZTpyZWFkIiwibGljZW5zZTp3cml0ZSIsImxpY2Vuc2VQb2xpY3k6cmVhZCIsImxpY2Vuc2VQb2xpY3k6d3JpdGUiLCJwYXltZW50TWV0aG9kOnJlYWQiLCJwYXltZW50TWV0aG9kOndyaXRlIiwicGVyc29uYWxBY2Nlc3NUb2tlbjp3cml0ZSIsInByb2R1Y3Q6cmVhZCIsInByb2R1Y3Q6d3JpdGUiLCJyZWZlcnJhbHM6cmVhZCIsInJlZmVycmFsczp3cml0ZSIsInJlbGVhc2U6cmVhZCIsInJlbGVhc2U6d3JpdGUiLCJyb2xlOnJlYWQiLCJyb2xlOndyaXRlIiwidGFnOnJlYWQiLCJ0YWc6d3JpdGUiLCJ0cmlhbEFjdGl2YXRpb246cmVhZCIsInRyaWFsQWN0aXZhdGlvbjp3cml0ZSIsInRyaWFsUG9saWN5OnJlYWQiLCJ0cmlhbFBvbGljeTp3cml0ZSIsInVzZXI6cmVhZCIsInVzZXI6d3JpdGUiLCJ3ZWJob29rOnJlYWQiLCJ3ZWJob29rOndyaXRlIl0sInN1YiI6ImYyYjA0OTQxLTMyOGUtNDI4OS04ZTUzLTVhOGZkYzg5MTlhZCIsImVtYWlsIjoiZWZlb0BzZW5la2EuY29tLnRyIiwianRpIjoiYTNiYjcyYWYtNjNhNi00OWNiLWI5YTQtOGYzNWExYzI2YWNiIiwiaWF0IjoxNTQ0NDU3NDg3LCJ0b2tlbl91c2FnZSI6InBlcnNvbmFsX2FjY2Vzc190b2tlbiIsInRlbmFudGlkIjoiMWRiOGNhZjctZWQ4OC00YWIwLWJiOWUtZmZhNWMwZWUxMjQ0IiwiZXhwIjoxNTQ1Njg1MjAwLCJpc3MiOiJodHRwczovL2FwaS5jcnlwdGxleC5jb20vIiwiYXVkIjoiaHR0cHM6Ly9hcGkuY3J5cHRsZXguY29tIn0.PwA7_AJhc5kYXd4lah1Vdr4a4fq8Fq3F4C1V2-aO8tI";
        public static async Task<CryptlexApiResult<List<CryptlexUser>>> GetAllUsersAsync()
        {
            CryptlexApiResult<List<CryptlexUser>> result = new CryptlexApiResult<List<CryptlexUser>>();

            try
            {
                result.Result = await "https://api.cryptlex.com/v3"
                      .AppendPathSegment("users")
                      .WithOAuthBearerToken(bearer)
                      .GetJsonAsync<List<CryptlexUser>>();

                if (result.Result == null)
                {
                    result.Result = new List<CryptlexUser>();
                }
            }
            catch (FlurlHttpException flurlHttpException)
            {
                result.Error = await flurlHttpException.GetResponseJsonAsync<CryptlexError>();
            }
            catch (Exception ex)
            {
                result.Error = new CryptlexError() { Code = "CelsusError", Message = ex.Message };
            }

            return result;

        }

        public static async Task<CryptlexApiResult<CryptlexUser>> CreateUserAsync(string firstName, string LastName, string eMail, string company, string password)
        {

            CryptlexApiResult<CryptlexUser> result = new CryptlexApiResult<CryptlexUser>();

            var cryptlexUserRequest = new CryptlexCreateUserRequest() { Company = company, Email = eMail, FirstName = firstName, LastName = LastName, Metadata = null, Password = password, Role = "user" };

            try
            {
                result.Result = await "https://api.cryptlex.com/v3"
                   .AppendPathSegment("users")
                   .WithHeader("Content-Type", "application/json")
                   .WithOAuthBearerToken(bearer)
                   .PostJsonAsync(cryptlexUserRequest)
                   .ReceiveJson<CryptlexUser>();
            }
            catch (FlurlHttpException flurlHttpException)
            {
                result.Error = await flurlHttpException.GetResponseJsonAsync<CryptlexError>();
            }
            catch (Exception ex)
            {
                result.Error = new CryptlexError() { Code = "CelsusError", Message = ex.Message };
            }

            return result;

        }
    }
}

