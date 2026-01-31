using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace NavetraERP.Controllers
{
    // KIEGÉSZÍTETTÜK a DTO-t a frontend új mezőivel
    public class DocumentUploadDto
    {
        public IFormFile File { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }   // Frontend küldi
        public string? TargetFolder { get; set; }
        public string? VariablesConfig { get; set; } // EZT a JSON stringet küldi a frontend mentéskor
    }

    [Route("api/templates")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public DocumentsController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // --- ÚJ VÉGPONT: Fájl elemzés szimuláció (DUMMY ADAT) ---
        // Ezt hívja a Frontend a handleFileChange során
        [HttpPost("parse-variables")]
        public IActionResult ParseVariables(IFormFile file)
        {
            // Itt később majd a DocX olvasó logika lesz (OpenXML)
            
            // MOST: Visszaadunk fix dummy adatokat
            var dummyVariables = new List<string> 
            { 
                "ugyfel_neve", 
                "szerzodes_szam", 
                "szuletesi_datum", 
                "fizetesi_mod",
                "hirlevel"
            };

            // A frontend egy sima JSON tömböt vár: ["var1", "var2"]
            return Ok(dummyVariables);
        }

        // --- MENTÉS (Create) ---
        [HttpPost]
        public async Task<IActionResult> Create(DocumentUploadDto dto)
        {
            // 1. Validálás
            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest("Nem érkezett fájl.");
            }

            try
            {
                // 2. Mappa logika
                string folderName = string.IsNullOrWhiteSpace(dto.TargetFolder) ? "uploads" : dto.TargetFolder;
                folderName = folderName.Replace("..", "").Replace("/", "\\");
                string rootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                string uploadsFolder = Path.Combine(rootPath, folderName);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 3. Mentés
                string uniqueFileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(fileStream);
                }

                // 4. ADATBÁZIS MENTÉS HELYE
                // Itt már eléred a dto.VariablesConfig-ot is!
                // Console.WriteLine($"Mentendő config: {dto.VariablesConfig}");
                
                // Példa Entity létrehozásra (ha van DbContext):
                /*
                var template = new Template {
                    Name = dto.Name,
                    Description = dto.Description,
                    FilePath = filePath,
                    VariablesConfig = dto.VariablesConfig // JSON string mentése
                };
                _context.Templates.Add(template);
                await _context.SaveChangesAsync();
                */

                return Ok(new { 
                    message = "Sikeres feltöltés", 
                    filePath = filePath,
                    variablesConfig = dto.VariablesConfig // Visszaküldjük ellenőrzésképp
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Belső hiba: {ex.Message}");
            }
        }
        
        // --- SZERKESZTÉS (Edit - PUT) ---
        // Ha a TemplateEdit oldalon a PUT-ot használod, kell egy ilyen is:
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] DocumentUploadDto dto)
        {
            // Itt hasonló logika, csak Update
            // Ha dto.File != null, akkor lecseréled a fájlt
            // És mindig frissíted a VariablesConfig mezőt az adatbázisban
            
            return Ok(new { message = "Sikeres frissítés (Dummy)" });
        }
    }
}