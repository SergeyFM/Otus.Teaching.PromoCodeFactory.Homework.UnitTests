﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;

namespace Otus.Teaching.PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Промокоды
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PromocodesController
    : ControllerBase {
    private readonly IRepository<PromoCode> _promoCodesRepository;

    public PromocodesController(IRepository<PromoCode> promoCodesRepository) => _promoCodesRepository = promoCodesRepository;

    /// <summary>
    /// Получить все промокоды
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<List<PromoCodeShortResponse>>> GetPromocodesAsync() {
        IEnumerable<PromoCode> preferences = await _promoCodesRepository.GetAllAsync();

        List<PromoCodeShortResponse> response = preferences.Select(x => new PromoCodeShortResponse() {
            Id = x.Id,
            Code = x.Code,
            BeginDate = x.BeginDate.ToString("yyyy-MM-dd"),
            EndDate = x.EndDate.ToString("yyyy-MM-dd"),
            PartnerName = x.PartnerName,
            ServiceInfo = x.ServiceInfo
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Создать промокод и выдать его клиентам с указанным предпочтением
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public Task<IActionResult> GivePromoCodesToCustomersWithPreferenceAsync(GivePromoCodeRequest request) =>
        //TODO: Создать промокод и выдать его клиентам с указанным предпочтением
        throw new NotImplementedException();
}