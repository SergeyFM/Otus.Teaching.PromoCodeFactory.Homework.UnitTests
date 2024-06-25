using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;

namespace Otus.Teaching.PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Предпочтения клиентов
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PreferencesController
    : ControllerBase {
    private readonly IRepository<Preference> _preferencesRepository;

    public PreferencesController(IRepository<Preference> preferencesRepository) => _preferencesRepository = preferencesRepository;

    [HttpGet]
    public async Task<ActionResult<List<PreferenceResponse>>> GetPreferencesAsync() {
        IEnumerable<Preference> preferences = await _preferencesRepository.GetAllAsync();

        List<PreferenceResponse> response = preferences.Select(x => new PreferenceResponse() {
            Id = x.Id,
            Name = x.Name
        }).ToList();

        return Ok(response);
    }
}