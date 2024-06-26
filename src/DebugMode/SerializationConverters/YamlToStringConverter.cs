using LBoL.Base;
using System;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DebugMode.SerializationConverters
{
    public class YamlToStringConverter : IYamlTypeConverter
    {

        public bool Accepts(Type type)
        {
            return new Type[] { typeof(ManaGroup) }.Contains(type);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException("deeznuts");
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var val = value?.ToString() ?? "";

            emitter.Emit(new Scalar(val));
        }
    }
}
