using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
//using Castle.MicroKernel.SubSystems.Conversion;

namespace Aur.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class AppUser : IdentityUser
    {


        public virtual ICollection<GroupMember> GroupMembers { get; set; }
    }

    public class GroupMember
    {
        public int Id { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [DisplayName("Должность")]
        [StringLength(160)]
        public string Position { get; set; }
    }

    public class Group
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название группы не может быть пустым")]
        [DisplayName("Название")]
        [StringLength(160)]
        public string Title { get; set; }


        public virtual ICollection<GroupMember> GroupMembers { get; set; }
        public virtual ICollection<Manga> Mangas { get; set; }
    }

    public class Manga
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

        public int? GroupId { get; set; }
        public virtual Group Group { get; set; }
    }

    public class Part
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя главы не может быть пустым")]
        [DisplayName("Название")]
        [StringLength(160)]
        public string Title { get; set; }

        //[DisplayName("Ссылка на главу")]
        //[StringLength(1024)]
        //public string PartUrl { get; set; }

        [DisplayName("Страниц в главе")]
        public int Pages { get; set; }

        //[DisplayName("Манга")]
        //public Manga Manga { get; set; }
        //public List<Image> Images { get; set; }
        public int? MangaId { get; set; }
        public virtual Manga Manga { get; set; }
        public virtual ICollection<Image> Images { get; set; }
    }
    public class Image
    {
        public int Id { get; set; }
        public byte[] ImageData { get; set; }
        //  public int PartId { get; set; }
        public int? PartId { get; set; }
        public virtual Part Part { get; set; }
    }


}
