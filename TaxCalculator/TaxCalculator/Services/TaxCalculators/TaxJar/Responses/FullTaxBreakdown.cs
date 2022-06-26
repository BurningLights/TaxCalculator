﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace TaxCalculator.Services.TaxCalculators.TaxJar.Responses
{
    internal class FullTaxBreakdown : TaxBreakdownBase
    {
        [DataMember(Name = "shipping")]
        public TaxBreakdownBase ShippingBreakdown { get; set; }

        [DataMember(Name = "line_items")]
        public List<TaxBreakdownLineItem> LineItemBreakdowns { get; set; }
    }
}
