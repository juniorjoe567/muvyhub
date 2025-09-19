using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MuvyHub.Models
{
    public enum Location
    {
        [Display(Name = "Kampala Town")] KampalaTown,
        [Display(Name = "Kololo")] Kololo,
        [Display(Name = "Makerere")] Makerere,
        [Display(Name = "Namirembe Road")] NamirembeRoad,
        [Display(Name = "Kamuli Road")] KamuliRoad,

        [Display(Name = "Bugolobi")] Bugolobi,
        [Display(Name = "Mutungo")] Mutungo,
        [Display(Name = "Kitende")] Kitende,
        [Display(Name = "Kiwatule")] Kiwatule,
        [Display(Name = "Kyaliwajjala")] Kyaliwajjala,
        [Display(Name = "Naalya")] Naalya,
        [Display(Name = "Najjera")] Najjera,
        [Display(Name = "Namugongo")] Namugongo,
        [Display(Name = "Kira Town")] KiraTown,
        [Display(Name = "Bweyogerere")] Bweyogerere,
        [Display(Name = "Bulindo")] Bulindo,

        [Display(Name = "Bukasa Muyenga")] BukasaMuyenga,
        [Display(Name = "Bunga")] Bunga,
        [Display(Name = "Buziga")] Buziga,
        [Display(Name = "Kansanga")] Kansanga,
        [Display(Name = "Munyonyo")] Munyonyo,
        [Display(Name = "Nsambya")] Nsambya,
        [Display(Name = "Namuwongo")] Namuwongo,
        [Display(Name = "Salaama Road")] SalamaRoad,

        [Display(Name = "Busega")] Busega,
        [Display(Name = "Kasubi")] Kasubi,
        [Display(Name = "Kawaala")] Kawaala,
        [Display(Name = "Lubya")] Lubya,
        [Display(Name = "Mengo")] Mengo,
        [Display(Name = "Masanafu")] Masanafu,
        [Display(Name = "Ndejje")] Ndejje,
        [Display(Name = "Namasuba")] Namasuba,
        [Display(Name = "Mutundwe")] Mutundwe,
        [Display(Name = "Rubaga")] Rubaga,

        [Display(Name = "Kawempe")] Kawempe,
        [Display(Name = "Komamboga")] Komamboga,
        [Display(Name = "Kanyanya")] Kanyanya,
        [Display(Name = "Kisaasi")] Kisaasi,
        [Display(Name = "Kyanja Escorts")] KyanjaEscorts,
        [Display(Name = "Kazo")] Kazo,
        [Display(Name = "Mpelerwe")] Mpelerwe,

        [Display(Name = "Gayaza")] Gayaza,
        [Display(Name = "Kajjansi Town")] KajjansiTown,
        [Display(Name = "Bulenga")] Bulenga,
        [Display(Name = "Bunamwaya")] Bunamwaya,
        [Display(Name = "Seguku")] Seguku,
        [Display(Name = "Entebbe Road")] EntebbeRoad,
        [Display(Name = "Mukono")] Mukono,
        [Display(Name = "Seeta")] Seeta,

        [Display(Name = "Makindye")] Makindye,

        [Display(Name = "Ntinda")] Ntinda,
        [Display(Name = "Naguru")] Naguru,
        [Display(Name = "Bukoto")] Bukoto,
        [Display(Name = "Wandegeya")] Wandegeya,
        [Display(Name = "Nakasero")] Nakasero,
        [Display(Name = "Old Kampala")] OldKampala,
        [Display(Name = "Mbuya")] Mbuya,
        [Display(Name = "Luzira")] Luzira,
        [Display(Name = "Kasangati")] Kasangati,
        [Display(Name = "Matugga")] Matugga,
        [Display(Name = "Kawuku")] Kawuku,
        [Display(Name = "Nabbingo")] Nabbingo,
        [Display(Name = "Kyengera")] Kyengera,
        [Display(Name = "Zzana")] Zzana,
        [Display(Name = "Katwe")] Katwe,
        [Display(Name = "Kabalagala")] Kabalagala,
        [Display(Name = "Kamwokya")] Kamwokya,
        [Display(Name = "Banda")] Banda,
        [Display(Name = "Kikoni")] Kikoni,
        [Display(Name = "Kawaala North")] KawaalaNorth,
        [Display(Name = "Kiteezi")] Kiteezi,
        [Display(Name = "Kyebando")] Kyebando,
        [Display(Name = "Ggaba")] Ggaba,
        [Display(Name = "Nakawa")] Nakawa,
        [Display(Name = "Industrial Area")] IndustrialArea,
        [Display(Name = "Entebbe Town")] EntebbeTown,
        [Display(Name = "Namilyango")] Namilyango,
        [Display(Name = "Nkokonjeru")] Nkokonjeru
    }


    public class Person
    {
        public Guid Id { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [RegularExpression(@"^256\d{9}$", ErrorMessage = "WhatsApp number must start with 256 and be 12 digits long.")]
        public string WhatsappNumber { get; set; }

        [Required]
        public Location Location { get; set; }

        public string ProfilePicturePath { get; set; }

        public string OtherMediaPathsJson { get; set; }

        [NotMapped]
        public List<string> OtherMediaPaths
        {
            get => string.IsNullOrEmpty(OtherMediaPathsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(OtherMediaPathsJson);
            set => OtherMediaPathsJson = JsonSerializer.Serialize(value);
        }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsVerified { get; set; }

        [NotMapped]
        public IFormFile ProfilePicture { get; set; }

        [NotMapped]
        public List<IFormFile> OtherMedia { get; set; }
    }
}