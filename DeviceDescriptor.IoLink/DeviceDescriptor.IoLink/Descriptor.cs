using DeviceDescriptor.Abstract;
using DeviceDescriptor.Abstract.Variables;
using DeviceDescriptor.IoLink.Variables;

namespace DeviceDescriptor.IoLink 
{
    public class Descriptor : BasicDescriptor<Variable>
    {
        public Descriptor(string name, DeviceVariables<Variable> variables, ProcessData<Variable> processData) 
            : base(name, variables, processData)
        {
        }
    }
}
