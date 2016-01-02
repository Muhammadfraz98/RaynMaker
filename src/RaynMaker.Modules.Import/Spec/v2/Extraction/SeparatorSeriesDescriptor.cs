﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace RaynMaker.Modules.Import.Spec.v2.Extraction
{
    /// <summary>
    /// Describes a series within a table of a CSV formatted document.
    /// </summary>
    [DataContract( Namespace = "https://github.com/bg0jr/RaynMaker/Import/Spec/v2", Name = "SeparatorSeriesDescriptor" )]
    public class SeparatorSeriesDescriptor : SeriesDescriptorBase
    {
        public SeparatorSeriesDescriptor( string figure )
            : base( figure )
        {
        }

        /// <summary>
        /// Cell separator used in the file.
        /// </summary>
        [Required( AllowEmptyStrings = false )]
        [DataMember]
        public string Separator { get; set; }
    }
}
