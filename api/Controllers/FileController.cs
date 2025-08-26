using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using api.Data;

namespace api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FileController : ControllerBase
	{
		private readonly AppDbContext _context;
		public FileController(AppDbContext context) { _context = context; }

		[HttpGet("{id}")]
		public IActionResult GetFile(int id)
		{
			var fileResource = _context.FileResources.Find(id);
			if (fileResource == null)
				return NotFound();

			var baseDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
			var requestedPath = Path.GetFullPath(Path.Combine(baseDirectory, fileResource.FilePath.TrimStart('/')));
			if (!requestedPath.StartsWith(baseDirectory, StringComparison.Ordinal))
				return BadRequest("Chemin de fichier invalide.");

			if (!System.IO.File.Exists(requestedPath))
				return NotFound();

			var compressedStream = new FileStream(requestedPath, FileMode.Open, FileAccess.Read);
			var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);

			return File(gzipStream, fileResource.ContentType, fileResource.FileName);
		}
				
	}
		
}