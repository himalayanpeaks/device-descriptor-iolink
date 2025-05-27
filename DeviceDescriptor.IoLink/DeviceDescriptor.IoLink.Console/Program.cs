using DeviceDescriptor.IoLink;
using DeviceDescriptor.IoLink.Source;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Translator translator = new Translator(new LocalStorage());

            // Load search results (optional pre-fetching step)
           // await translator.LoadFromWebAsync("https://ioddfinder.io-link.com/api/iodds/search?deviceId=131083");

            // Load and retrieve descriptor
            var descriptor = translator.LoadFromWebAsync(@"c:\\temp\\F77.xml", "131083", "E2EQ-X7B4-IL2");

            if (descriptor != null)
            {
                Console.WriteLine("Descriptor loaded successfully!");
                Console.WriteLine($"Specific variables: {descriptor.Variables.SpecificVariableCollection.Count}");
            //    Console.WriteLine($"Process Data In: {descriptor.ProcessData.ProcessDataInCollection.Count}");
            }
            else
            {
                Console.WriteLine("Failed to load descriptor.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }
    }
}
