using DeviceDescriptor.Abstract;
using DeviceDescriptor.IoLink.Variables;
using DeviceDescriptor.IoLink.IODD1_1;
using System.Xml.Serialization;
using DeviceDescriptor.Abstract.Variables;
using static DeviceDescriptor.Abstract.Definition;
using System.Globalization;

namespace DeviceDescriptor.IoLink.Source
{
    public class LocalStorage : IDescriptorTranslator<Variable>
    {
        public Task<BasicDescriptor<Variable>?> LoadFromWebAsync(string address, string deviceId = "0", string productName = "")
        {
            if (!File.Exists(address))
                return Task.FromResult<BasicDescriptor<Variable>?>(null);

            IODevice? root;
            try
            {
                var serializer = new XmlSerializer(typeof(IODevice));
                using var stream = File.OpenRead(address);
                root = serializer.Deserialize(stream) as IODevice;
            }
            catch
            {
                return Task.FromResult<BasicDescriptor<Variable>?>(null);
            }

            if (root?.ProfileBody?.DeviceFunction == null)
                return Task.FromResult<BasicDescriptor<Variable>?>(null);

            var variableDefs = root.ProfileBody.DeviceFunction.VariableCollection?.Variable?.OfType<VariableT>().ToList();
            var datatypeMap = root.ProfileBody.DeviceFunction.DatatypeCollection?.Datatype?.OfType<DatatypeT>().ToDictionary(d => d.id, d => d);

            var variables = new List<Variable>();

            foreach (var varDef in variableDefs ?? Enumerable.Empty<VariableT>())
            {
                string name = varDef.id;
                int index = varDef.index != 0 ? varDef.index : -1;
                bool isDynamic = varDef.dynamic;

                AccessType access = varDef.accessRights switch
                {
                    AccessRightsT.ro => AccessType.R,
                    AccessRightsT.wo => AccessType.W,
                    AccessRightsT.rw => AccessType.RW,
                    _ => AccessType.RW
                };

                string? defaultValue = null;
                if (varDef is VariableCollectionTVariable v && !string.IsNullOrWhiteSpace(v.defaultValue))
                    defaultValue = v.defaultValue;

                if (varDef.Item is DatatypeT inlineDt)
                {
                    ParseDatatype(inlineDt, datatypeMap!, variables, name, index, 0, 0, 
                        isDynamic, access, defaultValue, varDef.RecordItemInfo);
                }
                else if (varDef.Item is DatatypeRefT refT && datatypeMap != null && datatypeMap.TryGetValue(refT.datatypeId, out var dtDef))
                {
                    ParseDatatype(dtDef, datatypeMap, variables, name, index, 0, 0, 
                        isDynamic, access, defaultValue, varDef.RecordItemInfo);
                }
            }

            var descriptor = new BasicDescriptor<Variable>(
                new DeviceVariables<Variable>
                {
                    SpecificVariableCollection = variables,
                    StandardVariableCollection = new List<Variable>(),
                    SystemVariableCollection = new List<Variable>(),
                    CommandCollection = new List<Variable>()
                },
                null // processData
            );

            return Task.FromResult<BasicDescriptor<Variable>?>(descriptor);
        }

        private static void ParseDatatype(
            DatatypeT dt,
            Dictionary<string, DatatypeT> datatypeMap,
            List<Variable> variables,
            string parentId,
            int index,
            int subindex,
            int offset,
            bool isDynamic,
            AccessType access,
            string? defaultValue,
            RecordItemInfoT[] recordItemInfo)
        {
            DataType dataType = DataType.Byte;
            int lengthInBits = 0;
            string? minValue = null, maxValue = null, valid = null;

            switch (dt)
            {
                case UIntegerT u:
                    dataType = DataType.UINT;
                    lengthInBits = u.bitLength;
                    (minValue, maxValue) = GetMinMaxFromValueRange(u.Items);
                    valid = GetValidValues(u.Items);
                    break;

                case IntegerT i:
                    dataType = DataType.INT;
                    lengthInBits = i.bitLength;
                    (minValue, maxValue) = GetMinMaxFromValueRange(i.Items);
                    valid = GetValidValues(i.Items);
                    break;

                case Float32T f:
                    dataType = DataType.Float32;
                    lengthInBits = 32;
                    (minValue, maxValue) = GetMinMaxFromValueRange(f.Items);
                    valid = GetValidValues(f.Items);
                    break;

                case StringT s:
                    dataType = DataType.CHAR;
                    lengthInBits = s.fixedLength * 8;
                    break;

                case BooleanT b:
                    dataType = DataType.BOOL;
                    lengthInBits = 8;
                    valid = GetValidValues(b.SingleValue);
                    break;

                case RecordT r:
                    lengthInBits = r.bitLength;
                    dataType = DataType.Record;
                    var local = new Variable(
                                name: parentId,
                                index: index,
                                subindex: subindex,
                                access: access,
                                isDynamic: isDynamic,
                                dataType: dataType,
                                arrayCount: 1,
                                lengthInBits: lengthInBits,
                                offset: 0,
                                defaultValue: defaultValue,
                                minimum: minValue,
                                maximum: maxValue,
                                valid: valid,
                                value: null
                            );

                    variables.Add(local);
                    int count = 0;
                    foreach (var item in r.RecordItem)
                    {
                        string subId = item.Name.textId;
                        var dtypeId = item.Item is DatatypeRefT refT ? refT.datatypeId : null;
                        if (dtypeId != null && datatypeMap.TryGetValue(dtypeId, out var nestedType))
                        {
                            ParseDatatype(nestedType, datatypeMap, variables, subId, index, 
                                item.subindex, item.bitOffset, isDynamic, access, recordItemInfo[count++].defaultValue, null);
                        }
                    }
                    return; // no direct variable to add here
            }

            var variable = new Variable(
                name: parentId,
                index: index,
                subindex: subindex,
                access: access,
                isDynamic: isDynamic,
                dataType: dataType,
                arrayCount: 1,
                lengthInBits: lengthInBits,
                offset: offset,
                defaultValue: defaultValue,
                minimum: minValue,
                maximum: maxValue,
                valid: valid,
                value: null
            );

            variables.Add(variable);
        }

        public static (string? Min, string? Max) GetMinMaxFromValueRange(AbstractValueT[]? valueRange)
        {
            if (valueRange == null)
                return (null, null);

            return valueRange.FirstOrDefault() switch
            {
                UIntegerValueRangeT u => (u.lowerValue.ToString(), u.upperValue.ToString()),
                IntegerValueRangeT i => (i.lowerValue.ToString(), i.upperValue.ToString()),
                Float32ValueRangeT f => (f.lowerValue.ToString(), f.upperValue.ToString()),
                _ => (null, null)
            };
        }

        public static string? GetValidValues(AbstractValueT[]? valueRange)
        {
            if (valueRange == null)
                return null;

            var values = valueRange.Select(v => v switch
            {
                UIntegerValueT u => u.value.ToString(),
                IntegerValueT i => i.value.ToString(),
                Float32ValueT f => f.value.ToString(CultureInfo.InvariantCulture),
                BooleanValueT b => b.value.ToString(),
                _ => null
            }).Where(s => s != null);

            return string.Join(",", values!);
        }
    }
}
