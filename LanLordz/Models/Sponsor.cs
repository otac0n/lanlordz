namespace LanLordz.Models
{
    using System.ComponentModel.DataAnnotations;

    [MetadataType(typeof(SponsorMetadata))]
    public partial class Sponsor
    {
        private class SponsorMetadata
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public string Url { get; set; }

            [Required]
            public string ImageUrl { get; set; }
        }
    }
}
