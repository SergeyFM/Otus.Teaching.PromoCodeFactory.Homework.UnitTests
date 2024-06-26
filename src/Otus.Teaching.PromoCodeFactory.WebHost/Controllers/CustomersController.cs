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
/// Клиенты
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController
    : ControllerBase {
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Preference> _preferenceRepository;

    public CustomersController(IRepository<Customer> customerRepository,
        IRepository<Preference> preferenceRepository) {
        _customerRepository = customerRepository;
        _preferenceRepository = preferenceRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerShortResponse>>> GetCustomersAsync() {
        IEnumerable<Customer> customers = await _customerRepository.GetAllAsync();

        List<CustomerShortResponse> response = customers.Select(x => new CustomerShortResponse() {
            Id = x.Id,
            Email = x.Email,
            FirstName = x.FirstName,
            LastName = x.LastName
        }).ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomerAsync(Guid id) {
        Customer customer = await _customerRepository.GetByIdAsync(id);

        CustomerResponse response = new(customer);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> CreateCustomerAsync(CreateOrEditCustomerRequest request) {
        //Получаем предпочтения из бд и сохраняем большой объект
        IEnumerable<Preference> preferences = await _preferenceRepository
            .GetRangeByIdsAsync(request.PreferenceIds);

        Customer customer = new() {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };
        customer.Preferences = preferences.Select(x => new CustomerPreference() {
            Customer = customer,
            Preference = x
        }).ToList();

        await _customerRepository.AddAsync(customer);

        return CreatedAtAction(nameof(GetCustomerAsync), new { id = customer.Id }, null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> EditCustomersAsync(Guid id, CreateOrEditCustomerRequest request) {
        Customer customer = await _customerRepository.GetByIdAsync(id);

        if (customer == null)
            return NotFound();

        IEnumerable<Preference> preferences = await _preferenceRepository.GetRangeByIdsAsync(request.PreferenceIds);

        customer.Email = request.Email;
        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Preferences.Clear();
        customer.Preferences = preferences.Select(x => new CustomerPreference() {
            Customer = customer,
            Preference = x
        }).ToList();

        await _customerRepository.UpdateAsync(customer);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomerAsync(Guid id) {
        Customer customer = await _customerRepository.GetByIdAsync(id);

        if (customer == null)
            return NotFound();

        await _customerRepository.DeleteAsync(customer);

        return NoContent();
    }
}