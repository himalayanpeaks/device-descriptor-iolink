using DeviceDescriptor.Abstract;
using DeviceDescriptor.Abstract.Variables;
using DeviceDescriptor.IoLink.Variables;

namespace DeviceDescriptor.IoLink 
{
    public class Descriptor : BasicDescriptor<Variable>
    {
        public Descriptor(DeviceVariables<Variable> variables, ProcessData<Variable> processData) 
            : base(variables, processData)
        {
        }
    }
}
