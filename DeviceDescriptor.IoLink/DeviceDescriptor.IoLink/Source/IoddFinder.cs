using DeviceDescriptor.Abstract;
using DeviceDescriptor.Abstract.Variables;
using DeviceDescriptor.IoLink.IODD1_1;
using DeviceDescriptor.IoLink.Variables;
using System.Text.Json;

namespace DeviceDescriptor.IoLink.Source
{
    public class IoddFinder : IDescriptorTranslator<Variable>
    {
        private IODevice? _descriptor;
        private readonly HttpClient _httpClient = new();
        public async Task<BasicDescriptor<Variable>?> LoadFromWebAsync(string address, string deviceId, string productName)
        {
            string indexUrl = "https://ioddfinder.io-link.com/api/iodds/search?deviceId={deviceId}";
            try
            {
                if(string.IsNullOrEmpty(address))                    
                    indexUrl = $"https://ioddfinder.io-link.com/api/iodds/search?deviceId={deviceId}";
                var json = await _httpClient.GetStringAsync(indexUrl);

                var ioddList = JsonSerializer.Deserialize<List<IoddFinderResponse>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var match = ioddList?.FirstOrDefault(x =>
                    string.Equals(x.ProductName?.Trim(), productName.Trim(), StringComparison.OrdinalIgnoreCase));

                if (match?.DownloadUrl == null)
                    return null;
                //TODO: Add logic to download, unzip, find xml, cast it to IODD1_1 class
                return new BasicDescriptor<Variable>(new DeviceVariables<Variable>(), new ProcessData<Variable>());
            }
            catch
            {
                return null;
            }
        }
    }
}
