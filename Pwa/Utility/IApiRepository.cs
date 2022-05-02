namespace Pwa.Utility
{
    public interface IApiRepository
    {
        Task<HttpResponseMessage> ImportTransactions(MultipartFormDataContent dataContent);
    }
    public class ApiRepository : IApiRepository
    {
        private readonly HttpClient _httpClient;

        public ApiRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> ImportTransactions(MultipartFormDataContent dataContent)
        {
            return await _httpClient.PostAsync($"transactions/import", dataContent);
        }
    }
}
