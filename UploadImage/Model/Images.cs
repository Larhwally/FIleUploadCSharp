using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UploadImage.Model
{
    public class Images
    {
        public string imgPath { get; set; }
        public string imgUniquePath { get; set; }
        public string uploadBy { get; set; }
        public DateTime uploadDatetime { get; set; }

        //[NotMapped]
        //public IFormFile file { get; set; }
    }
}
