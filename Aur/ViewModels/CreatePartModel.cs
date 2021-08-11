using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Aur.ViewModels
{
    public class CreatePartModel
    { 

        //[DataType(DataType.ImageUrl)]
        [Required(ErrorMessage = "Имя главы не может быть пустым")]
        [DisplayName("Название манги")]
        [StringLength(160)]
        public string Title { get; set; }
        public List<byte[]> Images { get; set; }

        public List<bool> CheckBox { get; set; }
        public int mangaid { get; set; }

        [DisplayName("Картинки")]
        public IFormFileCollection UploadImages { get; set; }
    }
   
}
