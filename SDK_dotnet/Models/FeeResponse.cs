using GluwProSDK.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GluwProSDK.Models
{
    public sealed class FeeResponse
    {
        [Required]
        public ECurrency? Currency { get; set; }

      
        [Required]
        public string MinimumFee { get; set; }
    }
}
