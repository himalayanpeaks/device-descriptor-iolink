using DeviceDescriptor.Abstract;
using DeviceDescriptor.Abstract.Variables;

namespace DeviceDescriptor.IoLink.Variables
{
    public class Variable : BasicVariable
    {
        private int subindex;
        private bool isDynamic;

        public int Subindex 
        { 
            get => subindex; 
            set => SetProperty(ref subindex, value); 
        }

        public bool IsDynamic 
        {
            get => isDynamic; 
            set => SetProperty(ref isDynamic, value); 
        }
        public Variable(string name, int index, int subindex, bool isDynamic, Definition.AccessType access, Definition.DataType dataType, int arrayCount, int lengthInBits, int offset, string? value, string? defaultValue, string? minimum, string? maximum, string? valid) 
            : base(name, index, access, dataType, arrayCount, lengthInBits, offset, value, defaultValue, minimum, maximum, valid)
        {
            Subindex = subindex;
            IsDynamic = isDynamic;
        }
    }
}
