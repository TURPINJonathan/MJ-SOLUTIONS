using System.IO.Compression;
using api.Models;
using api.DTOs;
using api.Helpers;
using api.Data;
using api.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;

namespace api.Services
{
	public class FileService
	{
		private readonly string _uploadPath;
		private readonly ILogger<FileService> _logger;
		private readonly AppDbContext _context;
		private readonly long _maxUploadFileSize;
		private const long DEFAULT_MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB

		public FileService(ILogger<FileService> logger, AppDbContext context, IConfiguration configuration)
		{
			_logger = logger;
			_context = context;
			_uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
			_maxUploadFileSize = configuration.GetValue<long>("MaxUploadFileSize", DEFAULT_MAX_FILE_SIZE); // 10 Mo par défaut
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

			if (!Directory.Exists(_uploadPath))
			{
				_logger.LogInformation($"Création du dossier {_uploadPath}");
				Directory.CreateDirectory(_uploadPath);
			}

			var result = new List<FileResource>();
			for (int i = 0; i < files.Count; i++)
			{
				var file = files[i];
				var meta = metaList != null ? metaList[i] : null;

				if (file.Length > _maxUploadFileSize)
				{
					_logger.LogWarning($"Fichier trop volumineux : {file.FileName} ({file.Length} octets)");
					AuditLogHelper.AddAudit(_context, $"Échec upload : fichier trop volumineux ({file.FileName})", userEmail, userIp, ownerType, ownerId);
					_context.SaveChanges();
					throw new ArgumentOutOfRangeException($"Fichier trop volumineux (max {_maxUploadFileSize} octets).");
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

			_logger.LogInformation("Upload terminé pour {FileCount} fichier(s) pour {OwnerType} {OwnerId} par {UserEmail} ({UserIp})", files.Count, ownerType, ownerId, userEmail, userIp);
			AuditLogHelper.AddAudit(_context, $"Upload terminé pour {ownerType} {ownerId}", userEmail, userIp, ownerType, ownerId);
			_context.SaveChanges();

			return result;
		}

	}
		
}