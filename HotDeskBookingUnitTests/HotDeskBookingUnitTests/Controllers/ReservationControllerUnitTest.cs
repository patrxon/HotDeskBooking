using Moq;
using Xunit;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using HotDeskBooking.Controllers;
using HotDeskBooking.Models;
using HotDeskBooking.Data;

namespace ReservationSystem.Tests;

public class ReservationControllerUnitTest
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly ReservationsController _controller;

    public ReservationControllerUnitTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "ReservationDb")
            .Options;

        _context = new ApplicationDbContext(options);

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "ThisUser"),
            new Claim(ClaimTypes.Role, "Employee"),
        }, "mock"));

        var httpContext = new DefaultHttpContext();
        httpContext.User = userClaims;

        var controllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(userClaims);

        _controller = new ReservationsController(_context, _httpContextAccessorMock.Object);

        _controller.ControllerContext = controllerContext;

        SeedTestData();
    }

    private void SeedTestData()
    {
        

        _context.Desks.AddRange(new List<Desk>
        {
            new Desk { Id = 1, LocationId = 1 },
            new Desk { Id = 2, LocationId = 1 }
        });

        _context.Reservations.Add(new Reservation
        {
            Id = 1,
            UserId = "OtherUser",
            DeskId = 2,
            StartDate = new System.DateTime(2024, 10, 15),
            EndDate = new System.DateTime(2024, 10, 16)
        });

        _context.SaveChanges();
    }

    [Fact]
    public void GetAvailableDesks_ReturnsAvailableDesks_ForEmployee()
    {
        // Act
        var result = _controller.GetAvailableDesks(new System.DateTime(2024, 10, 15), null) as OkObjectResult;

        // Assert
        Assert.NotNull(result);

        var response = result.Value as List<Desk>;
       
        Assert.NotNull(response);
        Assert.Single(response);
        Assert.Equal(1, response.First().Id);
    }

    [Fact]
    public void GetAvailableDesks_ReturnsDetailedInfo_ForAdmin()
    {
        // Mock admin user
        var adminClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
        new Claim(ClaimTypes.NameIdentifier, "adminUserId"),
        new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(adminClaims);

        // Act
        var result = _controller.GetAvailableDesks(new System.DateTime(2024, 10, 17), null) as OkObjectResult;

        // Assert
        Assert.NotNull(result);

        var response = result.Value as List<Desk>;

        Assert.NotNull(response);
        Assert.Equal(2, response[1].Id);
        Assert.Equal("OtherUser", response[1].Reservations.First().UserId);
    }
}