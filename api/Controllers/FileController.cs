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

			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileResource.FilePath.TrimStart('/'));
			if (!System.IO.File.Exists(filePath))
				return NotFound();

			var compressedStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);

			return File(gzipStream, fileResource.ContentType, fileResource.FileName);
		}

	}
		
}