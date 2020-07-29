﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darwin.Features
{
    public enum FeaturePointType
    {
        NoFeature = -1,
        LeadingEdgeBegin = 1,
        LeadingEdgeEnd = 2,
        Tip = 3,
        Notch = 4,
        PointOfInflection = 5,

        Nasion = 6,
        Chin = 7,
        UpperLip = 8,
        Brow = 9,
        BottomLipProtrusion = 10,

        Eye = 100,
        NasalLateralCommissure = 101
    }
}
