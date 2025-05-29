using DeviceDescriptor.Abstract;
using DeviceDescriptor.IoLink.Variables;

namespace DeviceDescriptor.IoLink
{
    public class Translator
    {        
        private readonly IDescriptorTranslator<Variable> _descriptorTranslator;

        public Translator(IDescriptorTranslator<Variable> descriptorTranslator)
        {
            _descriptorTranslator = descriptorTranslator;
        }

        public BasicDescriptor<Variable>? LoadFromWebAsync(string address, string deviceId, string productName)
        {
            BasicDescriptor<Variable>? descriptor = null;
            try
            {
                descriptor = _descriptorTranslator.LoadFromWebAsync(address, deviceId, productName).Result;
            }
            catch
            {
                return null;
            }
            return descriptor;
        }       
    }
}