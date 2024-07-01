using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Controllers;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using Xunit;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.WebHost.Controllers.Partners;

public class SetPartnerPromoCodeLimitAsyncTests {
    private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
    private readonly Fixture _fixture;
    private readonly PartnersController _controller;

    public SetPartnerPromoCodeLimitAsyncTests() {
        _fixture = new Fixture();
        // там круговые ссылки получаются, надо их игнорить
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _partnersRepositoryMock = new Mock<IRepository<Partner>>();
        _controller = new PartnersController(_partnersRepositoryMock.Object);
    }

    // Если партнер не найден, то также нужно выдать ошибку 404
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_PartnerNotFound_ReturnsNotFound() {
        Guid partnerId = Guid.NewGuid();
        _partnersRepositoryMock
            .Setup(repo => repo.GetByIdAsync(partnerId))
            .ReturnsAsync((Partner)null);

        SetPartnerPromoCodeLimitRequest request = _fixture
            .Create<SetPartnerPromoCodeLimitRequest>();

        IActionResult result = await _controller.SetPartnerPromoCodeLimitAsync(partnerId, request);

        result
            .Should().BeOfType<NotFoundResult>();
    }

    // Если партнер заблокирован, то есть поле IsActive=false в классе Partner, то также нужно выдать ошибку 400
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_PartnerBlocked_ReturnsBadRequest() {
        Partner partner = _fixture
            .Build<Partner>()
            .With(p => p.IsActive, false)
            .Create();
        _partnersRepositoryMock
            .Setup(repo => repo.GetByIdAsync(partner.Id))
            .ReturnsAsync(partner);

        SetPartnerPromoCodeLimitRequest request = _fixture
            .Create<SetPartnerPromoCodeLimitRequest>();

        IActionResult result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

        result
            .Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Данный партнер не активен");
    }

    // Если партнеру выставляется лимит, то мы должны обнулить количество промокодов, которые партнер выдал NumberIssuedPromoCodes, если лимит закончился, то количество не обнуляется
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_SettingLimit_ResetsIssuedPromoCodesIfLimitEnded() {
        Partner partner = _fixture
            .Build<Partner>()
            .With(p => p.IsActive, true)
            .With(p => p.NumberIssuedPromoCodes, 10)
            .Create();
        PartnerPromoCodeLimit activeLimit = _fixture
            .Build<PartnerPromoCodeLimit>()
            .With(l => l.CancelDate, (DateTime?)null)
            .Create();
        partner.PartnerLimits.Add(activeLimit);

        _partnersRepositoryMock
            .Setup(repo => repo.GetByIdAsync(partner.Id))
            .ReturnsAsync(partner);

        SetPartnerPromoCodeLimitRequest request = _fixture
            .Build<SetPartnerPromoCodeLimitRequest>()
            .With(r => r.Limit, 5)
            .Create();

        IActionResult result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

        partner.NumberIssuedPromoCodes
            .Should().Be(0);
        _partnersRepositoryMock
            .Verify(repo => repo.UpdateAsync(partner), Times.Once);
        result
            .Should().BeOfType<CreatedAtActionResult>();
    }

    // При установке лимита нужно отключить предыдущий лимит
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_SettingLimit_DisablesPreviousLimit() {
        Partner partner = _fixture
            .Build<Partner>()
            .With(p => p.IsActive, true)
            .Create();
        PartnerPromoCodeLimit activeLimit = _fixture
            .Build<PartnerPromoCodeLimit>()
            .With(l => l.CancelDate, (DateTime?)null)
            .Create();
        partner.PartnerLimits.Add(activeLimit);

        _partnersRepositoryMock
            .Setup(repo => repo.GetByIdAsync(partner.Id))
            .ReturnsAsync(partner);

        SetPartnerPromoCodeLimitRequest request = _fixture
            .Build<SetPartnerPromoCodeLimitRequest>()
            .With(r => r.Limit, 5)
            .Create();

        IActionResult result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

        activeLimit.CancelDate
            .Should().NotBeNull();
        _partnersRepositoryMock
            .Verify(repo => repo.UpdateAsync(partner), Times.Once);
        result
            .Should().BeOfType<CreatedAtActionResult>();
    }

    // Лимит должен быть больше 0
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_LimitLessThanOrEqualToZero_ReturnsBadRequest() {
        Partner partner = _fixture
            .Build<Partner>()
            .With(p => p.IsActive, true)
            .Create();
        _partnersRepositoryMock
            .Setup(repo => repo.GetByIdAsync(partner.Id))
            .ReturnsAsync(partner);

        SetPartnerPromoCodeLimitRequest request = _fixture
            .Build<SetPartnerPromoCodeLimitRequest>()
            .With(r => r.Limit, 0)
            .Create();

        IActionResult result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

        result
            .Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Лимит должен быть больше 0");
    }

    // Нужно убедиться, что сохранили новый лимит в базу данных (это нужно проверить Unit-тестом)
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_SavesNewLimitInDatabase() {
        Partner partner = _fixture
            .Build<Partner>()
            .With(p => p.IsActive, true)
            .Create();
        PartnerPromoCodeLimit activeLimit = _fixture
            .Build<PartnerPromoCodeLimit>()
            .With(l => l.CancelDate, (DateTime?)null)
            .Create();
        partner.PartnerLimits.Add(activeLimit);

        _partnersRepositoryMock
            .Setup(repo => repo.GetByIdAsync(partner.Id))
            .ReturnsAsync(partner);

        SetPartnerPromoCodeLimitRequest request = _fixture.Build<SetPartnerPromoCodeLimitRequest>()
            .With(r => r.Limit, 5)
            .Create();

        IActionResult result = await _controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

        _partnersRepositoryMock
            .Verify(repo => repo.UpdateAsync(It.IsAny<Partner>()), Times.Once);
        result
            .Should().BeOfType<CreatedAtActionResult>();
    }
}
