using System.Threading.Tasks;
using Agience.Authority.Identity.Controllers.Manage;
using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Models.Manage;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace Agience.Authority.Identity.Tests.Controllers.Manage
{
    public class PersonControllerTests
    {
        private readonly Mock<IAgienceDataAdapter> _mockDataAdapter = new();
        private readonly Mock<ILogger<AgentController>> _mockLogger = new();
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly PersonController _controller;

        public PersonControllerTests()
        {
            _controller = new PersonController(_mockDataAdapter.Object, _mockLogger.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetPerson_ReturnsPerson()
        {
            // Arrange
            var personId = "testId";
            var modelPerson = new Models.Person { Id = personId };
            var person = new Person { Id = personId };
            _mockDataAdapter.Setup(x => x.GetRecordByIdAsPersonAsync<Models.Person>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(modelPerson);
            _mockMapper.Setup(x => x.Map<Person>(It.IsAny<Models.Person>())).Returns(person);

            // Act
            var result = await _controller.GetPerson();

            // Assert
            result.Should().BeOfType<ActionResult<Person>>();
            var actionResult = result as ActionResult<Person>;
            actionResult.Value.Should().BeEquivalentTo(person);
        }

        [Fact]
        public async Task PutPerson_UpdatesPersonSuccessfully()
        {
            // Arrange
            var person = new Person { Id = "testId" };
            _mockDataAdapter.Setup(x => x.UpdateRecordAsPersonAsync(It.IsAny<Models.Person>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockMapper.Setup(x => x.Map<Models.Person>(It.IsAny<Person>())).Returns(new Models.Person());

            // Act
            var result = await _controller.PutPerson(person);

            // Assert
            result.Should().BeOfType<OkResult>();
            _mockDataAdapter.Verify(x => x.UpdateRecordAsPersonAsync(It.IsAny<Models.Person>(), It.IsAny<string>()), Times.Once);
        }
    }
}
