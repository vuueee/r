using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Aur.Models;

namespace Aur.ViewModels
{
    public class MangaViewModel
    { 
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя главы не может быть пустым")]
        [DisplayName("Название")]
        [StringLength(160)]
        public string Title { get; set; }


        [DisplayName("Описание")]
        [StringLength(260)]
        public string Desc { get; set; }
        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0.01, 100.00,
           ErrorMessage = "Цена должна быть между 0.01 и 100.00")]
        public decimal Price { get; set; }

        [DisplayName("Жанр")]
        public int Genre { get; set; }



        [DisplayName("Глав в манге")]
        public int Count { get; set; }


        [DisplayName("Начало платных глав")]
        public int PriceStart { get; set; } = 0;
        public virtual ICollection<Part> Parts { get; set; }



        public List<bool> CheckBox { get; set; }


    }
   
}
