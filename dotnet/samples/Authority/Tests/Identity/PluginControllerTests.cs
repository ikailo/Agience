using System.Collections.Generic;
using System.Threading.Tasks;
using Agience.Authority.Identity.Controllers.Manage;
using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Models.Manage;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Agience.Authority.Identity.Tests.Controllers.Manage
{
    public class PluginControllerTests
    {
        private readonly Mock<IAgienceDataAdapter> _mockDataAdapter = new();
        //private readonly Mock<SDK.Authority> _mockAuthority = new();
        private readonly Mock<ILogger<PluginController>> _mockLogger = new();
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly PluginController _controller;

        public PluginControllerTests()
        {
            _controller = new PluginController(_mockDataAdapter.Object,/* _mockAuthority.Object, */_mockLogger.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetPlugins_ReturnsPlugins()
        {
            // Arrange
            var plugins = new List<Models.Plugin> { new Models.Plugin { Id = "testId" } };
            _mockDataAdapter.Setup(x => x.GetRecordsAsPersonAsync<Models.Plugin>(It.IsAny<string>(), false))
                .ReturnsAsync(plugins);
            _mockMapper.Setup(x => x.Map<IEnumerable<Plugin>>(It.IsAny<IEnumerable<Models.Plugin>>()))
                .Returns(new List<Plugin> { new Plugin { Id = "testId" } });

            // Act
            var result = await _controller.GetPlugins();

            // Assert
            var actionResult = result.Result as OkObjectResult;
            var value = actionResult.Value as IEnumerable<Plugin>;
            value.Should().NotBeNull();
            value.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetPlugin_ReturnsPlugin()
        {
            // Arrange
            var plugin = new Models.Plugin { Id = "testId" };
            _mockDataAdapter.Setup(x => x.GetRecordByIdAsPersonAsync<Models.Plugin>(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(plugin);
            _mockMapper.Setup(x => x.Map<Plugin>(It.IsAny<Models.Plugin>()))
                .Returns(new Plugin { Id = "testId" });

            // Act
            var result = await _controller.GetPlugin("testId");

            // Assert
            var actionResult = result.Result as OkObjectResult;
            var value = actionResult.Value as Plugin;
            value.Should().NotBeNull();
            value.Id.Should().Be("testId");
        }

        [Fact]
        public async Task PostPlugin_CreatesPlugin()
        {
            // Arrange
            //_mockDataAdapter.Setup(x => x.CreateRecordAsPersonAsync(It.IsAny<Models.Plugin>(), It.IsAny<string>()))
            //    .ReturnsAsync("createdId");
            _mockMapper.Setup(x => x.Map<Models.Plugin>(It.IsAny<Plugin>()))
                .Returns(new Models.Plugin());

            // Act
            var result = await _controller.PostPlugin(new Plugin());

            // Assert
            //var actionResult = result.Result as CreatedAtActionResult;
            //var value = actionResult.Value as string;
           // value.Should().NotBeNull();
            //value.Should().Be("createdId");
        }

        [Fact]
        public async Task PutPlugin_UpdatesPlugin()
        {
            // Arrange
            _mockDataAdapter.Setup(x => x.UpdateRecordAsPersonAsync(It.IsAny<Models.Plugin>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PutPlugin(new Plugin());

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task DeletePlugin_DeletesPlugin()
        {
            // Arrange
            _mockDataAdapter.Setup(x => x.DeleteRecordAsPersonAsync<Models.Plugin>(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeletePlugin("testId");

            // Assert
            result.Should().BeOfType<OkResult>();
        }
    }
}
