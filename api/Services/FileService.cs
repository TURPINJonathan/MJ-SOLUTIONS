using System.IO.Compression;
using api.Models;
using api.DTOs;
using api.Helpers;
using api.Data;
using api.Utils;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace api.Services
{
	public class FileService
	{
		private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
		private readonly ILogger<FileService> _logger;
		private readonly AppDbContext _context;

		public FileService(ILogger<FileService> logger, AppDbContext context)
		{
			_logger = logger;
			_context = context;
		}

		public List<FileResource> SaveFilesCompressed(
				List<IFormFile> files,
				List<FileResourceDTO>? metaList,
				int ownerId,
				string ownerType,
				string userEmail = "unknown",
				string userIp = "unknown"
		)
		{
			_logger.LogInformation($"Début de l'upload de {files.Count} fichier(s) pour {ownerType} {ownerId} par {userEmail} ({userIp})");
			AuditLogHelper.AddAudit(_context, $"Début upload {files.Count} fichier(s) pour {ownerType} {ownerId}", userEmail, userIp, ownerType, ownerId);
			_context.SaveChanges();

			if (metaList != null && metaList.Count != files.Count)
			{
				_logger.LogWarning("Le nombre de métadonnées ne correspond pas au nombre de fichiers.");
				AuditLogHelper.AddAudit(_context, "Échec upload : nombre de métadonnées incorrect", userEmail, userIp, ownerType, ownerId);
				_context.SaveChanges();
				throw new ArgumentException("Le nombre de métadonnées ne correspond pas au nombre de fichiers.");
			}

			var maxSize = 10 * 1024 * 1024; // 10 Mo
			var result = new List<FileResource>();
			for (int i = 0; i < files.Count; i++)
			{
				var file = files[i];
				var meta = metaList != null ? metaList[i] : null;

				if (file.Length > maxSize)
				{
					_logger.LogWarning($"Fichier trop volumineux : {file.FileName} ({file.Length} octets)");
					AuditLogHelper.AddAudit(_context, $"Échec upload : fichier trop volumineux ({file.FileName})", userEmail, userIp, ownerType, ownerId);
					_context.SaveChanges();
					throw new ArgumentOutOfRangeException("Fichier trop volumineux (max 10 Mo).");
				}

				if (!Directory.Exists(_uploadPath))
				{
					_logger.LogInformation($"Création du dossier {_uploadPath}");
					Directory.CreateDirectory(_uploadPath);
				}

				var sanitizedName = StringUtils.SanitizeString(file.FileName);
				var uniqueName = $"{Path.GetFileNameWithoutExtension(sanitizedName)}_{Guid.NewGuid()}{Path.GetExtension(sanitizedName)}.gz";
				var filePath = Path.Combine(_uploadPath, uniqueName);

				try
				{
					using (var fileStream = file.OpenReadStream())
					using (var compressedStream = new FileStream(filePath, FileMode.Create))
					using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal))
					{
						fileStream.CopyTo(gzipStream);
					}
					_logger.LogInformation($"Fichier {sanitizedName} compressé et sauvegardé sous {filePath}");
					AuditLogHelper.AddAudit(_context, $"Fichier {sanitizedName} uploadé pour {ownerType} {ownerId}", userEmail, userIp, ownerType, ownerId);
					_context.SaveChanges();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Erreur lors de la sauvegarde du fichier {sanitizedName}");
					AuditLogHelper.AddAudit(_context, $"Erreur upload fichier {sanitizedName} : {ex.Message}", userEmail, userIp, ownerType, ownerId);
					_context.SaveChanges();
					throw;
				}

				result.Add(new FileResource
				{
					FileName = sanitizedName,
					FilePath = $"/uploads/{uniqueName}",
					ContentType = file.ContentType,
					Description = meta?.Description,
					Size = file.Length,
					IsBanner = meta?.IsBanner,
					IsLogo = meta?.IsLogo,
					IsMaster = meta?.IsMaster,
					OwnerId = ownerId,
					OwnerType = ownerType
				});
			}

			_logger.LogInformation($"Upload terminé pour {files.Count} fichier(s) pour {ownerType} {ownerId} par {userEmail} ({userIp})");
			AuditLogHelper.AddAudit(_context, $"Upload terminé pour {ownerType} {ownerId}", userEmail, userIp, ownerType, ownerId);
			_context.SaveChanges();

			return result;
		}

	}
		
}