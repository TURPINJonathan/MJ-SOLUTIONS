using Microsoft.AspNetCore.Mvc;
using api.Data;
using api.Models;
using api.DTOs;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using api.Helpers;
using System;

namespace api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class SkillsController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly ILogger<SkillsController> _logger;
		private readonly ILogger<FileService> _fileLogger;

		public SkillsController(AppDbContext context, ILogger<SkillsController> logger, ILogger<FileService> fileLogger)
		{
			_context = context;
			_logger = logger;
			_fileLogger = fileLogger;
		}

		private string ConnectedUserEmail => User.Identity?.Name ?? "unknown";
		private string ConnectedUserIp => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

		// GET: api/skills
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Skill>>> GetSkills()
		{
			_logger.LogInformation($"Consultation de la liste des skills par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
			AuditLogHelper.AddAudit(_context, "Consultation liste skills", ConnectedUserEmail, ConnectedUserIp);
			await _context.SaveChangesAsync();

			return await _context.Skills.ToListAsync();
		}

		// GET: api/skills/5
		[HttpGet("{id}")]
		public async Task<ActionResult<Skill>> GetSkill(int id)
		{
			var skill = await _context.Skills.FindAsync(id);
			if (skill == null)
			{
				_logger.LogWarning($"Skill {id} non trouvé par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, $"Consultation skill {id} échouée (non trouvé)", ConnectedUserEmail, ConnectedUserIp);
				await _context.SaveChangesAsync();
				return NotFound();
			}
			_logger.LogInformation($"Consultation du skill {id} par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
			AuditLogHelper.AddAudit(_context, $"Consultation skill {id} réussie", ConnectedUserEmail, ConnectedUserIp);
			await _context.SaveChangesAsync();
			return skill;
		}

		// POST: api/skills
		[HttpPost]
		[Authorize(Roles = "SUPER_ADMIN")]
		public async Task<ActionResult<Skill>> PostSkill([FromForm] SkillCreateDTO model)
		{
			if (model.Files == null || model.Files.Count == 0)
			{
				_logger.LogWarning($"Tentative de création de skill sans fichier par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, "Échec création skill (aucun fichier)", ConnectedUserEmail, ConnectedUserIp);
				await _context.SaveChangesAsync();
				return BadRequest(new { error = "Au moins un fichier est obligatoire." });
			}

			var skill = new Skill
			{
				Name = model.Name,
				Description = model.Description,
				Color = model.Color,
				IsFavorite = model.IsFavorite,
				IsHardSkill = model.IsHardSkill,
				Type = model.Type,
				Proficiency = model.Proficiency,
				DocumentationUrl = model.DocumentationUrl,
				Files = new List<FileResource>()
			};

			_context.Skills.Add(skill);
			await _context.SaveChangesAsync();

			var fileService = new FileService(_fileLogger, _context);
			var fileResources = fileService.SaveFilesCompressed(model.Files, model.FilesMeta, skill.Id, "Skill");

			foreach (var fr in fileResources)
			{
				fr.OwnerId = skill.Id;
				fr.OwnerType = "Skill";
				_context.FileResources.Add(fr);
				skill.Files.Add(fr);
			}
			await _context.SaveChangesAsync();

			_logger.LogInformation($"Skill '{skill.Name}' créé par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
			AuditLogHelper.AddAudit(_context, $"Création skill '{skill.Name}' réussie", ConnectedUserEmail, ConnectedUserIp);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, skill);
		}

		// PUT: api/skills/5
		[HttpPut("{id}")]
		[Authorize(Roles = "SUPER_ADMIN")]
		public async Task<IActionResult> PutSkill(int id, Skill skill)
		{
			if (id != skill.Id)
			{
				_logger.LogWarning($"Modification skill échouée : id incohérent ({id} != {skill.Id}) par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, $"Échec modification skill {id} (id incohérent)", ConnectedUserEmail, ConnectedUserIp);
				await _context.SaveChangesAsync();
				return BadRequest();
			}

			_context.Entry(skill).State = EntityState.Modified;
			try
			{
				await _context.SaveChangesAsync();
				_logger.LogInformation($"Skill {id} modifié par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, $"Modification skill {id} réussie", ConnectedUserEmail, ConnectedUserIp);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!_context.Skills.Any(e => e.Id == id))
				{
					_logger.LogWarning($"Modification skill échouée : skill {id} non trouvé par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
					AuditLogHelper.AddAudit(_context, $"Échec modification skill {id} (non trouvé)", ConnectedUserEmail, ConnectedUserIp);
					await _context.SaveChangesAsync();
					return NotFound();
				}
				else
					throw;
			}
			return NoContent();
		}

		// DELETE: api/skills/5
		[HttpDelete("{id}")]
		[Authorize(Roles = "SUPER_ADMIN")]
		public async Task<IActionResult> DeleteSkill(int id)
		{
			var skill = await _context.Skills.FindAsync(id);
			if (skill == null)
			{
				_logger.LogWarning($"Suppression skill échouée : skill {id} non trouvé par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, $"Échec suppression skill {id} (non trouvé)", ConnectedUserEmail, ConnectedUserIp);
				await _context.SaveChangesAsync();
				return NotFound();
			}

			var files = await _context.FileResources
					.Where(f => f.OwnerId == id && f.OwnerType == "Skill")
					.ToListAsync();

			foreach (var file in files)
			{
				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));
				if (System.IO.File.Exists(filePath))
					System.IO.File.Delete(filePath);

				_context.FileResources.Remove(file);
			}

			_context.Skills.Remove(skill);
			await _context.SaveChangesAsync();

			_logger.LogInformation($"Skill {id} supprimé par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
			AuditLogHelper.AddAudit(_context, $"Suppression skill {id} réussie", ConnectedUserEmail, ConnectedUserIp);
			await _context.SaveChangesAsync();

			return NoContent();
		}

	}
		
}