using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Presentation.Events
{
    internal class NavigationEvent : PresentationEventBase
    {
        public NavigationEvent(Type featureType)
        {
            FeatureType = featureType;
        }

        public Type FeatureType { get; }
    }
}
