using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsFileMaker
{
    public interface SerializableData<t> : Serializable, Deserializable<t>
    {
      
    }

    public interface Serializable
    {
        string Serialize();
    }

    public interface Deserializable<t>
    {
        t Deserialize(string filedata);
    }
}
