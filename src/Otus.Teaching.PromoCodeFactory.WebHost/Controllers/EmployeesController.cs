﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.Administration;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;

namespace Otus.Teaching.PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Сотрудники
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class EmployeesController
    : ControllerBase {
    private readonly IRepository<Employee> _employeeRepository;

    public EmployeesController(IRepository<Employee> employeeRepository) => _employeeRepository = employeeRepository;

    /// <summary>
    /// Получить данные всех сотрудников
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<EmployeeShortResponse>> GetEmployeesAsync() {
        IEnumerable<Employee> employees = await _employeeRepository.GetAllAsync();

        List<EmployeeShortResponse> employeesModelList = employees.Select(x =>
            new EmployeeShortResponse() {
                Id = x.Id,
                Email = x.Email,
                FullName = x.FullName,
            }).ToList();

        return employeesModelList;
    }

    /// <summary>
    /// Получить данные сотрудника по id
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeResponse>> GetEmployeeByIdAsync(Guid id) {
        Employee employee = await _employeeRepository.GetByIdAsync(id);

        if (employee == null)
            return NotFound();

        EmployeeResponse employeeModel = new() {
            Id = employee.Id,
            Email = employee.Email,
            Role = new RoleItemResponse() {
                Id = employee.Id,
                Name = employee.Role.Name,
                Description = employee.Role.Description
            },
            FullName = employee.FullName,
            AppliedPromocodesCount = employee.AppliedPromocodesCount
        };

        return employeeModel;
    }
}