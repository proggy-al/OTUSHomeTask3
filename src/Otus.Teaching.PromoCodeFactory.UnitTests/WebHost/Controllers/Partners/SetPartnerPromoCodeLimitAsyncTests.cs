using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Controllers;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests : IClassFixture<TestFixture>
    {
        private Mock<IRepository<Partner>> _partnersRepositoryMock =
            new Mock<IRepository<Partner>>();

        private PartnersController _partnersController;

        public SetPartnerPromoCodeLimitAsyncTests(TestFixture testFixture)
        {
            _partnersController = new PartnersController(_partnersRepositoryMock.Object);
        }

        /// <summary>
        /// Если партнер не найден, то выдать ошибку 404.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Partner_IfNotExists_ReturnNotFound()
        {
            //Arrange
            var newId = Guid.NewGuid();
            var partner = _partnersRepositoryMock.Setup(m => m.GetByIdAsync(newId));

            //Act
            var result = await _partnersController.GetPartnerLimitAsync(newId, Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Если партнер не найден, то выдать ошибку 400.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Partner_IfNotActive_ReturnBadRequest()
        {
            //Arrange
            var partner = new PartnerBuilder()
                .WithCreatedParthnerId(Guid.NewGuid())
                .WithCreatedIsActive(false)
                .Build();
            _partnersRepositoryMock.Setup(m => m.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            //Act
            var result = await _partnersController.GetPartnerLimitAsync(partner.Id, Guid.NewGuid());

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// При выставлении лимита обнуляем NumberIssuedPromoCodes.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Partner_NumberIssuedPromoCodes_is_0_WhenAddLimit_And_CancelDate_is_null()
        {
            //Arrange
            var autoFixture = new Fixture();
            var partner = new PartnerBuilder()
                .WithCreatedParthnerId(Guid.NewGuid())
                .WithCreatedIsActive(true)
                .WithCreatedNumberIssuedPromoCodes(1)
                .WithCreatedPartnerLimits([
                    new PartnerPromoCodeLimit
                    {
                        Id = Guid.NewGuid(),
                        CreateDate = autoFixture.Create<DateTime>().Date,
                        CancelDate = null,
                        EndDate = autoFixture.Create<DateTime>().Date,
                        Limit = autoFixture.Create<Int16>()
                    }
                ])
                .Build();
            _partnersRepositoryMock.Setup(x => x.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var request = autoFixture.Create<SetPartnerPromoCodeLimitRequest>();

            //Act
            var result =
                await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            //Assert
            partner.NumberIssuedPromoCodes.Should().Be(0);
        }

        /// <summary>
        /// При выставлении лимита если лимит закончился, то не обнуляем NumberIssuedPromoCodes.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Partner_NumberIssuedPromoCodes_not_changed_WhenAddLimit_And_CancelDate_is_null()
        {
            //Arrange
            var autoFixture = new Fixture();
            var partner = new PartnerBuilder()
                .WithCreatedParthnerId(Guid.NewGuid())
                .WithCreatedIsActive(true)
                .WithCreatedNumberIssuedPromoCodes(5)
                .WithCreatedPartnerLimits([
                    new PartnerPromoCodeLimit
                    {
                        Id = Guid.NewGuid(),
                        CreateDate = autoFixture.Create<DateTime>().Date,
                        CancelDate = autoFixture.Create<DateTime>().Date,
                        EndDate = autoFixture.Create<DateTime>().Date,
                        Limit = autoFixture.Create<Int16>()
                    }
                ])
                .Build();
            _partnersRepositoryMock.Setup(x => x.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var request = autoFixture.Create<SetPartnerPromoCodeLimitRequest>();

            //Act
            var result =
                await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            //Assert
            partner.NumberIssuedPromoCodes.Should().Be(5);
        }

        /// <summary>
        /// При установке лимита отключается предыдущий лимит.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Partner_CancelDate_changed_WhenAddLimit()
        {
            var autoFixture = new Fixture();
            Guid idPartner = Guid.NewGuid();
            Guid idLimit = Guid.NewGuid();
            var partner = new PartnerBuilder()
                .WithCreatedParthnerId(idPartner)
                .WithCreatedIsActive(true)
                .WithCreatedNumberIssuedPromoCodes(1)
                .WithCreatedPartnerLimits([
                    new PartnerPromoCodeLimit
                    {
                        Id = idLimit,
                        CreateDate = autoFixture.Create<DateTime>().Date,
                        CancelDate = null,
                        EndDate = autoFixture.Create<DateTime>().Date,
                        Limit = 25
                    }
                ])
                .Build();
            _partnersRepositoryMock.Setup(x => x.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var request = autoFixture.Create<SetPartnerPromoCodeLimitRequest>();

            //Act
            var result =
                await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            //Assert
            partner.PartnerLimits.Where(x => x.Id == idLimit).First().CancelDate.Value.Date.Should().Be(DateTime.Now.Date);
        }

        /// <summary>
        /// Лимит должен быть больше 0.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Partner_Check_Limit_WhenLimit_Less_Then_1_ReturnBadRequest()
        {
            var autoFixture = new Fixture();
            Guid idPartner = Guid.NewGuid();
            Guid idLimit = Guid.NewGuid();
            var partner = new PartnerBuilder()
                .WithCreatedParthnerId(idPartner)
                .WithCreatedIsActive(true)
                .WithCreatedNumberIssuedPromoCodes(1)
                .WithCreatedPartnerLimits([
                    new PartnerPromoCodeLimit
                    {
                        Id = idLimit,
                        CreateDate = autoFixture.Create<DateTime>().Date,
                        CancelDate = null,
                        EndDate = autoFixture.Create<DateTime>().Date,
                        Limit = autoFixture.Create<Int16>()
                    }
                ])
                .Build();
            _partnersRepositoryMock.Setup(x => x.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var request = new SetPartnerPromoCodeLimitRequest
            {
                EndDate = DateTime.Now,
                Limit = 0
            };

            //Act
            var result =
                await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Сохранение нового лимита в БД.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Partner_Add_Limit_To_DB_Returns_Success()
        {
            var autoFixture = new Fixture();
            Guid idPartner = Guid.NewGuid();
            Guid idLimit = Guid.NewGuid();
            var partner = new PartnerBuilder()
                .WithCreatedParthnerId(idPartner)
                .WithCreatedIsActive(true)
                .WithCreatedNumberIssuedPromoCodes(1)
                .WithCreatedPartnerLimits([
                    new PartnerPromoCodeLimit
                    {
                        Id = idLimit,
                        CreateDate = autoFixture.Create<DateTime>().Date,
                        CancelDate = null,
                        EndDate = autoFixture.Create<DateTime>().Date,
                        Limit = autoFixture.Create<Int16>()
                    }
                ])
                .Build();
            _partnersRepositoryMock.Setup(x => x.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            var request = new SetPartnerPromoCodeLimitRequest
            {
                EndDate = DateTime.Now,
                Limit = 10
            };

            var saveLimit = _partnersRepositoryMock.Setup(x => x.UpdateAsync(partner));          

            //Act
            var result =
                await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            //Assert
            _partnersRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Partner>()), Times.Exactly(1));            
        }
    }
}