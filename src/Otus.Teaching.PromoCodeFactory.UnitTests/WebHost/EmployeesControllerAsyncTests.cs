using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.Administration;
using Otus.Teaching.PromoCodeFactory.WebHost.Controllers;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using Xunit;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.WebHost
{
    public class EmployeesControllerAsyncTests
    {
        private readonly Mock<IRepository<Employee>> _employeesRepositoryMock;
        private readonly EmployeesController _employeesController;

        public EmployeesControllerAsyncTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _employeesRepositoryMock = fixture.Freeze<Mock<IRepository<Employee>>>();
            //Before creating the object, the Build method can be used to add one-time customizations to be used for the creation of the next variable.
            _employeesController = fixture.Build<EmployeesController>()
                //The With construct allows the customization of writeable properties and public fields.
                //.With(x => x.CurrentDateTime, () => specificDateTime)
                .OmitAutoProperties()
                .Create();
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_EmployeeIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var employeeId = Guid.Parse("451533d5-d8d5-4a11-9c7b-eb9f14e1a32f");//random id
            Employee employee = null;

            _employeesRepositoryMock.Setup(repo => repo.GetByIdAsync(employeeId))
                .ReturnsAsync(employee);

            // Act
            var result = (await _employeesController.GetEmployeeByIdAsync(employeeId)).Result;

            // Assert
            result.Should().BeAssignableTo<NotFoundResult>();
        }

        [Theory, AutoData]
        public async void GetEmployeeByIdAsync_EmployeeIsFound_ReturnsEntityEmployee(Employee employee)//random employee
        {
            _employeesRepositoryMock.Setup(repo => repo.GetByIdAsync(employee.Id))
                .ReturnsAsync(employee);

            // Act
            var result = (await _employeesController.GetEmployeeByIdAsync(employee.Id)).Value;

            // Assert
            result.Should().BeAssignableTo<EmployeeResponse>();
        }
        /* 
                //Если партнеру выставляется лимит, то мы должны обнулить количество промокодов, которые партнер выдал NumberIssuedPromoCodes,
                //При установке лимита нужно отключить предыдущий лимит
                //[Fact]
                [Theory, AutoData]
                public async void SetEmployeePromoCodeLimit_SetNewLimit_ReturnsZeroPromocodesAndCancelDate(SetEmployeePromoCodeLimitRequest employeeLimitRequest)
                {
                    // Arrange
                    var employee = CreateBaseEmployee("7d994823-8226-4273-b063-1a95f3cc1df8");
                    employee.NumberIssuedPromoCodes = 100;
                    //спрятать в autofixture
                    _employeesRepositoryMock.Setup(repo => repo.GetByIdAsync(employee.Id))
                        .ReturnsAsync(employee);

                    // Act
                    var result = (await _employeesController.SetEmployeePromoCodeLimitAsync(employee.Id, employeeLimitRequest) as CreatedAtActionResult).Value as Employee;

                    // Assert
                    result.Should().BeAssignableTo<Employee>();
                    result.EmployeeLimits.Select(x => x.CancelDate).Should().Contain(specificDateTime);
                    result.NumberIssuedPromoCodes.Should().Be(0);

                }
                // если лимит закончился, то количество не обнуляется;
                [Theory, AutoData]
                public async void SetEmployeePromoCodeLimit_SetNewLimit_ReturnsNotZeroPromocodes(SetEmployeePromoCodeLimitRequest employeeLimitRequest)
                {
                    // Arrange
                    var employee = CreateBaseEmployee("7d994823-8226-4273-b063-1a95f3cc1df8", DateTime.UtcNow);

                    var expectedPromocedes = employee.NumberIssuedPromoCodes = 100;
                    _employeesRepositoryMock.Setup(repo => repo.GetByIdAsync(employee.Id))
                        .ReturnsAsync(employee);

                    // Act
                    var result = (await _employeesController.SetEmployeePromoCodeLimitAsync(employee.Id, employeeLimitRequest) as CreatedAtActionResult).Value as Employee;

                    // Assert
                    result.Should().BeAssignableTo<Employee>();
                    result.NumberIssuedPromoCodes.Should().Be(expectedPromocedes);
                }


                //Лимит должен быть больше 0;
                [Theory, AutoData]
                public async void SetEmployeePromoCodeLimit_SetNewLimitLessZero_ReturnsBadRequest(SetEmployeePromoCodeLimitRequest employeeLimitRequest)
                {
                    // Arrange
                    var employee = CreateBaseEmployee("7d994823-8226-4273-b063-1a95f3cc1df8");
                    employeeLimitRequest.Limit = -1;
                    _employeesRepositoryMock.Setup(repo => repo.GetByIdAsync(employee.Id))
                        .ReturnsAsync(employee);

                    // Act
                    var result = await _employeesController.SetEmployeePromoCodeLimitAsync(employee.Id, employeeLimitRequest);

                    //Assert
                    result.Should().BeAssignableTo<BadRequestObjectResult>();
                }


                //Нужно убедиться, что сохранили новый лимит в базу данных (это нужно проверить Unit-тестом);
                [Theory, AutoData]
                public async void SetEmployeePromoCodeLimit_SetNewLimit_VerifyUpdateEmployee(SetEmployeePromoCodeLimitRequest employeeLimitRequest)
                {
                    // Arrange
                    var employee = CreateBaseEmployee("7d994823-8226-4273-b063-1a95f3cc1df8");
                    employeeLimitRequest.Limit = 100;
                    _employeesRepositoryMock.Setup(repo => repo.GetByIdAsync(employee.Id))
                        .ReturnsAsync(employee);

                    // Act
                    await _employeesController.SetEmployeePromoCodeLimitAsync(employee.Id, employeeLimitRequest);

                    // Assert
                    _employeesRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Employee>()), Times.Once);
                }


                public Employee CreateBaseEmployee(string id = "def47943-7aaf-44a1-ae21-05aa4948b165", DateTime? cancelDate = null)
                {
                    var employee = new Employee()
                    {
                        Id = Guid.Parse(id),
                        Name = "Суперигрушки",
                        IsActive = true,
                        EmployeeLimits = new List<EmployeePromoCodeLimit>()
                        {
                            new EmployeePromoCodeLimit()
                            {
                                Id = Guid.Parse("e00633a5-978a-420e-a7d6-3e1dab116393"),
                                CreateDate = new DateTime(2020, 07, 9),
                                EndDate = new DateTime(2020, 10, 9),
                                Limit = 100,
                                CancelDate= cancelDate
                            }
                        }
                    };

                    return employee;
                }*/
    }
}