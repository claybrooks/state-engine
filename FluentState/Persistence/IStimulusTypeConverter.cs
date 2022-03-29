using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentState.Persistence
{
    public interface IStimulusTypeConverter<TStimulus>
        where TStimulus : struct
    {
        TStimulus? Convert(string state);
        string Convert(TStimulus state);
    }
}
