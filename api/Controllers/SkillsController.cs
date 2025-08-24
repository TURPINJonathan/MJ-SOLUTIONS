using Microsoft.AspNetCore.Mvc;
using api.Data;
using api.Models;
using api.DTOs;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using AutoMapper;
using api.Helpers;
using api.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class SkillsController : BaseController
	{
		private readonly IMapper _mapper;

		public SkillsController(
				AppDbContext context,
				ILogger<SkillsController> logger,
				ILogger<FileService> fileLogger,
				IMapper mapper
		) : base(context, logger, fileLogger, mapper)
		{
			_mapper = mapper;
		}

		// GET: api/skills
		[HttpGet]
		public async Task<ActionResult<IEnumerable<SkillResponseDTO>>> GetSkills()
		{
			var skills = await _context.Skills
					.Include(s => s.Files)
					.ToListAsync();

			var responses = skills.Select(skill => _mapper.Map<SkillResponseDTO>(skill)).ToList();

			_logger.LogInformation($"Consultation de la liste des skills par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
			AuditLogHelper.AddAudit(_context, "Consultation liste skills", ConnectedUserEmail, ConnectedUserIp, "Skill", null);
			await _context.SaveChangesAsync();

			return Ok(responses);
		}

		// GET: api/skills/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<SkillResponseDTO>> GetSkill(int id)
		{
			var skill = await _context.Skills
					.Include(s => s.Files)
					.FirstOrDefaultAsync(s => s.Id == id);

			if (skill == null)
			{
				_logger.LogWarning($"Skill {id} non trouvé par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, $"Consultation skill {id} échouée (non trouvé)", ConnectedUserEmail, ConnectedUserIp, "Skill", id);
				await _context.SaveChangesAsync();
				return NotFound();
			}

			var response = _mapper.Map<SkillResponseDTO>(skill);

			_logger.LogInformation($"Consultation du skill {id} par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
			AuditLogHelper.AddAudit(_context, $"Consultation skill {id} réussie", ConnectedUserEmail, ConnectedUserIp, "Skill", id);
			await _context.SaveChangesAsync();

			return Ok(response);
		}

		// POST: api/skills
		[HttpPost]
		[Authorize(Roles = "SUPER_ADMIN")]
		[Authorize(Policy = "CREATE_SKILL")]
		public async Task<ActionResult<SkillResponseDTO>> PostSkill([FromForm] SkillCreateDTO model)
		{
			if (model.Files == null || model.Files.Count == 0)
			{
				_logger.LogWarning($"Tentative de création de skill sans fichier par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, "Échec création skill (aucun fichier)", ConnectedUserEmail, ConnectedUserIp, "Skill", null);
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

			var response = _mapper.Map<SkillResponseDTO>(skill);

			_logger.LogInformation($"Skill '{skill.Name}' créé par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
			AuditLogHelper.AddAudit(_context, $"Création skill '{skill.Name}' réussie", ConnectedUserEmail, ConnectedUserIp, "Skill", skill.Id);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, response);
		}

		// PATCH: api/skills/{id}
		[HttpPatch("{id}")]
		[Authorize(Roles = "SUPER_ADMIN")]
		[Authorize(Policy = "UPDATE_SKILL")]
		public async Task<ActionResult<SkillResponseDTO>> PatchSkill(int id, [FromBody] SkillUpdateDTO model)
		{
			var skill = await _context.Skills
					.Include(s => s.Files)
					.FirstOrDefaultAsync(s => s.Id == id);

			if (skill == null)
			{
				_logger.LogWarning($"Modification skill échouée : skill {id} non trouvé par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, $"Échec modification skill {id} (non trouvé)", ConnectedUserEmail, ConnectedUserIp, "Skill", id);
				await _context.SaveChangesAsync();
				return NotFound();
			}

			// Mise à jour partielle
			if (model.Name != null) skill.Name = model.Name;
			if (model.Description != null) skill.Description = model.Description;
			if (model.Color != null) skill.Color = model.Color;
			if (model.IsFavorite.HasValue) skill.IsFavorite = model.IsFavorite.Value;
			if (model.IsHardSkill.HasValue) skill.IsHardSkill = model.IsHardSkill.Value;
			if (model.Type.HasValue) skill.Type = model.Type;
			if (model.Proficiency.HasValue) skill.Proficiency = model.Proficiency;
			if (model.DocumentationUrl != null) skill.DocumentationUrl = model.DocumentationUrl;

			_context.Skills.Update(skill);
			await _context.SaveChangesAsync();

			var response = _mapper.Map<SkillResponseDTO>(skill);

			_logger.LogInformation($"Skill {id} modifié par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
			AuditLogHelper.AddAudit(_context, $"Modification skill {id} réussie", ConnectedUserEmail, ConnectedUserIp, "Skill", id);
			await _context.SaveChangesAsync();

			return Ok(response);
		}

		// DELETE: api/skills/{id}
		[HttpDelete("{id}")]
		[Authorize(Roles = "SUPER_ADMIN")]
		[Authorize(Policy = "DELETE_SKILL")]
		public async Task<IActionResult> DeleteSkill(int id)
		{
			var skill = await _context.Skills
					.Include(s => s.Files)
					.FirstOrDefaultAsync(s => s.Id == id);

			if (skill == null)
			{
				_logger.LogWarning($"Suppression skill échouée : skill {id} non trouvé par {ConnectedUserEmail} depuis l'IP {ConnectedUserIp}");
				AuditLogHelper.AddAudit(_context, $"Échec suppression skill {id} (non trouvé)", ConnectedUserEmail, ConnectedUserIp, "Skill", id);
				await _context.SaveChangesAsync();
				return NotFound();
			}

			var files = skill.Files.ToList();

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
			AuditLogHelper.AddAudit(_context, $"Suppression skill {id} réussie", ConnectedUserEmail, ConnectedUserIp, "Skill", id);
			await _context.SaveChangesAsync();

			return NoContent();
		}

	}
		
}